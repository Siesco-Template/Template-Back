using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using ImportExportComponent.Configuration;
using ImportExportComponent.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QueryGenerator.Extensions;
using ImportExportComponent.Entities;
using ImportExportComponent.HelperServices;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Globalization;
using System.Linq.Expressions;
using System.Threading;

namespace ImportExportComponent.Services
{
    public class ExportService
    {
        private readonly DownloadItemService _downloadListService;
        private readonly IWebHostEnvironment _environment;
        private readonly ImportExportOptions _options;
        private readonly ILogger<ExportService> _logger;
        private readonly int _batchSize;
        private readonly ConcurrentDictionary<(Type ElementType, string PropertyName), Func<object, object?>> _getterCache = new();

        public ExportService(
            DownloadItemService downloadListService,
            IWebHostEnvironment environment,
            ImportExportOptions options,
            ILogger<ExportService> logger)
        {
            _downloadListService = downloadListService;
            _environment = environment;
            _options = options;
            _logger = logger;

            _batchSize = Math.Max(1, options.ExportBatchSize);
        }

        public async Task<DownloadItem> ExportAndNotifyAsync(
            IQueryable<dynamic> query,
            IReadOnlyList<ExportColumnDto> columns,
            string tableId,
            string userId,
            string? downloadItemId = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentNullException.ThrowIfNull(columns);
            if (columns.Count == 0) throw new ArgumentException("At least one column must be provided for export.", nameof(columns));
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("UserId cannot be null or whitespace.", nameof(userId));

            var safeTableId = string.IsNullOrWhiteSpace(tableId) ? "Export" : tableId;
            var exportsRoot = EnsureExportDirectory();
            var fileName = BuildFileName(safeTableId);
            var filePath = Path.Combine(exportsRoot, fileName);

            _logger.LogInformation("Starting export for table {TableId} into {FilePath}", safeTableId, filePath);

            try
            {
                // Get total count for progress tracking
                var totalCount = await query.CountAsync(cancellationToken);

                await CreateSpreadsheetAsync(filePath, query, columns, userId, totalCount, cancellationToken);

                var downloadItem = await PersistDownloadItemAsync(fileName, userId, downloadItemId, cancellationToken);

                _logger.LogInformation("Export completed for table {TableId}. File available at {FileUrl}", safeTableId, downloadItem.FileUrl);
                return downloadItem;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Export failed for table {TableId} and user {UserId}", safeTableId, userId);
                throw;
            }
        }

        private async Task CreateSpreadsheetAsync(
            string filePath,
            IQueryable<dynamic> baseQuery,
            IReadOnlyList<ExportColumnDto> columns,
            string userId,
            int totalCount,
            CancellationToken cancellationToken)
        {
            // Stream'i ReadWrite modunda aç - SpreadsheetDocument hem okuma hem yazma gerektirir
            await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read, 64 * 1024, useAsync: true);
            using var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook);
            var workbookPart = document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();
            var sheets = workbookPart.Workbook.AppendChild(new Sheets());

            var elementType = baseQuery.ElementType ?? typeof(object);
            var getters = BuildAccessors(elementType, columns);

            int sheetCounter = 1;
            int skip = 0;
            int processedCount = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                var batch = await baseQuery
                    .Skip(skip)
                    .Take(_batchSize)
                    .ToListAsync(cancellationToken);

                if (batch.Count == 0)
                {
                    break;
                }

                try
                {
                    var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    sheets.Append(new Sheet
                    {
                        Id = workbookPart.GetIdOfPart(worksheetPart),
                        SheetId = (uint)sheetCounter,
                        Name = $"Sheet_{sheetCounter}"
                    });

                    using var writer = OpenXmlWriter.Create(worksheetPart);
                    writer.WriteStartElement(new Worksheet());
                    writer.WriteStartElement(new SheetData());

                    WriteHeader(writer, columns);
                    WriteData(writer, batch, columns, getters, cancellationToken);

                    writer.WriteEndElement(); // SheetData
                    writer.WriteEndElement(); // Worksheet
                    writer.Close();

                    processedCount += batch.Count;
                    sheetCounter++;
                    skip += batch.Count;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing batch starting at {Skip} for export", skip);
                    throw;
                }
            }

            workbookPart.Workbook.Save();
        }

        private IReadOnlyDictionary<string, Func<object, object?>> BuildAccessors(Type elementType, IReadOnlyList<ExportColumnDto> columns)
        {
            var accessors = new Dictionary<string, Func<object, object?>>(columns.Count, StringComparer.OrdinalIgnoreCase);

            foreach (var column in columns)
            {
                var key = (elementType, column.PropertyName);
                var accessor = _getterCache.GetOrAdd(key, cacheKey =>
                {
                    var parameter = Expression.Parameter(typeof(object), "instance");
                    var castedParameter = Expression.Convert(parameter, elementType);
                    var body = IgnoreCaseHelper.BuildSafePropertyAccessExpression(castedParameter, column.PropertyName, out _);

                    if (body == null)
                    {
                        return _ => null;
                    }

                    var convert = Expression.Convert(body, typeof(object));
                    return Expression.Lambda<Func<object, object?>>(convert, parameter).Compile();
                });

                accessors[column.PropertyName] = accessor;
            }

            return accessors;
        }

        private static void WriteHeader(OpenXmlWriter writer, IReadOnlyList<ExportColumnDto> columns)
        {
            writer.WriteStartElement(new Row());
            foreach (var column in columns)
            {
                writer.WriteElement(new Cell
                {
                    DataType = CellValues.String,
                    CellValue = new CellValue(string.IsNullOrWhiteSpace(column.Header) ? column.PropertyName : column.Header)
                });
            }
            writer.WriteEndElement(); // Row
        }

        private void WriteData(
            OpenXmlWriter writer,
            IReadOnlyList<dynamic> batch,
            IReadOnlyList<ExportColumnDto> columns,
            IReadOnlyDictionary<string, Func<object, object?>> getters,
            CancellationToken cancellationToken)
        {
            foreach (var item in batch)
            {
                cancellationToken.ThrowIfCancellationRequested();

                writer.WriteStartElement(new Row());
                foreach (var column in columns)
                {
                    var rawValue = getters[column.PropertyName](item);
                    var formatted = FormatCellValue(rawValue);

                    writer.WriteElement(new Cell
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(formatted)
                    });
                }
                writer.WriteEndElement(); // Row
            }
        }

        private static string? FormatCellValue(object? value)
        {
            switch (value)
            {
                case null:
                    return null;
                case Enum enumValue:
                    return EnumDisplayHelper.GetEnumDisplayName(enumValue.GetType(), enumValue);
                case DateTime dateTime:
                    return dateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                case DateOnly dateOnly:
                    return dateOnly.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                case TimeOnly timeOnly:
                    return timeOnly.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
                default:
                    return Convert.ToString(value, CultureInfo.InvariantCulture);
            }
        }

        private async Task<DownloadItem> PersistDownloadItemAsync(string fileName, string userId, string? downloadItemId = null, CancellationToken cancellationToken = default)
        {
            var downloadUrl = BuildDownloadUrl(fileName);

            if (!string.IsNullOrWhiteSpace(downloadItemId))
            {
                // Update existing DownloadItem
                await _downloadListService.UpdateFileInfoAsync(downloadItemId, fileName, downloadUrl, cancellationToken);
                
                // Get updated item
                var updatedItem = await _downloadListService.GetByIdAsync(downloadItemId, cancellationToken);
                if (updatedItem != null)
                {
                    return updatedItem;
                }
            }

            // Create new DownloadItem
            var downloadItem = new DownloadItem
            {
                FileName = fileName,
                FileUrl = downloadUrl,
                Extension = Path.GetExtension(fileName),
                CreatedAt = DateTime.UtcNow.AddHours(4),
                UserId = userId,
                Status = "completed"
            };

            await _downloadListService.AddItem(downloadItem, cancellationToken);
            return downloadItem;
        }

        private string EnsureExportDirectory()
        {
            var webRoot = _environment.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRoot))
            {
                webRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
            }

            var exportPath = Path.Combine(webRoot, _options.ExportFolderName);
            Directory.CreateDirectory(exportPath);
            return exportPath;
        }

        private string BuildDownloadUrl(string fileName)
        {
            var baseUrl = _options.BaseUrl;
            var relativePath = $"{_options.ExportFolderName}/{fileName}";

            return string.IsNullOrWhiteSpace(baseUrl)
                ? $"/{relativePath.Replace("\\", "/")}"
                : $"{baseUrl.TrimEnd('/')}/{relativePath.Replace("\\", "/")}";
        }

        private static string BuildFileName(string tableId)
        {
            var normalized = string.Concat(tableId.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
            if (string.IsNullOrWhiteSpace(normalized))
            {
                normalized = "Export";
            }

            return $"{normalized}_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.xlsx";
        }
    }
}
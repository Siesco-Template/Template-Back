using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using ImportExportComponent.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QueryGenerator.Extensions;
using SharedLibrary.Entities;
using SharedLibrary.HelperServices;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using TableComponent.Extensions;

namespace ImportExportComponent.Services
{
    public class ExportService(DownloadItemService downloadListService, IHttpContextAccessor httpContextAccessor,
     GetQueryHelper getQueryHelper, IConfiguration configuration)
    {
        private readonly DownloadItemService _downloadListService = downloadListService;
        private readonly GetQueryHelper _getQueryHelper = getQueryHelper;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IConfiguration _configuration = configuration;
        public async Task ExportAndNotifyAsync(IQueryable<dynamic> query, List<ExportColumnDto> columns, string userId)
        {
            var tableName = "Reports";
            var fileId = Guid.NewGuid().ToString("N");
            var fileName = $"{tableName}_{fileId}.xlsx";

            var exportDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "exports");
            Directory.CreateDirectory(exportDir);

            var filePath = Path.Combine(exportDir, fileName);
            await ExportDataAsync(filePath, columns, query);

            var baseUrl = _configuration["BaseUrl"];

            var downloadItem = new DownloadItem
            {
                FileName = fileName,
                FileUrl = $"{baseUrl}/exports/{fileName}",
                Extension = ".xlsx",
                CreatedAt = DateTime.UtcNow.AddHours(4),
                UserId = userId,
            };

            await _downloadListService.AddItem(downloadItem);
        }

        public async Task ExportDataAsync(string filePath, List<ExportColumnDto> columns, IQueryable<dynamic> baseQuery)
        {
            var batchSize = 250_000;
            var getters = new Dictionary<string, Func<object, object?>>();
            var type = baseQuery.ElementType;

            using var doc = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook);
            var workbookPart = doc.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();
            var sheets = workbookPart.Workbook.AppendChild(new Sheets());

            int sheetCounter = 1;

            foreach (var col in columns)
            {
                //her setirdeki datani temsil eden parametr
                var param = Expression.Parameter(typeof(object), "x");
                //parametri querydeki tipe cast edir
                var castedParam = Expression.Convert(param, type);

                //gelen PropertyName catmaq ucun expression yaradilir
                var expr = IgnoreCaseHelper.BuildSafePropertyAccessExpression(castedParam, col.PropertyName, out var resultType);
                if (expr == null)
                {
                    getters[col.PropertyName] = _ => null;
                    continue;
                }

                var lambda = Expression.Lambda<Func<object, object?>>(
                    Expression.Convert(expr, typeof(object)), param
                ).Compile();

                // lambda expression getterse elave edilir
                getters[col.PropertyName] = lambda;
            }

            for (int i = 0; ; i++)
            {
                var batch = await baseQuery.Skip(i * batchSize).Take(batchSize).ToListAsync();
                if (batch.Count == 0) break;

                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                var sheetName = $"Sheet_{sheetCounter}";

                sheets.Append(new Sheet
                {
                    Id = workbookPart.GetIdOfPart(worksheetPart),
                    SheetId = (uint)(sheetCounter - 1),
                    Name = sheetName
                });

                using var writer = OpenXmlWriter.Create(worksheetPart);
                writer.WriteStartElement(new Worksheet());
                writer.WriteStartElement(new SheetData());

                WriteHeader(writer, columns);
                WriteData(writer, batch, columns, getters);

                writer.WriteEndElement(); // SheetData
                writer.WriteEndElement(); // Worksheet
                writer.Close();

                sheetCounter++;
            }

            workbookPart.Workbook.Save();
        }

        private void WriteHeader(OpenXmlWriter writer, List<ExportColumnDto> columns)
        {
            writer.WriteStartElement(new Row());
            foreach (var col in columns)
            {
                writer.WriteElement(new Cell
                {
                    DataType = CellValues.String,
                    CellValue = new CellValue(col.Header ?? col.PropertyName)
                });
            }
            writer.WriteEndElement();
        }

        private void WriteData(OpenXmlWriter writer, IEnumerable<dynamic> batch, List<ExportColumnDto> columns, Dictionary<string, Func<object, object?>> getters)
        {
            foreach (var item in batch)
            {
                writer.WriteStartElement(new Row());
                foreach (var col in columns)
                {
                    var value = getters[col.PropertyName](item);
                    if (value != null && value.GetType().IsEnum)
                        value = EnumDisplayHelper.GetEnumDisplayName(value.GetType(), value);

                    writer.WriteElement(new Cell
                    {
                        DataType = CellValues.String,
                        CellValue = new CellValue(value?.ToString())
                    });
                }
                writer.WriteEndElement();
            }
        }
    }
}
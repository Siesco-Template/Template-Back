using ClosedXML.Excel;
using ImportExportComponent.Configuration;

namespace ImportExportComponent.Services
{
    /// <summary>
    /// Detects Excel header rows using configurable algorithms
    /// </summary>
    public class ExcelHeaderDetector
    {
        private readonly ImportExportOptions _options;

        public ExcelHeaderDetector(ImportExportOptions options)
        {
            _options = options;
        }

        /// <summary>
        /// Detects the header row in an Excel worksheet
        /// </summary>
        public (List<string> Headers, int RowNumber) DetectHeader(IXLWorksheet worksheet, CancellationToken cancellationToken = default)
        {
            var rows = worksheet.RowsUsed().ToList();
            if (rows.Count == 0)
            {
                throw new InvalidOperationException("Excel faylı boşdur.");
            }

            var maxSearchDepth = Math.Min(_options.MaxHeaderRowSearchDepth, rows.Count);
            var sensitivity = Math.Clamp(_options.HeaderDetectionSensitivity, 0.0, 1.0);

            for (int i = 0; i < maxSearchDepth; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var row = rows[i];
                var cellValues = row.CellsUsed()
                    .Select(c => c.GetValue<string>()?.Trim())
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Select(c => c!)
                    .ToList();

                if (IsHeaderRow(cellValues, sensitivity))
                {
                    return (cellValues, row.RowNumber());
                }
            }

            throw new InvalidOperationException("Başlıq sətri tapılmadı. Zəhmət olmasa Excel faylınızı yoxlayın.");
        }

        private bool IsHeaderRow(List<string> cellValues, double sensitivity)
        {
            if (cellValues.Count < 2)
            {
                return false;
            }

            // Calculate the ratio of non-empty, reasonable-length cells
            var validCells = cellValues.Count(cell =>
                !string.IsNullOrWhiteSpace(cell) &&
                cell.Length < 100 &&
                !LooksLikeData(cell));

            var ratio = (double)validCells / cellValues.Count;

            // Header row should have high ratio of valid header-like cells
            return ratio >= sensitivity;
        }

        private static bool LooksLikeData(string value)
        {
            // Check if the value looks like data rather than a header
            // Headers are typically short, don't contain numbers in certain patterns, etc.
            
            // If it's a date-like string, it's probably data
            if (DateTime.TryParse(value, out _))
            {
                return true;
            }

            // If it's a number, it's probably data
            if (decimal.TryParse(value, out _))
            {
                return true;
            }

            // If it's very long, it's probably data
            if (value.Length > 50)
            {
                return true;
            }

            return false;
        }
    }
}


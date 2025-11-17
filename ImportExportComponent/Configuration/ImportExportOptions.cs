namespace ImportExportComponent.Configuration
{
    /// <summary>
    /// Import/Export component configuration options
    /// </summary>
    public class ImportExportOptions
    {
        public const string SectionName = "ImportExport";

        /// <summary>
        /// Export batch size for processing large datasets
        /// </summary>
        public int ExportBatchSize { get; set; } = 250_000;

        /// <summary>
        /// Import batch size for saving records to database
        /// </summary>
        public int ImportBatchSize { get; set; } = 1_000;

        /// <summary>
        /// Export folder name relative to wwwroot
        /// </summary>
        public string ExportFolderName { get; set; } = "exports";

        /// <summary>
        /// Base URL for download links
        /// </summary>
        public string? BaseUrl { get; set; }

        /// <summary>
        /// Rules file path (relative to AppContext.BaseDirectory or absolute)
        /// </summary>
        public string RulesFilePath { get; set; } = Path.Combine("Resources", "rules.json");

        /// <summary>
        /// Enable automatic rule generation from EF Core metadata
        /// </summary>
        public bool EnableAutoRuleGeneration { get; set; } = true;

        /// <summary>
        /// Maximum number of retry attempts for failed imports
        /// </summary>
        public int MaxRetryAttempts { get; set; } = 3;

        /// <summary>
        /// Delay between retry attempts in milliseconds
        /// </summary>
        public int RetryDelayMs { get; set; } = 1_000;

        /// <summary>
        /// Enable progress notifications via SignalR
        /// </summary>
        public bool EnableProgressNotifications { get; set; } = true;

        /// <summary>
        /// Progress notification interval (number of records processed before notification)
        /// </summary>
        public int ProgressNotificationInterval { get; set; } = 1_000;

        /// <summary>
        /// Maximum number of preview records to return
        /// </summary>
        public int MaxPreviewRecords { get; set; } = 100;

        /// <summary>
        /// Enable transaction management for imports
        /// </summary>
        public bool EnableTransactionManagement { get; set; } = true;

        /// <summary>
        /// Excel header detection sensitivity (0.0 - 1.0)
        /// Higher values require more cells to be non-empty for header detection
        /// </summary>
        public double HeaderDetectionSensitivity { get; set; } = 0.7;

        /// <summary>
        /// Maximum header row search depth
        /// </summary>
        public int MaxHeaderRowSearchDepth { get; set; } = 10;
    }
}



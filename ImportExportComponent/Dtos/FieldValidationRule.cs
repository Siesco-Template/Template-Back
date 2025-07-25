using ImportExportComponent.Enums;

namespace ImportExportComponent.Dtos
{
    public class FieldValidationRule
    {
        public string Field { get; set; }
        public bool IsRequired { get; set; } = false;
        public int? MaxLength { get; set; }
        public string? ForeignTable { get; set; }
        public string? ForeignColumn { get; set; }
        public ValidationDataType? DataType { get; set; }
    }
}
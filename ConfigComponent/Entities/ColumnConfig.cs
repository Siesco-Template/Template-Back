namespace ConfigComponent.Entities
{
    public class ColumnConfig
    {
        public bool Visible { get; set; }
        public bool Freeze { get; set; }
        public bool Sortable { get; set; } // gerekli mi?

        public TextStyle Style { get; set; }
        public ColumnSize Size { get; set; }
        public SummaryRowConfig SummaryRow { get; set; }
    }

    public class ColumnSize
    {
        public int? Width { get; set; } // px
        public int? MinWidth { get; set; }
        public int? MaxWidth { get; set; }
    }   

    public class TextStyle
    {
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public string Color { get; set; } // HEX kod (#ffffff)
        public int? FontSize { get; set; }
        public string Alignment { get; set; } // "left", "center", "right"   // metnin ve basligin hizasi eyni olacaq?
    }

    public class SummaryRowConfig
    {
        public string Mode { get; set; } // "hidden", "top", "bottom"
    }
}
namespace ConfigComponent.Entities
{
    public class HeaderConfig
    {
        public bool Pinned { get; set; }

        public CellStyle Cell { get; set; }
        public TextStyle Text { get; set; }
        public BorderStyle Border { get; set; }
    }

    public class CellStyle
    {
        public int Padding { get; set; } // px (Xana məsafəsi)
        public string BackgroundColor { get; set; } // #hex formatda
    }
     
    public class BorderStyle
    {
        public int Thickness { get; set; } // px qalınlıq
        public string Style { get; set; } // "solid", "dotted"
        public string Color { get; set; } // HEX kod (#e0e0e0)
    }
}
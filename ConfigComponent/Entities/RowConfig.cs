namespace ConfigComponent.Entities
{
    public class RowConfig
    {
        public string StripeStyle { get; set; } // "striped" | "plain"
        public CellStyle Cell { get; set; } // Xana arxa fon və spacing
        public BorderStyle Border { get; set; } // border parametrləri
        public TextStyle Text { get; set; } // Mətn vizualı
    }
}
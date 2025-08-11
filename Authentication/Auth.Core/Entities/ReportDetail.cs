using Auth.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFMISMain.Core.Entities
{
    public class ReportDetail : BaseEntity
    {
        public Guid DetailId { get; set; }
        public Detail Detail { get; set; }


        public Guid ReportId { get; set; }
        public Report Report { get; set; }


        //Smeta məbləği
        public decimal EstimateAmount { get; set; }

        //Maaliyələşmə məbləği
        public decimal FinancingAmount { get; set; }

        //Kassa xərc
        public decimal CheckoutAmount { get; set; }

        //Faktiki xərc
        public decimal ActualAmount { get; set; }
    }
}

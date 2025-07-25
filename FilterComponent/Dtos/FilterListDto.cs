using FilterComponent.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterComponent.Dtos
{
    public class FilterListDto
    {
        public string Id { get; set; }
        public string FilterTitle { get; set; }
        public bool IsDefault { get; set; }
        public List<FilterKeyValue> FilterValues { get; set; }
    }
}

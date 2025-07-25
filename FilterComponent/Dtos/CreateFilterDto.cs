using FilterComponent.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace FilterComponent.Dtos
{
    public class CreateFilterDto
    {
        public string TableId { get; set; }
        public string FilterTitle { get; set; }
        public List<FilterKeyValue> FilterValues { get; set; }

        //public Dictionary<string, byte> FilterValues { get; set; }
        //public List<FilterValueDto> FilterValues { get; set; }
    }
}

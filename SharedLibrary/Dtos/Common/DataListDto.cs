using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Dtos.Common
{
    public class DataListDto<T>
    {
        public List<T> Datas { get; set; }
        public int TotalCount { get; set; }
    }
}

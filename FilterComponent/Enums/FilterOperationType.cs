using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterComponent.Enums
{
    public enum FilterOperationType : byte
    {
        /// <summary>
        /// Beraberdir
        /// </summary>
        Equal = 1,
        /// <summary>
        /// Beraber deyil
        /// </summary>
        NotEqual = 2,
        /// <summary>
        /// Oxsar olanlar
        /// </summary>
        Like = 3,
        /// <summary>
        /// Oxsar olmayanlar
        /// </summary>
        NotLike = 4,
        /// <summary>
        /// Boyukdur
        /// </summary>
        GreaterThan = 5,
        /// <summary>
        /// Kicikdir
        /// </summary>
        LessThan = 6,
        /// <summary>
        /// Boyuk beraberdir
        /// </summary>
        GreaterThanOrEqual = 7,
        /// <summary>
        /// Kicik beraberdir
        /// </summary>
        LessThanOrEqual = 8,
        /// <summary>
        /// Daxildir
        /// </summary>
        In = 9,
        /// <summary>
        /// Daxil deyil
        /// </summary>
        NotIn = 10,
        /// <summary>
        /// Eded ve yaxud tarix araligi
        /// </summary>
        RangeNumberOrDate = 11,
        //RangeDate = 12
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FilterComponent.Utilities
{
    public static class Property
    {
        /// <summary>
        /// Bu metod üzərində filter edəcəyimiz entitynin varlığını və eyni zamanda entitynin belə bir propertysinin olub olmadiğini yoxlayir
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">Filter edilmək istənilən table</param>
        /// <param name="entityProperty">Filter edilmək istənilən table-nin sütunu</param>
        /// <returns></returns>
        private static bool CheckProperty<T>(this T entity , string entityProperty) where T : class
        {
            if (entity == null) return false;
            if (typeof(T).GetProperty(entityProperty) == null) return false;

            return true;
        }
    }
}

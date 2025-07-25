using System.Linq.Expressions;
using System.Reflection;

namespace QueryGenerator.Extensions
{
    public static class IgnoreCaseHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// ustunde ishlecek obyekt
        /// <param name="parameter"></param>
        /// bize lazim olam prop adlarını ozunde saxlayan string
        /// <param name="path"></param>
        /// axirinci propun tipi
        /// <param name="resultType"></param>
        /// <returns></returns>
        public static Expression? BuildSafePropertyAccessExpression(Expression parameter, string path, out Type resultType)
        {
            Expression? current = parameter;
            resultType = parameter.Type;

            foreach (var part in path.Split('.'))
            {
                //boyuk-kicik herf ferq etmeden, yalniz public proplari ve instance proplari tapir
                var prop = current.Type.GetProperty(part,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (prop == null)
                {
                    return null;
                }

                current = Expression.Property(current, prop);
                resultType = prop.PropertyType;
            }

            return current;
        }
    }
}
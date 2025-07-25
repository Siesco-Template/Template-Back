using System.Linq.Expressions;

namespace QueryGenerator.Parser
{
    internal static class Constants
    {
        public static bool IsNull(Expression exp)
        {
            return exp is ConstantExpression { Value: null };
        }
    }
}
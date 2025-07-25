using AnyOfTypes;
using System.Linq.Expressions;

namespace QueryGenerator.Parser
{
    internal interface IKeywordsHelper
    {
        bool IsItOrRootOrParent(AnyOf<string, Expression, Type> value);

        bool TryGetValue(string text, out AnyOf<string, Expression, Type> value);
    }
}
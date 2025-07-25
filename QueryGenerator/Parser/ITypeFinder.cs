using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace QueryGenerator.Parser;

internal interface ITypeFinder
{
    bool TryFindTypeByName(string name, ParameterExpression?[]? expressions, bool forceUseCustomTypeProvider, [NotNullWhen(true)] out Type? type);
}
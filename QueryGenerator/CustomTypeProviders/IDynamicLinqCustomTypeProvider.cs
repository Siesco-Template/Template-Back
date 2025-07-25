using System.Reflection;

namespace QueryGenerator.CustomTypeProviders
{
    public interface IDynamicLinqCustomTypeProvider
    {
        //
        // Summary:
        //     Returns a list of custom types that System.Linq.Dynamic.Core will understand.
        //
        //
        // Returns:
        //     A System.Collections.Generic.HashSet`1 list of custom types.
        HashSet<Type> GetCustomTypes();

        //
        // Summary:
        //     Returns a list of custom extension methods that System.Linq.Dynamic.Core will
        //     understand.
        //
        // Returns:
        //     A list of custom extension methods that System.Linq.Dynamic.Core will understand.
        Dictionary<Type, List<MethodInfo>> GetExtensionMethods();

        //
        // Summary:
        //     Resolve any type by fullname which is registered in the current application domain.
        //
        //
        // Parameters:
        //   typeName:
        //     The typename to resolve.
        //
        // Returns:
        //     A resolved System.Type or null when not found.
        Type? ResolveType(string typeName);

        //
        // Summary:
        //     Resolve any type by the simple name which is registered in the current application
        //     domain.
        //
        // Parameters:
        //   simpleTypeName:
        //     The typename to resolve.
        //
        // Returns:
        //     A resolved System.Type or null when not found.
        Type? ResolveTypeBySimpleName(string simpleTypeName);
    }
}
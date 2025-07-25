using QueryGenerator.Validation;
using System.Reflection;

namespace QueryGenerator.CustomTypeProviders
{
    public abstract class AbstractDynamicLinqCustomTypeProvider
    {
        //
        // Summary:
        //     Additional types which should also be resolved.
        protected readonly IList<Type> AdditionalTypes;

        //
        // Summary:
        //     Initializes a new instance of the System.Linq.Dynamic.Core.CustomTypeProviders.AbstractDynamicLinqCustomTypeProvider
        //     class.
        //
        // Parameters:
        //   additionalTypes:
        //     A list of additional types (without the DynamicLinqTypeAttribute annotation)
        //     which should also be resolved.
        protected AbstractDynamicLinqCustomTypeProvider(IList<Type> additionalTypes)
        {
            AdditionalTypes = Check.NotNull(additionalTypes, "additionalTypes");
        }

        //
        // Summary:
        //     Finds the unique types annotated with DynamicLinqTypeAttribute.
        //
        // Parameters:
        //   assemblies:
        //     The assemblies to process.
        //
        // Returns:
        //     System.Collections.Generic.IEnumerable`1
        protected Type[] FindTypesMarkedWithDynamicLinqTypeAttribute(IEnumerable<Assembly> assemblies)
        {
            Check.NotNull(assemblies, "assemblies");
            assemblies = assemblies.Where((Assembly a) => !a.IsDynamic);
            return GetAssemblyTypesWithDynamicLinqTypeAttribute(assemblies).Distinct().ToArray();
        }

        //
        // Summary:
        //     Resolve a type which is annotated with DynamicLinqTypeAttribute or when the type
        //     is listed in AdditionalTypes.
        //
        // Parameters:
        //   assemblies:
        //     The assemblies to inspect.
        //
        //   typeName:
        //     The typename to resolve.
        //
        // Returns:
        //     A resolved System.Type or null when not found.
        protected Type? ResolveType(IEnumerable<Assembly> assemblies, string typeName)
        {
            string typeName2 = typeName;
            Check.NotNull(assemblies, "assemblies");
            Check.NotEmpty(typeName2, "typeName");
            return FindTypesMarkedWithDynamicLinqTypeAttribute(assemblies).Union(AdditionalTypes).FirstOrDefault((Type t) => t.FullName == typeName2);
        }

        //
        // Summary:
        //     Resolve a type which is annotated with DynamicLinqTypeAttribute by the simple
        //     (short) name. Also when the type is listed in AdditionalTypes.
        //
        // Parameters:
        //   assemblies:
        //     The assemblies to inspect.
        //
        //   simpleTypeName:
        //     The simple typename to resolve.
        //
        // Returns:
        //     A resolved System.Type or null when not found.
        protected Type? ResolveTypeBySimpleName(IEnumerable<Assembly> assemblies, string simpleTypeName)
        {
            string simpleTypeName2 = simpleTypeName;
            Check.NotNull(assemblies, "assemblies");
            Check.NotEmpty(simpleTypeName2, "simpleTypeName");
            IEnumerable<Type> source = FindTypesMarkedWithDynamicLinqTypeAttribute(assemblies).Union(AdditionalTypes);
            string[] source2 = source.Select((Type t) => t.FullName).Distinct().ToArray();
            string firstMatchingFullname = source2.FirstOrDefault((string fn) => fn.EndsWith("." + simpleTypeName2));
            if (firstMatchingFullname != null)
            {
                return source.FirstOrDefault((Type t) => t.FullName == firstMatchingFullname);
            }

            return null;
        }

        //
        // Summary:
        //     Gets the assembly types annotated with System.Linq.Dynamic.Core.CustomTypeProviders.DynamicLinqTypeAttribute
        //     in an Exception friendly way.
        //
        // Parameters:
        //   assemblies:
        //     The assemblies to process.
        //
        // Returns:
        //     Array of System.Type
        protected Type[] GetAssemblyTypesWithDynamicLinqTypeAttribute(IEnumerable<Assembly> assemblies)
        {
            Check.NotNull(assemblies, "assemblies");
            List<Type> list = new List<Type>();
            foreach (Assembly assembly in assemblies)
            {
                Type[] array;
                try
                {
                    array = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    array = ex.Types.OfType<Type>().ToArray();
                }
                catch
                {
                    array = Type.EmptyTypes;
                }

                Type[] array2 = array;
                foreach (Type type in array2)
                {
                    try
                    {
                        if (type.IsDefined(typeof(DynamicLinqTypeAttribute), inherit: false))
                        {
                            list.Add(type);
                        }
                    }
                    catch
                    {
                    }
                }
            }

            return list.Distinct().ToArray();
        }
    }
}
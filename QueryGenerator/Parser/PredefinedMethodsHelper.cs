using QueryGenerator.Core;
using QueryGenerator.Validation;
using System.Reflection;

namespace QueryGenerator.Parser;

internal static class PredefinedMethodsHelper
{
    internal static readonly MethodInfo ObjectToString = typeof(object).GetMethod(nameof(ToString), BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null)!;
    internal static readonly MethodInfo ObjectInstanceEquals = typeof(object).GetMethod(nameof(Equals), BindingFlags.Instance | BindingFlags.Public, null, [typeof(object)], null)!;
    internal static readonly MethodInfo ObjectStaticEquals = typeof(object).GetMethod(nameof(Equals), BindingFlags.Static | BindingFlags.Public, null, [typeof(object), typeof(object)], null)!;
    internal static readonly MethodInfo ObjectStaticReferenceEquals = typeof(object).GetMethod(nameof(ReferenceEquals), BindingFlags.Static | BindingFlags.Public, null, [typeof(object), typeof(object)], null)!;


    private static readonly HashSet<MemberInfo> ObjectToStringAndObjectEquals =
    [
        ObjectToString,
        ObjectInstanceEquals,
        ObjectStaticEquals,
        ObjectStaticReferenceEquals
    ];

    public static bool IsPredefinedMethod(ParsingConfig config, MemberInfo member)
    {
        Check.NotNull(config);
        Check.NotNull(member);

        return config.AllowEqualsAndToStringMethodsOnObject && ObjectToStringAndObjectEquals.Contains(member);
    }
}
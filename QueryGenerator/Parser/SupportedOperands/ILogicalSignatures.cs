using System.Diagnostics.CodeAnalysis;

namespace QueryGenerator.Parser.SupportedOperands;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
internal interface ILogicalSignatures
{
    void F(bool x, bool y);
    void F(bool? x, bool? y);
}
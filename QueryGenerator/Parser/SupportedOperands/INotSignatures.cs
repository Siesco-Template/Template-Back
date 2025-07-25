using System.Diagnostics.CodeAnalysis;

namespace QueryGenerator.Parser.SupportedOperands;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
internal interface INotSignatures
{
    void F(bool x);
    void F(bool? x);
}
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DomainDrivenDesign.DomainObjects.Persistence.Generator
{
    public interface IMemberFactory
    {
        MemberDeclarationSyntax Create(Definition definition);
    }
}
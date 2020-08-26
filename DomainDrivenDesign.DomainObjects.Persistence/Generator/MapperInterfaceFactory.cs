using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DomainDrivenDesign.DomainObjects.Persistence.Generator
{
    public class MapperInterfaceFactory : IMemberFactory
    {
        public MemberDeclarationSyntax Create(Definition definition)
        {
                var mapDomainObjectToEntity = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(definition.TargetType), "Map")
                    .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("domainObject")).WithType(SyntaxFactory.ParseTypeName(definition.SourceType)))
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
            
                var mapEntityOnToDomainObject = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(definition.SourceType), "Map") 
                    .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("entity")).WithType(SyntaxFactory.ParseTypeName(definition.TargetType)))
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
            
                var apply = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "Apply")
                    .AddParameterListParameters(
                        SyntaxFactory.Parameter(SyntaxFactory.Identifier("domainObject")).WithType(SyntaxFactory.ParseTypeName(definition.SourceType)),
                        SyntaxFactory.Parameter(SyntaxFactory.Identifier("entity")).WithType(SyntaxFactory.ParseTypeName(definition.TargetType))
                    )
                    .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
            
                return SyntaxFactory.InterfaceDeclaration($"I{definition.TargetType}Mapper")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.InternalKeyword))
                    .WithMembers(
                        new SyntaxList<MemberDeclarationSyntax>(new []
                        {
                            mapDomainObjectToEntity,
                            mapEntityOnToDomainObject,
                            apply
                        })
                    );
            
        }
    }
}
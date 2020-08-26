using System;
using System.Collections.Generic;
using System.Linq;
using DomainDrivenDesign.DomainObjects.Persistence.ExtensionMethods;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DomainDrivenDesign.DomainObjects.Persistence.Generator
{
    public class MapperFactory : IMemberFactory
    {
        public MemberDeclarationSyntax Create(Definition definition)
        {
            return SyntaxFactory.ClassDeclaration($"{definition.TargetType}DataModelMapper")
                .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"I{definition.TargetType}Mapper")))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.InternalKeyword), SyntaxFactory.Token(SyntaxKind.AbstractKeyword))
                .AddMembers(
                    CreateMapEntityToDomainObjectMethod(definition), 
                    CreateApplyMethod(definition),
                    CreateMapDomainObjectMethod(definition)
                );
        }
        
        private static MethodDeclarationSyntax CreateMapDomainObjectMethod(Definition definition)
        {           
            return SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(definition.SourceType), "Map")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AbstractKeyword))
                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("domainObject")).WithType(SyntaxFactory.ParseTypeName(definition.TargetType)))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        }
        
        private static MethodDeclarationSyntax CreateMapEntityToDomainObjectMethod(Definition definition)
        {
            var statements = new List<StatementSyntax>
            {
                SyntaxFactory.ParseStatement($"var x = new {definition.TargetType}();"),
                SyntaxFactory.ParseStatement($"Apply(domainObject, x);"),
                SyntaxFactory.ParseStatement($"return x;")
            };

            // Wrap in method declaration type
            return SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(definition.TargetType), "Map")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.VirtualKeyword))
                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("domainObject")).WithType(SyntaxFactory.ParseTypeName(definition.SourceType)))
                .WithBody(SyntaxFactory.Block(statements));
        }

        private static MethodDeclarationSyntax CreateApplyMethod(Definition definition)
        {
            // The mapping statements
            var conversionMethods = new Dictionary<Type, string>
            {
                {typeof(string), "ToString()"},
                {typeof(DateTime), "ToDateTime()"},
                {typeof(DateTime?), "ToDateTime()"},
                {typeof(Guid), "ToGuid()"},
                {typeof(Guid?), "ToGuid()"},
                {typeof(bool), "ToBool()"},
                {typeof(int), "ToInt()"},
                {typeof(int?), "ToInt()"},
                {typeof(double), "ToDouble()"},
                {typeof(double?), "ToDouble()"},
                {typeof(decimal), "ToDecimal()"},
                {typeof(decimal?), "ToDecimal()"}
            };

            var statements = from property in definition.Columns
                let conversionMethod = conversionMethods[property.Value]
                let nullProtection = property.Value.IsNullable() ? "?" : string.Empty
                select SyntaxFactory.ParseStatement($"entity.{property.Key} = domainObject.{property.Key}{nullProtection}.{conversionMethod};");

            // Apply method
            return SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "Apply")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.VirtualKeyword))
                .AddParameterListParameters(
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("domainObject")).WithType(SyntaxFactory.ParseTypeName(definition.SourceType)),
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("entity")).WithType(SyntaxFactory.ParseTypeName(definition.TargetType))
                )
                .WithBody(SyntaxFactory.Block(statements));
        }
    }
}
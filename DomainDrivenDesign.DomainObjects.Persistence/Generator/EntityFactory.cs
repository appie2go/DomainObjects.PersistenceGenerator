using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DomainDrivenDesign.DomainObjects.Persistence.ExtensionMethods;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DomainDrivenDesign.DomainObjects.Persistence.Generator
{
    public class EntityFactory : IMemberFactory
    {
        public MemberDeclarationSyntax Create(Definition definition)
        {
            var properties = new List<PropertyDeclarationSyntax>();
            foreach (var (propertyName, targetDataType) in definition.Columns)
            {
                var publicProperty = CreateProperty(targetDataType, propertyName);
                if (definition.Keys.ContainsKey(propertyName))
                {
                    publicProperty = DecorateWithKeyAttribute(publicProperty);
                }

                properties.Add(publicProperty);
            }

            // Generate the class
            return SyntaxFactory.ClassDeclaration(definition.TargetType)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.InternalKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                .AddMembers(properties.ToArray());
        }

        private static PropertyDeclarationSyntax DecorateWithKeyAttribute(PropertyDeclarationSyntax publicProperty)
        {
            return publicProperty.AddAttributeLists(SyntaxFactory.AttributeList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Key"))
                )
            ));
        }

        private static PropertyDeclarationSyntax CreateProperty(Type targetDataType, string propertyName)
        {
            var semicolon = SyntaxFactory.Token(SyntaxKind.SemicolonToken);
            
            return SyntaxFactory
                .PropertyDeclaration(SyntaxFactory.ParseTypeName(GetTypeName(targetDataType)), propertyName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.VirtualKeyword))
                .AddAccessorListAccessors(
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(semicolon),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(semicolon)
                );
        }

        private static string GetTypeName(Type type)
        {
            if (!type.IsNullable())
            {
                return type.ToString();
            }

            const string pattern = "\\[(.*?)\\]";
            var matches = Regex.Matches(type.ToString(), pattern);
            if (!matches.Any())
            {
                return type.ToString();
            }
            
            return $"System.Nullable<{matches[0].Groups[1].Value}>";

        }
    }
}
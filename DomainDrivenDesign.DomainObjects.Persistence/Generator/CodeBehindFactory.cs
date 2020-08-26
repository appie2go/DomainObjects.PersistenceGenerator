using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DomainDrivenDesign.DomainObjects.Persistence.Generator
{
    public class CodeBehindFactory : IMemberFactory
    {
        private readonly string _targetNamespace;
        private readonly IEnumerable<IMemberFactory> _factories;

        public CodeBehindFactory(string targetNamespace, params IMemberFactory[] factories) 
            : this(targetNamespace, factories.ToList())
        {
            // i.l.e.
        }
        
        public CodeBehindFactory(string targetNamespace, IEnumerable<IMemberFactory> factories)
        {
            _targetNamespace = targetNamespace;
            _factories = factories ?? throw new ArgumentNullException(nameof(factories));
        }

        public MemberDeclarationSyntax Create(Definition definition)
        {
            var elements = _factories.Select(x => x.Create(definition)).ToArray();
            return WrapInNamespace(elements);
        }
        
        private NamespaceDeclarationSyntax WrapInNamespace(IEnumerable<MemberDeclarationSyntax> classes)
        {
            return SyntaxFactory
                .NamespaceDeclaration(SyntaxFactory.ParseName(_targetNamespace))
                .AddUsings(
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")), 
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.ComponentModel.DataAnnotations")),
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("DomainDrivenDesign.DomainObjects.Persistence.ExtensionMethods"))
                )
                .AddMembers(classes.ToArray())
                .NormalizeWhitespace();
        }
    }
}
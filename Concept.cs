using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FunAndGames
{

    public class CodeBuilder
    {
        private readonly Type _source;
        private readonly string _targetTableName;
        private readonly IDictionary<PropertyInfo, Type> _properties = new Dictionary<PropertyInfo, Type>();
        
        private string _key;

        public CodeBuilder(Type source, string targetTableName)
        {
            _source = source;
            _targetTableName = targetTableName;
        }
        
        public CodeBuilder WithId(string propertyNameOnSource, Type columnTypeInDatabase)
        {
            WithProperty(propertyNameOnSource, columnTypeInDatabase);
            _key = propertyNameOnSource;
            return this;
        }
        
        public CodeBuilder WithProperty(string propertyNameOnSource, Type columnTypeInDatabase)
        {
            var prop = _source.GetProperty(propertyNameOnSource);
            if (prop == null)
            {
                throw new Exception("Property does not exist");
            }

            _properties.Add(prop, columnTypeInDatabase);
            return this;
        }

        public string Build() => WrapInNamespace(CreateEntity(), CreateMapperInterface(), CreateMapper()).ToString();

// <Entity>
        private ClassDeclarationSyntax CreateEntity()
        {
            var properties = new List<PropertyDeclarationSyntax>();
            foreach (var (propertyInfo, targetDataType) in _properties)
            {
                // Create the property
                var publicProperty = SyntaxFactory
                    .PropertyDeclaration(SyntaxFactory.ParseTypeName(targetDataType.ToString()), propertyInfo.Name)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                        SyntaxFactory.Token(SyntaxKind.VirtualKeyword))
                    .AddAccessorListAccessors(
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));

                // Add the primary key attribute
                if (propertyInfo.Name == _key)
                {
                    publicProperty = publicProperty.AddAttributeLists(SyntaxFactory.AttributeList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Key"))
                        )
                    ));
                }

                // Append
                properties.Add(publicProperty);
            }

            // Generate the class
            return SyntaxFactory.ClassDeclaration(_targetTableName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.InternalKeyword))
                .AddMembers(properties.ToArray());
        }
// </Entity>

// <Mapper>
        private InterfaceDeclarationSyntax CreateMapperInterface()
        {
            var mapDomainObjectToEntity = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(_targetTableName), "Map")
                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("domainObject")).WithType(SyntaxFactory.ParseTypeName(_source.ToString())))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
            
            var mapEntityOnToDomainObject = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(_source.ToString()), "Map") 
                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("entity")).WithType(SyntaxFactory.ParseTypeName(_targetTableName)))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
            
            var apply = SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "Apply")
                .AddParameterListParameters(
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("domainObject")).WithType(SyntaxFactory.ParseTypeName(_source.ToString())),
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("entity")).WithType(SyntaxFactory.ParseTypeName(_targetTableName))
                )
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
            
            return SyntaxFactory.InterfaceDeclaration($"I{_targetTableName}Mapper")
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
        
        private ClassDeclarationSyntax CreateMapper()
        {
            // Create an abstract mapper class, the method to map the entity to the domain object is to be implemented
            return SyntaxFactory.ClassDeclaration($"{_targetTableName}Mapper")
                .AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName($"I{_targetTableName}Mapper")))
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.InternalKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                .AddMembers(
                    CreateMapMethod(), 
                    CreateApplyMethod()
                    //, CreateMapDomainObjectMethod()
                );
        }

        // private MethodDeclarationSyntax CreateMapDomainObjectMethod()
        // {           
        //     // Create an abstract method to map the entity class to a domain object
        //     return SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(_source.ToString()), "Map")
        //         .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AbstractKeyword))
        //         .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("domainObject")).WithType(SyntaxFactory.ParseTypeName(_targetTableName)))
        //         .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
        // }

        private MethodDeclarationSyntax CreateMapMethod()
        {
            // Create new instance of entity, map the values and return it
            var statements = new List<StatementSyntax>
            {
                SyntaxFactory.ParseStatement($"var x = new {_targetTableName}();"),
                SyntaxFactory.ParseStatement($"Apply(domainObject, x);"),
                SyntaxFactory.ParseStatement($"return x;")
            };

            // Wrap in method declaration type
            return SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName(_targetTableName), "Map")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.VirtualKeyword))
                .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier("domainObject")).WithType(SyntaxFactory.ParseTypeName(_source.ToString())))
                .WithBody(SyntaxFactory.Block(statements));
        }

        private MethodDeclarationSyntax CreateApplyMethod()
        {
            // The mapping statements
            var conversionMethods = new Dictionary<Type, string>
            {
                {typeof(string), "ToString()"},
                {typeof(DateTime), "ToDateTime()"},
                {typeof(Guid), "ToGuid()"},
                {typeof(bool), "ToBool()"},
                {typeof(int), "ToInt()"},
                {typeof(decimal), "ToDecimal()"}
            };

            var statements = from property in _properties
                let conversionMethod = conversionMethods[property.Value]
                select SyntaxFactory.ParseStatement($"entity.{property.Key.Name} = domainObject.{property.Key.Name}.{conversionMethod};");

            // Apply method
            return SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "Apply")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.VirtualKeyword))
                .AddParameterListParameters(
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("domainObject")).WithType(SyntaxFactory.ParseTypeName(_source.ToString())),
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("entity")).WithType(SyntaxFactory.ParseTypeName(_targetTableName))
                )
                .WithBody(SyntaxFactory.Block(statements));
        }

        private NamespaceDeclarationSyntax WrapInNamespace(params MemberDeclarationSyntax[] classes)
        {
            return SyntaxFactory
                .NamespaceDeclaration(SyntaxFactory.ParseName(GetType().Namespace))
                .AddUsings(
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")), 
                    SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.ComponentModel.DataAnnotations"))
                )
                .AddMembers(classes)
                .NormalizeWhitespace();
        }
// </mapper>

// <Repository>

// </Repository>

    }
}

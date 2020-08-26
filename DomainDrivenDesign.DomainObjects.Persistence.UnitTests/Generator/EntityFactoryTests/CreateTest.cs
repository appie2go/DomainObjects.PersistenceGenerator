using System;
using AutoFixture;
using DomainDrivenDesign.DomainObjects.Persistence.Generator;
using DomainDrivenDesign.DomainObjects.Persistence.UnitTests.SpecimenBuilders;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DomainDrivenDesign.DomainObjects.Persistence.UnitTests.Generator.EntityFactoryTests
{
    [TestClass]
    public class CreateTest
    {
        private readonly Fixture _fixture = new Fixture();

        [TestInitialize]
        public void Initialize()
        {
            _fixture.Customizations.Add(new DefinitionBuilder());
        }
        
        [TestMethod]
        public void ShouldGenerateInternalPartialClass()
        {
            // Arrange
            var definition = _fixture.Create<Definition>();
            var sut = new EntityFactory();
            
            // Act
            var actual = sut.Create(definition);
            
            // Assert
            var code = actual.NormalizeWhitespace().ToString();
            code.Should().Contain($"internal partial class {definition.TargetType}");
        }

        [TestMethod]
        public void WhenColumns_ShouldGenerateProperties()
        {
            // Arrange
            var definition = _fixture.Create<Definition>();
            definition.Columns.Add("Test", typeof(Guid));
            var sut = new EntityFactory();
            
            // Act
            var actual = sut.Create(definition);
            
            // Assert
            var code = actual.NormalizeWhitespace().ToString();
            code.Should().Contain($"public virtual System.Guid Test");
        }
        
        [TestMethod]
        public void WhenKeys_ShouldGeneratePublicVirtualPropertyWithKeyAttribute()
        {
            // Arrange
            var definition = _fixture.Create<Definition>();
            definition.Keys.Clear();
            definition.Keys.Add("Id", typeof(Guid));
            definition.Columns.Add("Id", typeof(Guid));
            var sut = new EntityFactory();
            
            // Act
            var actual = sut.Create(definition);
            
            // Assert
            var code = actual.NormalizeWhitespace().ToString();
            code.Should().Contain($"[Key]\r\n    public virtual System.Guid Id");
        }
        
        [TestMethod]
        public void WhenNullableType_ShouldCreateNullableProperty()
        {
            // Arrange
            var definition = _fixture.Create<Definition>();
            definition.Columns.Add("test", typeof(Guid?));
            var sut = new EntityFactory();
            
            // Act
            var actual = sut.Create(definition);
            
            // Assert
            var code = actual.NormalizeWhitespace().ToString();
            code.Should().Contain($"virtual System.Nullable<System.Guid> test");
        }
        
        [TestMethod]
        public void WhenString_ShouldCreateNotNullableProperty()
        {
            // Arrange
            var definition = _fixture.Create<Definition>();
            definition.Columns.Add("test", typeof(string));
            var sut = new EntityFactory();
            
            // Act
            var actual = sut.Create(definition);
            
            // Assert
            var code = actual.NormalizeWhitespace().ToString();
            code.Should().Contain($"virtual System.String test");
        }
    }
}
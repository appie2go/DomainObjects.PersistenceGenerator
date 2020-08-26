using AutoFixture;
using DomainDrivenDesign.DomainObjects.Persistence.Generator;
using DomainDrivenDesign.DomainObjects.Persistence.UnitTests.SpecimenBuilders;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DomainDrivenDesign.DomainObjects.Persistence.UnitTests.Generator.MapperFatoryTests
{
    [TestClass]
    public class CreateTests
    {
        private readonly Fixture _fixture = new Fixture();

        [TestInitialize]
        public void Initialize()
        {
            _fixture.Customizations.Add(new DefinitionBuilder());
        }

        [TestMethod]
        public void WhenNullableType_ShouldProtectAgainstNulls()
        {
            // Arrange
            var definition = _fixture.Create<Definition>();
            definition.Columns.Add("test", typeof(string));
            var sut = new MapperFactory();
            
            // Act
            var actual = sut.Create(definition);

            // Assert
            var code = actual.NormalizeWhitespace().ToString();
            code.Should().Contain("test?.ToString();");
        }

        [TestMethod]
        public void WhenNotNullableType_ShouldNotProtectAgainstNulls()
        {
            // Arrange
            var definition = _fixture.Create<Definition>();
            definition.Columns.Add("test", typeof(int));
            var sut = new MapperFactory();
            
            // Act
            var actual = sut.Create(definition);

            // Assert
            var code = actual.NormalizeWhitespace().ToString();
            code.Should().Contain("test.ToInt();");
        }
    }
}
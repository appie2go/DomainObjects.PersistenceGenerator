using AutoFixture;
using DomainDrivenDesign.DomainObjects.Persistence.UnitTests.SpecimenBuilders;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DomainDrivenDesign.DomainObjects.Persistence.UnitTests.DefinitionTests
{
    [TestClass]
    public class ValidateTest
    {        
        private readonly Fixture _fixture = new Fixture();

        [TestInitialize]
        public void Initialize()
        {
            _fixture.Customizations.Add(new DefinitionBuilder());
        }
        
        [TestMethod]
        public void WhenValidDefinition_ShouldReturnTrue()
        {
            // Arrange
            var sut = _fixture.Create<Definition>();
            
            // Act
            var actual = sut.Validate(out var errorMessage);
            
            // Asset
            actual.Should().BeTrue();
            errorMessage.Should().BeNull();
        }
        
        [TestMethod]
        public void WhenNoSourceType_ShouldReturnFalse()
        {
            // Arrange
            var sut = _fixture.Create<Definition>();
            sut.SourceType = null;
            
            // Act
            var actual = sut.Validate(out var errorMessage);
            
            // Asset
            actual.Should().BeFalse();
            errorMessage.Should().NotBeNull();
        }
        
        [TestMethod]
        public void WhenNoTargetType_ShouldReturnFalse()
        {
            // Arrange
            var sut = _fixture.Create<Definition>();
            sut.TargetType = null;
            
            // Act
            var actual = sut.Validate(out var errorMessage);
            
            // Asset
            actual.Should().BeFalse();
            errorMessage.Should().NotBeNull();
        }
        
        [TestMethod]
        public void WhenNoTargetNameSpace_ShouldReturnFalse()
        {
            // Arrange
            var sut = _fixture.Create<Definition>();
            sut.TargetNamespace = null;
            
            // Act
            var actual = sut.Validate(out var errorMessage);
            
            // Asset
            actual.Should().BeFalse();
            errorMessage.Should().NotBeNull();
        }
        
        [TestMethod]
        public void WhenNoColumns_ShouldReturnFalse()
        {
            // Arrange
            var sut = _fixture.Create<Definition>();
            sut.Columns.Clear();
            
            // Act
            var actual = sut.Validate(out var errorMessage);
            
            // Asset
            actual.Should().BeFalse();
            errorMessage.Should().NotBeNull();
        }
    }
}
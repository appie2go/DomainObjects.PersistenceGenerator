using System;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DomainDrivenDesign.DomainObjects.Persistence.UnitTests.DefinitionTests
{
    [TestClass]
    public class ParseTest
    {
        private const string Testcase = @"- sourceType: FunAndGames.TestEntity
- targetType:
  - entityName: Banaan
  - targetNamespace: FunAndGames.Test.Repositories.Bananas
  - key:
    Id, System.Guid
    CombinedId, System.Int32
  - properties:
    Price, System.Decimal
    Discount, Nullable<System.Decimal>
    Address, System.String";

        private Definition _actual;

        [TestInitialize]
        public void Act()
        {
            _actual = Definition.Parse(Testcase);
        }

        [TestMethod]
        public void WhenPropertiesAndKeys_ColumnsShouldContainPropertiesAndKeys()
        {
            _actual.Columns.Count.Should().Be(5);
        }
        
        [TestMethod]
        public void WhenKeys_ShouldContainKeys()
        {
            _actual.Keys.Count.Should().Be(2);
        }
        
        [TestMethod]
        public void WhenPropertiesWithValidTypes_ColumnsShouldContainTypes()
        {
            _actual.Columns.All(x => x.Value != null).Should().BeTrue();
            _actual.Columns.ContainsValue(typeof(decimal)).Should().BeTrue();
            _actual.Columns.ContainsValue(typeof(string)).Should().BeTrue();
        }
        
        [TestMethod]
        public void WhenKeysWithValidTypes_ColumnsShouldContainTypes()
        {
            _actual.Columns.All(x => x.Value != null).Should().BeTrue();
            _actual.Columns.ContainsValue(typeof(int)).Should().BeTrue();
            _actual.Columns.ContainsValue(typeof(Guid)).Should().BeTrue();
        }
        
        [TestMethod]
        public void WhenKeysWithValidTypes_KeysShouldContainTypes()
        {
            _actual.Keys.All(x => x.Value != null).Should().BeTrue();
            _actual.Keys.ContainsValue(typeof(int)).Should().BeTrue();
            _actual.Keys.ContainsValue(typeof(Guid)).Should().BeTrue();
        }
    }
}
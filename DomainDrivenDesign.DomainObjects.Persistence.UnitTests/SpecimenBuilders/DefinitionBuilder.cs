using System;
using System.Text.RegularExpressions;
using AutoFixture;
using AutoFixture.Kernel;

namespace DomainDrivenDesign.DomainObjects.Persistence.UnitTests.SpecimenBuilders
{
    public class DefinitionBuilder : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            var t = request as Type;
            if (typeof(Definition) != t)
            {
                return new NoSpecimen();
            }

            var result = new Definition
            {
                SourceType = CreateRandomString(),
                TargetNamespace = CreateRandomString(),
                TargetType = CreateRandomString(),
                Keys =
                {
                    { CreateRandomString(), CreateRandomType() },
                    { CreateRandomString(), CreateRandomType() }
                },
                Columns =
                {
                    { CreateRandomString(), CreateRandomType() },
                    { CreateRandomString(), CreateRandomType() },
                    { CreateRandomString(), CreateRandomType() }
                }
            };

            return result;
        }

        private static Type CreateRandomType()
        {
            var fixture = new Fixture();
            var types = new[]
            {
                typeof(string),
                typeof(decimal),
                typeof(Guid),
                typeof(int),
                typeof(bool),
                typeof(double)
            };

            var number = fixture.Create<int>();
            return types[number % types.Length];
        }

        private static string CreateRandomString()
        {
            var fixture = new Fixture();
            var name = fixture.Create<string>();
            
            return Regex.Replace(name, "[0-9\\-]", string.Empty);
        }
    }
}
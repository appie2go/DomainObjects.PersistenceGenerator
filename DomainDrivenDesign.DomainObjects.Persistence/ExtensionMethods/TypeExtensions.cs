using System;

namespace DomainDrivenDesign.DomainObjects.Persistence.ExtensionMethods
{
    public static class TypeExtensions
    {
        public static bool IsNullable(this Type type)
        {
            return !type.IsValueType || (Nullable.GetUnderlyingType(type) != null);
        }
    }
}
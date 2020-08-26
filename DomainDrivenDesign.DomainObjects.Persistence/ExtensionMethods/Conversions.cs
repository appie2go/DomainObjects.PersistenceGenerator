using System;

namespace DomainDrivenDesign.DomainObjects.Persistence.ExtensionMethods
{
    public static class Conversions
    {
        public static bool ToBool(this bool value) => value;
        public static DateTime ToDateTime(this DateTime value) => value;
        public static int ToInt(this int value) => value;
        public static double ToDouble(this double value) => value;
        public static decimal ToDecimal(this decimal value) => value;
        public static Guid ToGuid(this Guid value) => value;
        
        public static bool? ToBool(this bool? value) => value;
        public static DateTime? ToDateTime(this DateTime? value) => value;
        public static int? ToInt(this int? value) => value;
        public static double? ToDouble(this double? value) => value;
        public static decimal? ToDecimal(this decimal? value) => value;
        public static Guid? ToGuid(this Guid? value) => value;
    }
}
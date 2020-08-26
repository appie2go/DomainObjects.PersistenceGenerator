using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DomainDrivenDesign.DomainObjects.Persistence
{
    public class Definition
    {
        public string SourceType { get; set; }
        public string TargetType { get; set; }
        public string TargetNamespace { get; set; }
        public Dictionary<string, Type> Columns { get; private set; } = new Dictionary<string, Type>();
        public Dictionary<string, Type> Keys { get; private set; } = new Dictionary<string, Type>();
        
        public static Definition Parse(string str)
        {
            var result = new Definition();
            
            var lines = str.Split(new[] {'\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var isReadingColumns = false;
            var isReadingKey = false;
            
            foreach (var line in lines)
            {   
                if (line.StartsWith("- sourceType:"))
                {
                    result.SourceType = line.Replace("- sourceType:", string.Empty).Trim();
                }
                
                if (line.StartsWith("  - targetNamespace:"))
                {
                    result.TargetNamespace = line.Replace("  - targetNamespace:", string.Empty).Trim();
                }
  
                if (line.StartsWith("  - entityName:"))
                {
                    result.TargetType = line.Replace("  - entityName:", string.Empty).Trim();
                }
                
                if ((line.StartsWith("   ") || line.StartsWith("    ")) && (isReadingColumns || isReadingKey))
                {
                    var statement = line.Split(new[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries);
                    if (statement.Length != 2)
                    {
                        continue;
                    }

                    var type = ParseType(statement[1]);
                    if (type == null)
                    {
                        throw new ArgumentException($"Error parsing table definition. {statement[1]} is not a valid type. " +
                                                    $"Supply the fully qualified name like System.Guid, or System.String for example.");
                    }

                    if (isReadingKey)
                    {
                        result.Keys.Add(statement[0], type);
                    }
                    
                    result.Columns.Add(statement[0], type);
                    continue;
                }
                
                if (line.StartsWith("  - properties"))
                {
                    isReadingColumns = true;
                    isReadingKey = false;
                    continue;
                }
                
                if (line.StartsWith("  - key"))
                {
                    isReadingKey = true;
                    isReadingColumns = false;
                    continue;
                }
                
                isReadingColumns = false;
                isReadingKey = false;
            }

            return result;
        }

        public bool Validate(out string errorMessage)
        {
            if (SourceType == null)
            {
                errorMessage = $"{nameof(SourceType)} cannot be null.";
                return false;
            }
            
            if (TargetType == null)
            {
                errorMessage = $"{nameof(TargetType)} cannot be null.";
                return false;
            }
            
            if (TargetNamespace == null)
            {
                errorMessage = $"{nameof(TargetNamespace)} cannot be null.";
                return false;
            }
            
            if (!Columns.Any())
            {
                errorMessage = $"{nameof(Columns)} must contain values.";
                return false;
            }

            errorMessage = null;
            return true;
        }
        
        private static Type ParseType(string typeName)
        {
            const string pattern = "Nullable\\<([a-zA-Z\\.]{1,})\\>";
            var isNullable = Regex.Matches(typeName, pattern);
            var type = isNullable.Any()
                ?  $"System.Nullable`1[{isNullable[0].Groups[1].Value}]"
                : typeName;

            return Type.GetType(type);
        }

    }
}
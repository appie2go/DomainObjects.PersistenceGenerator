using System;
using System.IO;
using DomainDrivenDesign.DomainObjects.Persistence.Generator;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace DomainDrivenDesign.DomainObjects.Persistence
{
    public class CodeGeneratorTask : Task
    {
        public override bool Execute()
        {
            var files = Explorer.GetFiles(this.BuildEngine.ProjectFileOfTaskNode);
            foreach (var file in files)
            {
                Log.LogMessage(MessageImportance.High, $"Reading {file}.definition.cs.");
                
                var text = File.ReadAllText(file);
                Definition definition;
                try
                {
                    definition = Definition.Parse(text);
                }
                catch (ArgumentException e)
                {
                    Log.LogError($"ERROR PARSING {file} {e}");
                    continue;
                }

                if (!definition.Validate(out var message))
                {
                    Log.LogError($"INVALID TEMPLATE {file}: {message}");
                    continue;
                }

                var codeBuilder = new CodeBehindFactory(definition.TargetNamespace,
                        new EntityFactory(),
                        new MapperInterfaceFactory(),
                        new MapperFactory()
                    );
                
                Log.LogMessage(MessageImportance.High, $"Generating EF entity, repository and mapper for {file} into {file}.definition.cs.");
                var code = codeBuilder.Create(definition).ToString();
                Log.LogMessage(MessageImportance.High, $"Writing code to {file}.cs.");
                File.WriteAllText($"{file}.cs", code);
            }
            
            return true;
        }
    }
}
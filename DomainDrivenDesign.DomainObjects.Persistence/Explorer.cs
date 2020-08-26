using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DomainDrivenDesign.DomainObjects.Persistence
{
    public static class Explorer
    {
        public static IEnumerable<string> GetFiles(string projectFilePath)
        {
            var projectFile = Path.GetDirectoryName(projectFilePath);
            return ListAllFilesInFolder(projectFile)
                .Where(x => x.EndsWith(".repository", StringComparison.InvariantCultureIgnoreCase))
                .ToArray();
        }
        
        private static IEnumerable<string> ListAllFilesInFolder(string directory)
        {
            var files = Directory.GetFiles(directory).ToList();
            
            var subdirectories = Directory.GetDirectories(directory);
            foreach (var subdirectory in subdirectories)
            {
                var filesInSubdirectory = ListAllFilesInFolder(subdirectory);
                files.AddRange(filesInSubdirectory);
            }

            return files;
        }
    }
}
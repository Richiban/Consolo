using System;
using Richiban.Cmdr.Models;

namespace Richiban.Cmdr.Writers
{
    internal class CmdrAttributeFileGenerator : CodeFileGenerator
    {
        public CmdrAttributeFileGenerator()
        {
            FileName = $"{CmdrAttributeDefinition.LongName}.g.cs";
        }

        public override string FileName { get; }

        public override string GetCode() =>
            $$"""
            using System;

            namespace Cmdr;

            [AttributeUsage(
                AttributeTargets.Method 
                | AttributeTargets.Class
                | AttributeTargets.Parameter,
                Inherited = false,
                AllowMultiple = false)]
            public class {{CmdrAttributeDefinition.LongName}} : Attribute
            {
                public {{CmdrAttributeDefinition.LongName}}(string name = null)
                {
                    Name = name;
                }

                public string Name { get; }

                public string? Description { get; set; }
            }
            """;
    }
}
using System;
using Richiban.Cmdr.Models;

namespace Richiban.Cmdr.Writers
{
    internal class CmdrAttributeFileGenerator : CodeFileGenerator
    {
        private readonly CmdrAttributeDefinition _cmdrAttributeDefinition;

        public CmdrAttributeFileGenerator(CmdrAttributeDefinition cmdrAttributeDefinition)
        {
            _cmdrAttributeDefinition = cmdrAttributeDefinition;
            FileName = $"{_cmdrAttributeDefinition.LongName}.g.cs";
        }

        public override string FileName { get; }

        public override string GetCode() =>
            $$"""
            using System;

            namespace Cmdr;

            [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
            public class {{_cmdrAttributeDefinition.LongName}} : Attribute
            {
                public {{_cmdrAttributeDefinition.LongName}}(string name = null)
                {
                    Name = name;
                }

                public string Name { get; }
            }
            """;
    }
}
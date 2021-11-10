using System;
using System.Collections.Generic;

namespace Richiban.Cmdr.Generators
{
    internal class CmdrAttributeFileGenerator : ICodeFileGenerator
    {
        private readonly CmdrAttributeDefinition _cmdrAttributeDefinition;

        public CmdrAttributeFileGenerator(CmdrAttributeDefinition cmdrAttributeDefinition)
        {
            _cmdrAttributeDefinition = cmdrAttributeDefinition;
        }

        public string GetCode() =>
            @$"using System;

namespace {_cmdrAttributeDefinition.Namespace}
{{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class {_cmdrAttributeDefinition.LongName} : Attribute
    {{
        public {_cmdrAttributeDefinition.LongName}(string name)
        {{
            Name = name;
        }}

        public string Name {{ get; }}
    }}
}}";
    }
}
using System;
using Microsoft.CodeAnalysis;

namespace Richiban.Cmdr.Writers
{
    class CmdrAttributeWriter : CodeWriter
    {
        private readonly CmdrAttribute _cmdrAttribute;
        private static string _fileName = "CmdrMethodAttribute.g.cs";

        public CmdrAttributeWriter(
            CmdrAttribute cmdrAttribute,
            GeneratorExecutionContext context) : base(_fileName, context)
        {
            _cmdrAttribute = cmdrAttribute;
        }

        protected override string GetCode() =>
            @$"
namespace {_cmdrAttribute.Namespace}
{{
    [System.AttributeUsage(System.AttributeTargets.Method|System.AttributeTargets.Class, AllowMultiple = false)]
    public class {_cmdrAttribute.AttributeName} : System.Attribute
    {{
        public {_cmdrAttribute.AttributeName}(string alias)
        {{
            Alias = alias;
        }} 

        public string Alias {{ get; }}
    }}
}}";
    }
}
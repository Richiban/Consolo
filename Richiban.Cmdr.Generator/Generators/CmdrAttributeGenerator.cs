using System;
using System.Collections.Generic;

namespace Richiban.Cmdr.Generators
{
    internal class CmdrAttributeGenerator : ICodeGenerator
    {
        private readonly CmdrAttribute _cmdrAttribute;

        public CmdrAttributeGenerator(CmdrAttribute cmdrAttribute)
        {
            _cmdrAttribute = cmdrAttribute;
        }

        public IEnumerable<string> GetCodeLines() => Array.Empty<string>(); // TODO disabled for now

        public string GetCodeBk() =>
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
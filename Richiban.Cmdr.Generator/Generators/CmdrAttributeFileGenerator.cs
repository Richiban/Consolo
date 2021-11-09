using System;
using System.Collections.Generic;

namespace Richiban.Cmdr.Generators
{
    internal class CmdrAttributeFileGenerator : ICodeFileGenerator
    {
        private readonly CmdrAttribute _cmdrAttribute;

        public CmdrAttributeFileGenerator(CmdrAttribute cmdrAttribute)
        {
            _cmdrAttribute = cmdrAttribute;
        }

        public string GetCode() =>""; // TODO disabled for now

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
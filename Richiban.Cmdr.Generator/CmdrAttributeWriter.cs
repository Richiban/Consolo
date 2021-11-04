using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Richiban.Cmdr.Generator
{
    class CmdrAttributeWriter
    {
        private readonly GeneratorExecutionContext _context;

        public CmdrAttributeWriter(GeneratorExecutionContext context)
        {
            _context = context;
        }

        public string Name { get; } = "CmdrMethod";

        public void WriteToContext()
        {
            var code = @$"
namespace Richiban.Cmdr
{{
    public class {Name} : System.Attribute {{}}
}}";

            _context.AddSource(
                "CmdrMethodAttribute.g.cs",
                SourceText.From(code, Encoding.UTF8));
        }
    }
}
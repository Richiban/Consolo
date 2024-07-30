using System;

namespace Consolo;

internal class ConsoloAttributeFileGenerator : CodeFileGenerator
{
    public ConsoloAttributeFileGenerator()
    {
        FileName = $"{ConsoloAttributeDefinition.LongName}.g.cs";
    }

    public override string FileName { get; }

    public override string GetCode() =>
        $$"""
        using System;

        namespace Consolo
        {
            [AttributeUsage(
                AttributeTargets.Method 
                | AttributeTargets.Class
                | AttributeTargets.Parameter,
                Inherited = false,
                AllowMultiple = false)]
            internal class {{ConsoloAttributeDefinition.LongName}} : Attribute
            {
                public {{ConsoloAttributeDefinition.LongName}}(string name = null)
                {
                }

                public string Alias { get; set; }
            }
        }
        """;
}
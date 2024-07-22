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

        namespace Consolo;

        #nullable enable
        [AttributeUsage(
            AttributeTargets.Method 
            | AttributeTargets.Class
            | AttributeTargets.Parameter,
            Inherited = false,
            AllowMultiple = false)]
        internal class {{ConsoloAttributeDefinition.LongName}} : Attribute
        {
            public {{ConsoloAttributeDefinition.LongName}}(params string[] names)
            {
                Names = names;
            }

            public string[] Names { get; }

            public string? ShortForm { get; set; }
        }
        
        #nullable disable
        """;
}
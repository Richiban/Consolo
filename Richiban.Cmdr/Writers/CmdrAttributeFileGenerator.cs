using System;

namespace Richiban.Cmdr;

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

        #nullable enable
        [AttributeUsage(
            AttributeTargets.Method 
            | AttributeTargets.Class
            | AttributeTargets.Parameter,
            Inherited = false,
            AllowMultiple = false)]
        internal class {{CmdrAttributeDefinition.LongName}} : Attribute
        {
            public {{CmdrAttributeDefinition.LongName}}(params string[] names)
            {
                Names = names;
            }

            public string[] Names { get; }

            public string? ShortForm { get; set; }
        }
        
        #nullable disable
        """;
}
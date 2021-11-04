using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Richiban.Cmdr.Generator
{
    [Generator]
    public class CmdrGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                var cmdrAttributeWriter = new CmdrAttributeWriter(context);
                cmdrAttributeWriter.WriteToContext();

                var methods =
                    new MethodModelBuilder(context).GetMethods(cmdrAttributeWriter);

                new ProgramClassWriter(context).WriteToContext(methods);

                new ReplWriter(context).WriteToContext();
            }
            catch (Exception)
            {
                Debugger.Launch();
            }
        }
    }
}
using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Richiban.Cmdr.Writers;

namespace Richiban.Cmdr
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
                var cmdrAttribute = new CmdrAttribute();
                
                new CmdrAttributeWriter(cmdrAttribute, context).WriteToContext();

                var methods =
                    new MethodModelBuilder(context, cmdrAttribute).GetMethods();

                new ProgramClassWriter(context).WriteToContext(methods);

                new ReplWriter(context, cmdrAttribute).WriteToContext();
            }
            catch (Exception)
            {
                Debugger.Launch();
            }
        }
    }
}
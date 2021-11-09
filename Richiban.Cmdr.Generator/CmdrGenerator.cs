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

                var methods = new MethodModelContextBuilder(context, cmdrAttribute).Build();

                new ProgramClassWriter(context, methods).WriteToContext();

                new ReplWriter(context, cmdrAttribute).WriteToContext();
            }
            catch (Exception ex)
            {
                //Debugger.Launch();

                context.ReportDiagnostic(
                    Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "Cmdr0004",
                            "Failed!",
                            $"There was a failure: {ex.Message}",
                            "Error",
                            DiagnosticSeverity.Error,
                            isEnabledByDefault: true),
                        location: null));
            }
        }
    }
}
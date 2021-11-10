using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
                var cmdrAttribute = new CmdrAttributeDefinition();

                new CmdrAttributeWriter(cmdrAttribute, context).WriteToContext();

                var methods =
                    new MethodModelContextBuilder(context, cmdrAttribute).Build();

                new ProgramClassWriter(context, methods).WriteToContext();

                new ReplWriter(context, cmdrAttribute).WriteToContext();
            }
            catch (Exception ex)
            {
                //Debugger.Launch();

                context.ReportDiagnostic(
                    Diagnostic.Create(
                        new DiagnosticDescriptor(
                            id: "Cmdr0004",
                            title: "Unhandled exception",
                            messageFormat:
                            $"There was an unhandled exception: {ex.Message}",
                            category: "Cmdr",
                            DiagnosticSeverity.Error,
                            isEnabledByDefault: true),
                        location: null));
            }
        }
    }
}
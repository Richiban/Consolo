using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Richiban.Cmdr.Models;
using Richiban.Cmdr.Transformers;
using Richiban.Cmdr.Utils;
using Richiban.Cmdr.Writers;

namespace Richiban.Cmdr
{
    [Generator]
    public class CmdrGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForPostInitialization(InjectStaticSourceFiles);

            context.RegisterForSyntaxNotifications(
                () => new CmdrSyntaxReceiver());
        }

        private void InjectStaticSourceFiles(
            GeneratorPostInitializationContext postInitializationContext)
        {
            var cmdrAttributeFileGenerator =
                new CmdrAttributeFileGenerator();

            postInitializationContext.AddSource(
                cmdrAttributeFileGenerator.FileName,
                cmdrAttributeFileGenerator.GetCode());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var diagnostics = new CmdrDiagnostics(context);

            try
            {
                if (context.SyntaxReceiver is not CmdrSyntaxReceiver receiver ||
                    receiver.QualifyingMembers.Count == 0)
                {
                    return;
                }

                var candidateMethods =
                    new MethodScanner(context.Compilation, diagnostics)
                        .GetCandidateMethods(receiver.QualifyingMembers);

                var (methodModels, failures) = new MethodModelBuilder()
                    .BuildFrom(candidateMethods)
                    .SeparateResults();

                diagnostics.ReportMethodFailures(failures);

                var rootCommandModel =
                    new CommandModelTransformer().Transform(methodModels);

                context.AddCodeFile(new ProgramClassFileGenerator(rootCommandModel));
            }
            catch (Exception ex)
            {
                diagnostics.ReportUnknownError(ex);
            }
        }

        private class CmdrSyntaxReceiver : ISyntaxReceiver
        {
            internal List<MethodDeclarationSyntax> QualifyingMembers { get; } = new();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is not MethodDeclarationSyntax method)
                {
                    return;
                }

                var attribute = method.AttributeLists.SelectMany(
                        list => list.Attributes.Where(x => CmdrAttributeDefinition.Matches(x)))
                    .FirstOrDefault();

                if (attribute is null)
                {
                    return;
                }

                QualifyingMembers.Add(method);
            }
        }
    }
}
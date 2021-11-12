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
        private CmdrAttributeDefinition _cmdrAttribute = null!;

        public void Initialize(GeneratorInitializationContext context)
        {
            _cmdrAttribute = new CmdrAttributeDefinition();

            context.RegisterForPostInitialization(InjectStaticSourceFiles);

            context.RegisterForSyntaxNotifications(
                () => new CmdrSyntaxReceiver(_cmdrAttribute));
        }

        private void InjectStaticSourceFiles(
            GeneratorPostInitializationContext postInitializationContext)
        {
            var cmdrAttributeFileGenerator =
                new CmdrAttributeFileGenerator(_cmdrAttribute);

            var replFileGenerator = new ReplFileGenerator();

            postInitializationContext.AddSource(
                cmdrAttributeFileGenerator.FileName,
                cmdrAttributeFileGenerator.GetCode());

            postInitializationContext.AddSource(
                replFileGenerator.FileName,
                replFileGenerator.GetCode());
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
                    new MethodScanner(context.Compilation, _cmdrAttribute, diagnostics)
                        .GetCandidateMethods(receiver.QualifyingMembers);

                var (methodModels, failures) = new MethodModelBuilder(_cmdrAttribute)
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
            private readonly CmdrAttributeDefinition _cmdrAttribute;

            public CmdrSyntaxReceiver(CmdrAttributeDefinition cmdrAttribute)
            {
                _cmdrAttribute = cmdrAttribute;
            }

            internal List<MethodDeclarationSyntax> QualifyingMembers { get; } = new();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is not MethodDeclarationSyntax method)
                {
                    return;
                }

                var attribute = method.AttributeLists.SelectMany(
                        list => list.Attributes.Where(x => _cmdrAttribute.Matches(x)))
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
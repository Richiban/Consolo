using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Richiban.Cmdr.Models;
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

            context.RegisterForSyntaxNotifications(
                () => new CmdrSyntaxReceiver(_cmdrAttribute));
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var diagnostics = new CmdrDiagnostics(context);

            try
            {
                if (context.SyntaxReceiver is not CmdrSyntaxReceiver
                {
                    QualifyingMembers: { Count: > 0 }
                } receiver)
                {
                    return;
                }

                DoGeneration(context, diagnostics, receiver.QualifyingMembers);
            }
            catch (Exception ex)
            {
                diagnostics.ReportUnknownError(ex);
            }
        }

        private void DoGeneration(
            GeneratorExecutionContext context,
            CmdrDiagnostics cmdrDiagnostics,
            List<MethodDeclarationSyntax> qualifyingMembers)
        {
            var cmdrAttributeFileGenerator =
                new CmdrAttributeFileGenerator(_cmdrAttribute);

            context.AddCodeFile(cmdrAttributeFileGenerator);
            context.AddCodeFile(new ReplFileGenerator());

            var options = ((CSharpCompilation)context.Compilation).SyntaxTrees[0]
                .Options;

            var attributeTree = CSharpSyntaxTree.ParseText(
                cmdrAttributeFileGenerator.GetCode(),
                (CSharpParseOptions)options);

            var newCompilation =
                context.Compilation.AddSyntaxTrees(attributeTree);

            var candidateMethods =
                new MethodScanner(newCompilation, _cmdrAttribute, cmdrDiagnostics)
                    .GetCandidateMethods(qualifyingMembers);

            var (methodModels, failures) = new MethodModelBuilder(_cmdrAttribute)
                .BuildFrom(candidateMethods)
                .SeparateResults();

            cmdrDiagnostics.ReportMethodFailures(failures);

            context.AddCodeFile(new ProgramClassFileGenerator(methodModels));
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
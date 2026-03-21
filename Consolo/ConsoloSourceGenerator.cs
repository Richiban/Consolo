using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharp.CSharpExtensions;

namespace Consolo;

[Generator]
public class ConsoloSourceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForPostInitialization(InjectStaticSourceFiles);

        context.RegisterForSyntaxNotifications(() => new ConsoloSyntaxReceiver());
    }

    private void InjectStaticSourceFiles(
        GeneratorPostInitializationContext postInitializationContext)
    {
        var consoloAttributeFileGenerator = new ConsoloAttributeFileGenerator();

        postInitializationContext.AddSource(
            consoloAttributeFileGenerator.FileName,
            consoloAttributeFileGenerator.GetCode());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var diagnosticsManager = new ConsoloDiagnosticsManager(context);

        NewMethod(context, diagnosticsManager);
    }

    private static void NewMethod(
        GeneratorExecutionContext context,
        ConsoloDiagnosticsManager diagnosticsManager)
    {
        try
        {
            if (context.SyntaxReceiver is not ConsoloSyntaxReceiver receiver)
            {
                return;
            }

            if (!ConsoloSourceGenerator2
                    .GenerateCommandTree(context, diagnosticsManager, receiver)
                    .IsSome(out var rootCommandModel))
            {
                return;
            }

            AddCodeFile(context, rootCommandModel);
        }
        catch (Exception ex)
        {
            diagnosticsManager.ReportUnknownError(ex);
        }
    }

    private static void AddCodeFile(
        GeneratorExecutionContext context,
        CommandTree.Root rootCommandModel)
    {
        var assemblyName = context.Compilation.AssemblyName;
        var assemblyTitle = GetAssemblyAttributeString(
            context.Compilation,
            "System.Reflection.AssemblyTitleAttribute");
        var assemblyDescription = GetAssemblyAttributeString(
            context.Compilation,
            "System.Reflection.AssemblyDescriptionAttribute");
        var generatedNamespace = $"{assemblyName ?? "Consolo"}.g";

        if (rootCommandModel.Description.IsNone && !String.IsNullOrWhiteSpace(assemblyDescription))
        {
            rootCommandModel.Description = assemblyDescription.Trim();
        }

        var applicationName =
            !String.IsNullOrWhiteSpace(assemblyTitle)
                ? assemblyTitle!.Trim()
                : assemblyName ?? "Unknown assembly";

        context.AddCodeFile(
            new ProgramClassFileGenerator(
                applicationName, generatedNamespace, rootCommandModel));
    }

    private static string? GetAssemblyAttributeString(
        Compilation compilation,
        string attributeMetadataName)
    {
        var attribute = compilation.Assembly
            .GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == attributeMetadataName);

        if (attribute is null || attribute.ConstructorArguments.Length == 0)
        {
            return null;
        }

        return attribute.ConstructorArguments[0].Value as string;
    }
}
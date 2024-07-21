using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace Richiban.Cmdr;

internal class CommandModelTreeBuilder(CmdrDiagnosticsManager diagnosticsManager)
{
    public ResultWithDiagnostics<CommandTree.Root> Transform(IEnumerable<MethodModel> methodModels)
    {
        var root = new CommandTree.Root();
        var diagnostics = new List<DiagnosticModel>();

        foreach (var methodModel in methodModels)
        {
            Set(
                root,
                new ListWalker<CommandPathItem>(methodModel.ParentCommandPath, diagnosticsManager),
                methodModel,
                diagnostics);
        }

        return new ResultWithDiagnostics<CommandTree.Root>(root, diagnostics);
    }

    private static void Set(
        CommandTree commandModel,
        ListWalker<CommandPathItem> pathWalker,
        MethodModel methodModel,
        List<DiagnosticModel> diagnostics)
    {
        var currentName = methodModel.ProvidedName.GetValueOrDefault(() => pathWalker.Current.Name);

        if (pathWalker.AtEnd)
        {
            if (methodModel.ProvidedName == "")
            {
                commandModel.Method = MapMethod(methodModel);
                commandModel.Description = methodModel.Description;
                commandModel.Parameters = MapParameters(methodModel.Parameters);

                return;
            }

            if (commandModel.SubCommands.Any(t => t.CommandName == StringUtils.ToKebabCase(currentName)))
            {
                diagnostics.Add(
                    DiagnosticModel.CommandNameAlreadyInUse(
                        currentName, 
                        methodModel.Location.GetValueOrDefault(defaultValue: null!)
                    )
                );

                return;
            }

            commandModel.SubCommands.Add(
                new CommandTree.SubCommand(StringUtils.ToKebabCase(currentName))
                {
                    Method = MapMethod(methodModel),
                    Description = methodModel.Description,
                    Parameters = MapParameters(methodModel.Parameters),
                });

            return;
        }

        var (current, remaining) = pathWalker;
        var subTreeWasMatched = false;

        foreach (var tree in commandModel.SubCommands.Where(
            t => t.CommandName == current.Name))
        {
            subTreeWasMatched = true;

            Set(tree, remaining, methodModel, diagnostics);
        }

        if (subTreeWasMatched)
        {
            return;
        }

        var newSubTree = new CommandTree.SubCommand(StringUtils.ToKebabCase(current.Name))
        {
            Description = current.XmlComment,
        };

        Set(newSubTree, remaining, methodModel, diagnostics);
        commandModel.SubCommands.Add(newSubTree);
    }

    private static CommandMethod MapMethod(MethodModel methodModel) =>
        new CommandMethod(
            methodModel.FullyQualifiedClassName,
            methodModel.MethodName);

    private static IReadOnlyCollection<CommandParameter> MapParameters(
        IReadOnlyCollection<ParameterModel> methodModelArguments)
    {
        return methodModelArguments.Select(MapParameter).ToList();
    }

    private static CommandParameter MapParameter(ParameterModel arg) =>
        arg.IsFlag
            ? new CommandParameter.Flag(
                name: arg.Name,
                shortForm: arg.ShortForm,
                description: arg.Description,
                originalName: arg.OriginalName)
            : arg.IsRequired 
                ? new CommandParameter.Positional(
                    name: arg.Name,
                    originalName: arg.OriginalName,
                    type: MapType(arg.Type),
                    description: arg.Description)
                : new CommandParameter.OptionalPositional(
                    name: arg.Name,
                    type: MapType(arg.Type),
                    defaultValue: arg.DefaultValue | "default",
                    description: arg.Description,
                    originalName: arg.OriginalName
                );

    private static ParameterType MapType(ITypeSymbol type)
    {
        switch (type)
        {
            case INamedTypeSymbol t when t.HasParseMethod():
                return new ParameterType.Parse(type, "Parse");
            case INamedTypeSymbol t when t.HasExplicitCastFromString():
                return new ParameterType.ExplicitCast(type);
            case INamedTypeSymbol t when t.HasConstructorWithSingleStringParameter():
                return new ParameterType.Constructor(type);
            default:
                return new ParameterType.AsIs(type);
        }
    }

    private readonly struct ListWalker<T>
    {
        private readonly CmdrDiagnosticsManager _diagnosticsManager;

        public ListWalker(IReadOnlyList<T> list, CmdrDiagnosticsManager diagnosticsManager)
            : this(list, index: 0, diagnosticsManager)
        {
            _diagnosticsManager = diagnosticsManager;
        }

        private ListWalker(IReadOnlyList<T> list, int index, CmdrDiagnosticsManager diagnosticsManager)
        {
            List = list;
            Index = index;
            _diagnosticsManager = diagnosticsManager;
        }

        public int Index { get; }
        public IReadOnlyList<T> List { get; }
        public T Current => List[Index];
        public ListWalker<T> Next => new(List, Index + 1, _diagnosticsManager);
        public bool AtEnd => Index >= List.Count;

        public void Deconstruct(out T current, out ListWalker<T> next)
        {
            current = Current;
            next = Next;
        }
    }
}
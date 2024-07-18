using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Richiban.Cmdr;

internal class CommandModelTreeBuilder(CmdrDiagnosticsManager diagnosticsManager)
{
    public CommandTree.Root Transform(IEnumerable<MethodModel> methodModels)
    {
        var root = new CommandTree.Root();

        foreach (var methodModel in methodModels)
        {
            Set(
                root,
                new ListWalker<CommandPathItem>(methodModel.ParentCommandPath, diagnosticsManager),
                methodModel);
        }

        return root;
    }

    private static void Set(
        CommandTree commandModel,
        ListWalker<CommandPathItem> pathWalker,
        MethodModel methodModel)
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

            Set(tree, remaining, methodModel);
        }

        if (subTreeWasMatched)
        {
            return;
        }

        var newSubTree = new CommandTree.SubCommand(StringUtils.ToKebabCase(current.Name))
        {
            Description = current.XmlComment,
        };

        Set(newSubTree, remaining, methodModel);
        commandModel.SubCommands.Add(newSubTree);
    }

    private static CommandMethod MapMethod(MethodModel methodModel) =>
        new CommandMethod(
            methodModel.FullyQualifiedClassName,
            methodModel.MethodName);

    private static IReadOnlyCollection<CommandParameterModel> MapParameters(
        IReadOnlyCollection<ParameterModel> methodModelArguments)
    {
        return methodModelArguments.Select(MapParameter).ToList();
    }

    private static CommandParameterModel MapParameter(ParameterModel arg) =>
        arg.IsFlag
            ? new CommandParameterModel.CommandFlagModel(
                arg.Name,
                arg.ShortForm,
                arg.Description)
            : arg.IsRequired 
                ? new CommandParameterModel.CommandPositionalParameterModel(
                    arg.Name,
                    arg.FullyQualifiedTypeName,
                    arg.Description)
                : new CommandParameterModel.CommandOptionalPositionalParameterModel(
                    arg.Name,
                    arg.FullyQualifiedTypeName,
                    arg.DefaultValue | "default",
                    arg.Description
                );

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
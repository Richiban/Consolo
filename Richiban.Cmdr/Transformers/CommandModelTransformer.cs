﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Richiban.Cmdr;

internal class CommandModelTransformer(CmdrDiagnosticsManager diagnosticsManager)
{
    public CommandModel.RootCommandModel Transform(
        IEnumerable<MethodModel> methodModels)
    {
        var root = new CommandModel.RootCommandModel();

        foreach (var methodModel in methodModels)
        {
            Set(
                root,
                new ListWalker<CommandPathItem>(methodModel.GroupCommandPath, diagnosticsManager),
                methodModel);
        }

        return root;
    }

    private static void Set(
        CommandModel commandModel,
        ListWalker<CommandPathItem> pathWalker,
        MethodModel methodModel)
    {
        var currentName = methodModel.ProvidedName.GetValueOrDefault(() => pathWalker.Current.Name);

        if (pathWalker.AtEnd)
        {
            if (methodModel.ProvidedName == "")
            {
                commandModel.Method = MapMethod(methodModel);

                return;
            }

            commandModel.SubCommands.Add(
                new CommandModel.SubCommandModel(StringUtils.ToKebabCase(currentName))
                {
                    Method = MapMethod(methodModel),
                    Description = methodModel.Description,
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

        var newSubTree = new CommandModel.SubCommandModel(StringUtils.ToKebabCase(current.Name))
        {
            Description = current.XmlComment,
        };

        Set(newSubTree, remaining, methodModel);
        commandModel.SubCommands.Add(newSubTree);
    }

    private static CommandMethod MapMethod(MethodModel methodModel) =>
        new CommandMethod(
            methodModel.FullyQualifiedClassName,
            methodModel.MethodName,
            MapParameters(methodModel.Parameters));

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
            : new CommandParameterModel.CommandPositionalParameterModel(
                arg.Name,
                arg.FullyQualifiedTypeName,
                arg.IsRequired,
                arg.DefaultValue,
                arg.Description);

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
            _diagnosticsManager.ReportDiagnostic(
                new DiagnosticModel(
                    $"ListWalker created with index {new {index, count = list.Count}}",
                    Location: null,
                    DiagnosticSeverity.Warning));
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
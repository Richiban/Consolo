using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Richiban.Cmdr.Models;

namespace Richiban.Cmdr.Transformers
{
    internal class CommandModelTransformer
    {
        public CommandModel.RootCommandModel Transform(
            IEnumerable<MethodModel> methodModels)
        {
            var tree = Group(methodModels);

            return (CommandModel.RootCommandModel)Map(tree, isRoot: true);
        }

        private CommandModel Map(CommandTree tree, bool isRoot)
        {
            if (isRoot)
            {
                return new CommandModel.RootCommandModel(
                    tree.SubTrees.Select(tree1 => Map(tree1, isRoot: false))
                        .ToImmutableArray());
            }

            {
                var commandText = Utils.ToKebabCase(tree.CommandText);
                var commandText = Utils.ToKebabCase(primaryName);

                var x = tree.SubTrees.Select(tree1 => Map(tree1, isRoot: false))
                    .ToImmutableArray();

                var methodModel = tree.MethodModel!;

                var primaryName = methodModel switch
                {
                    { ProvidedName: { } name } => name,
                    { MethodName: var methodName } => methodName
                };

                var commandParameterModels = MapCommandParameterModels(methodModel);

                return new CommandModel.NormalCommandModel(
                    commandText,
                    methodModel.FullyQualifiedClassName,
                    methodModel.MethodName,
                    commandParameterModels);
            }
        }

        private static CommandParameterModel[] MapCommandParameterModels(
            MethodModel treeMethodModel)
        {
            CommandParameterModel transformParameter(ArgumentModel argumentModel) =>
                argumentModel.IsFlag
                    ? new CommandParameterModel.CommandFlagParameterModel(
                        Utils.ToKebabCase(argumentModel.Name))
                    : new CommandParameterModel.CommandPositionalParameterModel(
                        Utils.ToKebabCase(argumentModel.Name),
                        argumentModel.FullyQualifiedTypeName);

            return treeMethodModel.Arguments.Select(transformParameter).ToArray();
        }

        private CommandTree Group(IEnumerable<MethodModel> models)
        {
            var root = new CommandTree("");

            foreach (var methodModel in models)
            {
                root.Set(new ListWalker<string>(methodModel.GroupCommandPath), methodModel);
            }

            return root;
        }

        private readonly struct ListWalker<T>
        {
            public ListWalker(IReadOnlyList<T> list) : this(list, index: 0)
            {
            }

            private ListWalker(IReadOnlyList<T> list, int index)
            {
                List = list;
                Index = index;
            }

            public int Index { get; }
            public IReadOnlyList<T> List { get; }
            public T Current => List[Index];
            public ListWalker<T> Next => new(List, Index + 1);
            public bool AtEnd => Index >= List.Count;

            public void Deconstruct(out T current, out ListWalker<T> next)
            {
                current = Current;
                next = Next;
            }
        }

        private class CommandTree
        {
            public CommandTree(MethodModel methodModel)
            {
                MethodModel = methodModel;
                CommandText = "";
            }

            public CommandTree(string commandText)
            {
                CommandText = commandText;
            }

            public MethodModel? MethodModel { get; }
            public string CommandText { get; }

            public List<CommandTree> SubTrees { get; } = new();

            public void Set(in ListWalker<string> pathWalker, MethodModel value)
            {
                if (pathWalker.AtEnd)
                {
                    SubTrees.Add(new CommandTree(value));

                    return;
                }

                var (current, remaining) = pathWalker;
                var subTreeWasMatched = false;

                foreach (var tree in SubTrees.Where(t => t.CommandText == current))
                {
                    subTreeWasMatched = true;

                    tree.Set(remaining, value);
                }

                if (subTreeWasMatched)
                {
                    return;
                }

                var newSubTree = new CommandTree(current);
                newSubTree.Set(remaining, value);
                SubTrees.Add(newSubTree);
            }
        }
    }
}
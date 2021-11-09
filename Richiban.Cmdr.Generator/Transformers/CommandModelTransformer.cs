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

            if (tree.SubTrees.Any())
            {
                var commandText = Utils.ToKebabCase(tree.CommandText);

                return new CommandModel.CommandGroupModel(
                    commandText,
                    tree.SubTrees.Select(tree1 => Map(tree1, isRoot: false))
                        .ToImmutableArray());
            }
            else
            {
                var methodModel = tree.MethodModel!;

                var primaryName = methodModel switch
                {
                    { ProvidedName: { } name } => name,
                    { MethodName: var methodName } => methodName
                };

                var commandText = Utils.ToKebabCase(primaryName);

                var commandParameterModels = MapCommandParameterModels(methodModel);

                return new CommandModel.LeafCommandModel(
                    commandText,
                    methodModel.FullyQualifiedClassName,
                    primaryName,
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
                root[new ListWalker<string>(methodModel.GroupCommandPath)] = methodModel;
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

            public MethodModel this[in ListWalker<string> path]
            {
                set
                {
                    if (path.AtEnd)
                    {
                        SubTrees.Add(new CommandTree(value));

                        return;
                    }

                    var (current, remaining) = path;
                    var subTreeWasMatched = false;

                    foreach (var tree in SubTrees.Where(t => t.CommandText == current))
                    {
                        subTreeWasMatched = true;

                        tree[remaining] = value;
                    }

                    if (subTreeWasMatched)
                    {
                        return;
                    }

                    SubTrees.Add(new CommandTree(current) { [remaining] = value });
                }
            }
        }
    }
}
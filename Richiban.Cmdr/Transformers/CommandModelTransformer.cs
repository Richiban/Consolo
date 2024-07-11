using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using Richiban.Cmdr.Models;
using Richiban.Cmdr.Utils;

namespace Richiban.Cmdr.Transformers
{
    internal class CommandModelTransformer
    {
        public CommandModel.RootCommandModel Transform(
            IEnumerable<MethodModel> methodModels)
        {
            var root = new CommandModel.RootCommandModel();

            foreach (var methodModel in methodModels)
            {
                Set(
                    root,
                    new ListWalker<CommandPathItem>(methodModel.GroupCommandPath),
                    methodModel);
            }

            return root;
        }

        private static void Set(
            CommandModel commandModel,
            in ListWalker<CommandPathItem> pathWalker,
            MethodModel methodModel)
        {
            var currentName = methodModel.ProvidedName ?? pathWalker.Current.Name;

            if (pathWalker.AtEnd)
            {
                if (methodModel.ProvidedName == "")
                {
                    commandModel.Method = MapMethod(methodModel);

                    return;
                }

                commandModel.SubCommands.Add(
                    new CommandModel.SubCommandModel
                    {
                        CommandName = StringUtils.ToKebabCase(currentName),
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

            var newSubTree = new CommandModel.SubCommandModel
            {
                CommandName = StringUtils.ToKebabCase(current.Name),
                Description = current.Description,
            };

            Set(newSubTree, remaining, methodModel);
            commandModel.SubCommands.Add(newSubTree);
        }

        private static CommandMethod MapMethod(MethodModel methodModel) =>
            new CommandMethod(
                methodModel.FullyQualifiedClassName,
                methodModel.MethodName,
                MapParameters(methodModel.Arguments));

        private static IReadOnlyCollection<CommandParameterModel> MapParameters(
            IReadOnlyCollection<ArgumentModel> methodModelArguments)
        {
            return methodModelArguments.Select(MapParameter).ToList();
        }

        private static CommandParameterModel MapParameter(ArgumentModel arg) =>
            arg.IsFlag
                ? new CommandParameterModel.CommandFlagModel(
                    arg.Name,
                    arg.Description)
                : new CommandParameterModel.CommandPositionalParameterModel(
                    arg.Name,
                    arg.FullyQualifiedTypeName,
                    arg.IsRequired,
                    arg.DefaultValue,
                    arg.Description);

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
    }
}
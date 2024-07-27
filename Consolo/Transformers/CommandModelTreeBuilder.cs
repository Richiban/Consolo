using Microsoft.CodeAnalysis;
using static Consolo.Prelude;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Consolo;

/// <summary>
/// The <see cref="CommandTreeBuilder"/> is responsible for taking a collection 
/// of <see cref="MethodModel"/>s, each with a flat sequence of command names,
/// and transforming them into a tree-based structure with <see cref="CommandTree.Root"/>
/// as the root
/// </summary>
/// <remarks> 
/// For example, the models:
/// a, b, c
/// a, b, d
/// a, c, e
/// 
/// Would be transformed into:
/// a
///  ├  b
///  │  ├ c
///  │  └ d
///  └ c
///    └ e
/// </remarks>
class CommandTreeBuilder
{
    public ResultWithDiagnostics<CommandTree.Root> Transform(IEnumerable<MethodModel> methodModels)
    {
        var root = new CommandTree.Root();
        var diagnostics = new List<DiagnosticModel>();

        // foreach (var methodModel in methodModels)
        // {
        //     Set(
        //         root,
        //         new ListWalker<CommandPathItem>(methodModel.ParentCommandPath, diagnosticsManager),
        //         methodModel,
        //         diagnostics);
        // }

        foreach (var methodModel in methodModels)
        {
            CommandTree currentLevel = root;
            var currentPath = GetPath(methodModel);

            foreach (var (pathEntry, i) in currentPath.Select((a, b) => (a, b)))
            {
                if (pathEntry.Name == "")
                {
                    currentLevel.Method = MapMethod(methodModel, diagnostics);
                    currentLevel.Description = methodModel.Description;
                    continue;
                }

                var sub = currentLevel.SubCommands.FirstOrDefault(it => it.CommandName == pathEntry.Name);

                switch (sub)
                {
                    case null when i == currentPath.Count - 1:
                        {
                            var newLevel = new CommandTree.SubCommand(StringUtils.ToKebabCase(pathEntry.Name))
                            {
                                Method = MapMethod(methodModel, diagnostics),
                                Description = methodModel.Description,
                            };

                            currentLevel.SubCommands.Add(newLevel);
                            currentLevel = newLevel;
                            break;
                        }

                    case null:
                        {
                            var newLevel = new CommandTree.SubCommand(StringUtils.ToKebabCase(pathEntry.Name))
                            {
                                Description = methodModel.Description,
                            };

                            currentLevel.SubCommands.Add(newLevel);
                            currentLevel = newLevel;
                            break;
                        }

                    default:
                        currentLevel = sub;
                        break;
                }
            }
        }

        IReadOnlyList<CommandPathItem> GetPath(MethodModel methodModel)
        {
            var pathItems = methodModel.ParentCommandPath.ToList();
            var newItem = new CommandPathItem(
                Name: methodModel.ProvidedName | methodModel.MethodName,
                XmlComment: methodModel.Description
            );

            if (methodModel.ProvidedName == "")
            {
                var last = pathItems.LastOrDefault();

                if (last is not null)
                {
                    pathItems.RemoveAt(pathItems.Count - 1);
                    newItem = new CommandPathItem(
                        Name: last.Name,
                        XmlComment: last.XmlComment + "\n" + methodModel.Description
                    );
                }
            }

            pathItems.Add(newItem);

            return pathItems;
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
                commandModel.Method = MapMethod(methodModel, diagnostics);
                commandModel.Description = methodModel.Description;

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
                    Method = MapMethod(methodModel, diagnostics),
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

    private static CommandMethod MapMethod(MethodModel methodModel, List<DiagnosticModel> diagnostics) =>
        new CommandMethod(
            FullyQualifiedClassName: methodModel.FullyQualifiedClassName,
            MethodName: methodModel.MethodName,
            Parameters: MapParameters(methodModel.Parameters, diagnostics),
            Description: methodModel.Description);

    private static IReadOnlyCollection<CommandParameter> MapParameters(
        IReadOnlyCollection<ParameterModel> methodModelArguments,
        List<DiagnosticModel> diagnostics) => methodModelArguments
            .Select(arg => MapParameter(arg, diagnostics))
            .ToList();

    private static CommandParameter MapParameter(ParameterModel param, List<DiagnosticModel> diagnostics) =>
        param.IsRequired
            ? new CommandParameter.Positional(
                name: param.Name,
                sourceName: param.OriginalName,
                type: MapType(param, diagnostics),
                description: param.Description | param.Type.Name)
            : new CommandParameter.Option(
                name: param.Name,
                shortForm: param.ShortForm,
                type: MapType(param, diagnostics),
                defaultValue: param.DefaultValue | "default",
                description: param.Description | param.Type.Name,
                sourceName: param.OriginalName,
                isFlag: param.IsFlag
            );

    private static ParameterType MapType(ParameterModel param, List<DiagnosticModel> diagnostics)
    {
        switch (param.Type)
        {
            case { SpecialType: SpecialType.System_String }:
                return new ParameterType.AsIs(param.Type.GetFullyQualifiedName());
            case { SpecialType: SpecialType.System_Boolean }:
                return new ParameterType.Bool();
            case { TypeKind: TypeKind.Enum }:
                var enumValues = MapEnumValues(param, diagnostics);

                return new ParameterType.Enum(param.Type.GetFullyQualifiedName(), enumValues);
            case INamedTypeSymbol t when t.HasParseMethod():
                return new ParameterType.Parse(t.GetFullyQualifiedName(), "Parse");
            case INamedTypeSymbol t when t.HasCastFromString():
                return new ParameterType.ExplicitCast(t.GetFullyQualifiedName());
            case INamedTypeSymbol t when t.HasConstructorWithSingleStringParameter():
                return new ParameterType.Constructor(t.GetFullyQualifiedName());
            default:
                diagnostics.Add(DiagnosticModel.UnsupportedParameterType(param));
                return new ParameterType.AsIs(param.Type.GetFullyQualifiedName());
        };
    }

    private static ImmutableArray<EnumValue> MapEnumValues(ParameterModel param, List<DiagnosticModel> diagnostics)
    {
        return param.Type
            .GetMembers()
            .Where(member => member.Kind == SymbolKind.Field)
            .Select(GetEnumValue)
            .ToImmutableArray();

        EnumValue GetEnumValue(ISymbol member)
        {
            var name = member.Name;
            var xmlComments = XmlCommentModelBuilder.GetXmlComments(member);

            diagnostics.AddRange(xmlComments.Diagnostics);

            return new(name, xmlComments.Result.FlatMap(c => c.Summary));
        }
    }

    private readonly struct ListWalker<T>
    {
        public ListWalker(IReadOnlyList<T> list)
            : this(list, index: 0)
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
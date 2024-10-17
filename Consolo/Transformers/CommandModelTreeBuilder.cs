using Microsoft.CodeAnalysis;
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

        foreach (var methodModel in methodModels)
        {
            var currentLevel = (CommandTree)root;
            var currentPath = GetPath(methodModel);

            foreach (var (pathEntry, i) in currentPath.Select((a, b) => (a, b)))
            {
                if (i == 0 && pathEntry.Name == "")
                {
                    if (root.Description.IsNone)
                    {
                        root.Description = pathEntry.XmlComment;
                    }
                    continue;
                }

                if (pathEntry.Name == "")
                {
                    if (currentLevel.Method.HasValue)
                    {
                        if (currentLevel is CommandTree.SubCommand s)
                        {
                            diagnostics.Add(DiagnosticModel.DuplicateCommand(s.CommandName, methodModel.Location));
                        }
                        else
                        {
                            diagnostics.Add(DiagnosticModel.DuplicateRootCommand(methodModel.Location));
                        }

                        continue;
                    }
                    currentLevel.Method = MapMethod(methodModel, diagnostics);

                    continue;
                }

                switch (currentLevel.SubCommands.FirstOrDefault(it => it.CommandName == pathEntry.Name))
                {
                    case null when i == currentPath.Count - 1:
                        {
                            var newLevel = new CommandTree.SubCommand(StringUtils.ToKebabCase(pathEntry.Name))
                            {
                                Method = MapMethod(methodModel, diagnostics),
                                Description = pathEntry.XmlComment,
                            };

                            currentLevel.SubCommands.Add(newLevel);
                            currentLevel = newLevel;
                            break;
                        }

                    case null:
                        {
                            var newLevel = new CommandTree.SubCommand(StringUtils.ToKebabCase(pathEntry.Name))
                            {
                                Description = pathEntry.XmlComment,
                            };

                            currentLevel.SubCommands.Add(newLevel);
                            currentLevel = newLevel;
                            break;
                        }
                    case { } sub when i == currentPath.Count - 1:
                        {
                            if (currentLevel is CommandTree.SubCommand s)
                            {
                                diagnostics.Add(DiagnosticModel.DuplicateCommand(sub.CommandName, s.CommandName, methodModel.Location));
                            }
                            else
                            {
                                diagnostics.Add(DiagnosticModel.DuplicateCommand(sub.CommandName, methodModel.Location));
                            }
                            continue;
                        }

                    case { } sub:
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

            pathItems.Add(newItem);

            return pathItems;
        }

        return new ResultWithDiagnostics<CommandTree.Root>(root, diagnostics);
    }

    private static CommandMethod MapMethod(MethodModel methodModel, List<DiagnosticModel> diagnostics) =>
        new CommandMethod(
            FullyQualifiedClassName: methodModel.FullyQualifiedClassName,
            MethodName: methodModel.MethodName,
            Parameters: MapParameters(methodModel, diagnostics),
            Description: methodModel.Description);

    private static IReadOnlyCollection<CommandParameter> MapParameters(
        MethodModel methodModel,
        List<DiagnosticModel> diagnostics)
    {
        var claimedParameterNames = new HashSet<string>();

        return methodModel.Parameters
            .Select(arg => MapParameter(arg, methodModel, claimedParameterNames, diagnostics))
            .WhereIsSome()
            .ToList();
    }

    private static Option<CommandParameter> MapParameter(
        ParameterModel param,
        MethodModel methodModel,
        HashSet<string> claimedParameterNames,
        List<DiagnosticModel> diagnostics)
    {
        if (claimedParameterNames.Contains(param.Name))
        {
            diagnostics.Add(DiagnosticModel.DuplicateNameOrAlias(
                param.Name, methodModel.MethodName, param.NameLocation | param.Location));

            return None;
        }

        if (param.Alias.IsSome(out var a) && claimedParameterNames.Contains(a))
        {
            diagnostics.Add(DiagnosticModel.DuplicateNameOrAlias(
                a, methodModel.MethodName, param.AliasLocation | param.Location));

            return None;
        }

        claimedParameterNames.AddRange(param.GetAllNames());

        if (param.IsRequired)
        {
            return new CommandParameter.Positional(
                name: param.Name,
                sourceName: param.SourceName,
                type: MapType(param, diagnostics),
                description: param.Description | param.Type.Name);
        }
        else
        {
            var name = Some(param.Name);
            var alias = param.Alias;
            var description = param.Description;

            if (param.Name.Length == 1 && alias.IsNone)
            {
                name = None;
                alias = Some(param.Name);

                if (param.Description.IsNone)
                {
                    description = param.SourceName;
                }
            }

            return new CommandParameter.Option(
                name: name,
                alias: alias,
                type: MapType(param, diagnostics),
                defaultValue: param.DefaultValue | "default",
                description: description | param.Type.Name,
                sourceName: param.SourceName,
                isFlag: param.IsFlag
            );
        }
    }

    private static ParameterType MapType(ParameterModel param, List<DiagnosticModel> diagnostics)
    {
        switch (param.Type)
        {
            case { SpecialType: SpecialType.System_String }:
                return new ParameterType.AsIs(param.Type.FullyQualifiedName);
            case { SpecialType: SpecialType.System_Boolean }:
                return new ParameterType.Bool();
            case { TypeKind: TypeKind.Enum }:
                var enumValues = MapEnumValues(param);

                return new ParameterType.Enum(param.Type.FullyQualifiedName, enumValues);
            case { HasParseMethod: true } t:
                return new ParameterType.Parse(t.FullyQualifiedName, "Parse");
            case { HasCastFromString: true } t:
                return new ParameterType.ExplicitCast(t.FullyQualifiedName);
            case { HasConstructorWithSingleStringParameter: true } t:
                return new ParameterType.Constructor(t.FullyQualifiedName);
            default:
                diagnostics.Add(DiagnosticModel.UnsupportedParameterType(param));
                return new ParameterType.AsIs(param.Type.FullyQualifiedName);
        };
    }

    private static ImmutableArray<EnumValue> MapEnumValues(ParameterModel param) =>
        param.Type.AllowedValues
            .Select(value => new EnumValue(value.Value, value.Description))
            .ToImmutableArray();
}
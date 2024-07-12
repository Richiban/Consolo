using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Richiban.Cmdr.Models;
using Richiban.Cmdr.Utils;
using System.Xml.Linq;

namespace Richiban.Cmdr;

internal class MethodModelBuilder
{
    public IEnumerable<Result<MethodModelFailure, MethodModel>> BuildFrom(
        IEnumerable<IMethodSymbol?> qualifyingMethods) =>
        qualifyingMethods.SelectNotNull(TryMapMethod);

    private Result<MethodModelFailure, MethodModel> TryMapMethod(
        IMethodSymbol? methodSymbol)
    {
        if (methodSymbol is null)
        {
            return new MethodModelFailure("Method not found", location: null);
        }

        if (!methodSymbol.IsStatic)
        {
            return new MethodModelFailure(
                $"Method {methodSymbol} must be static in order to use the {CmdrAttributeDefinition.ShortName} attribute.",
                methodSymbol.Locations.FirstOrDefault());
        }

        var xmlComments = GetXmlComments(methodSymbol);

        var parameters = methodSymbol.Parameters
            .Select(GetArgumentModel)
            .Select(model => model with
            {
                Description = xmlComments?.Params.TryGetValue(model.Name, out var v) == true ? v : model.Description
            })
            .ToImmutableArray();

        var fullyQualifiedName = methodSymbol.ContainingType.GetFullyQualifiedName();

        var commandPath = GetCommandPath(methodSymbol);
        var parentNames = commandPath.Truncate(count: -1).ToList();

        var lastPathItem = commandPath.LastOrDefault();

        return new MethodModel(
            methodSymbol.Name,
            lastPathItem.Name,
            parentNames,
            fullyQualifiedName,
            parameters,
            xmlComments?.Summary ?? lastPathItem.Description);
    }

    private static XmlCommentModel? GetXmlComments(ISymbol symbol)
    {
        var xmlString = symbol.GetDocumentationCommentXml();

        if (xmlString is null or "")
        {
            return null;
        }

        var doc = XDocument.Parse(xmlString);
        var memberElement = doc.Element("member");
        var summary = memberElement.Element("summary").Value;

        var paramComments = memberElement.Elements("param")
            .ToDictionary(
                el => el.Attribute("name")?.Value ?? "",
                el => SourceValueUtils.EscapeCSharpString(el.Value));

        summary = SourceValueUtils.EscapeCSharpString(summary?.Trim());

        return new XmlCommentModel
        {
            Summary = summary,
            Params = paramComments
        };
    }

    private ImmutableList<CommandPathItem> GetCommandPath(ISymbol symbol)
    {
        var path = ImmutableList.CreateBuilder<CommandPathItem>();

        while (symbol != null)
        {
            if (AttributeUsageUtils.GetUsage(symbol) is { } attr)
            {
                path.Add(new(attr?.Names.LastOrDefault() ?? symbol.Name, attr?.Description));
            }

            symbol = symbol.ContainingType;
        }

        path.Reverse();

        return path.ToImmutable();
    }

    private ArgumentModel GetArgumentModel(IParameterSymbol parameterSymbol)
    {
        var attr = AttributeUsageUtils.GetUsage(parameterSymbol);

        var name = attr?.Names?.LastOrDefault() is { } lastName ? lastName : parameterSymbol.Name;

        var type = parameterSymbol.Type.GetFullyQualifiedName();
        var isFlag = type == "System.Boolean";
        var description = attr?.Description;
        var isRequired = !parameterSymbol.HasExplicitDefaultValue;
        var defaultValue =
            parameterSymbol.HasExplicitDefaultValue
            ? SourceValueUtils.SourceValue(parameterSymbol.ExplicitDefaultValue)
            : null;

        return new ArgumentModel(
            name,
            type,
            isFlag,
            IsRequired: isRequired,
            DefaultValue: defaultValue,
            Description: description);
    }
}

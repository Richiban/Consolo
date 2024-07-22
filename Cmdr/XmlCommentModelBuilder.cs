using System.Linq;
using Microsoft.CodeAnalysis;
using System.Xml.Linq;
using static Cmdr.Prelude;
using System.Collections.Generic;

namespace Cmdr;

internal class XmlCommentModelBuilder
{
    public static ResultWithDiagnostics<Option<XmlCommentModel>> GetXmlComments(ISymbol symbol)
    {
        var xmlString = symbol.GetDocumentationCommentXml();

        if (xmlString is null or "")
        {
            return new(None, []);
        }

        var doc = XDocument.Parse(xmlString);
        var memberElement = doc.Element("member");
        var summary = memberElement.Element("summary").Value;

        var paramComments = new Dictionary<string, string>();
        
        foreach (var paramComment in memberElement.Elements("param"))
        {
            var paramName = paramComment.Attribute("name")?.Value ?? "";
            
            if (!paramComments.ContainsKey(paramName))
            {
                paramComments.Add(paramName, SourceValueUtils.EscapeCSharpString(paramComment.Value));
            }
        }

        summary = SourceValueUtils.EscapeCSharpString(summary.Trim());

        return Some(new XmlCommentModel
        {
            Summary = summary,
            Params = paramComments
        });
    }
}

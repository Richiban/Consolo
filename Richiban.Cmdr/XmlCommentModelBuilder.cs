using System.Linq;
using Microsoft.CodeAnalysis;
using System.Xml.Linq;
using static Richiban.Cmdr.Prelude;

namespace Richiban.Cmdr;

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

        var paramComments = memberElement.Elements("param")
            .ToDictionary(
                el => el.Attribute("name")?.Value ?? "",
                el => SourceValueUtils.EscapeCSharpString(el.Value));

        summary = SourceValueUtils.EscapeCSharpString(summary.Trim());

        return Some(new XmlCommentModel
        {
            Summary = summary,
            Params = paramComments
        });
    }
}

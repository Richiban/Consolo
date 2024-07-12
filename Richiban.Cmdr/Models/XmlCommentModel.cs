using System.Collections.Generic;

namespace Richiban.Cmdr;

public record XmlCommentModel
{
    public string? Summary { get; init; }
    public IReadOnlyDictionary<string, string> Params { get; init; } = new Dictionary<string, string>();
}
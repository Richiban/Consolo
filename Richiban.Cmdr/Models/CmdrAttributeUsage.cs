using System.Collections.Generic;

namespace Richiban.Cmdr;

public record CmdrAttributeUsage(IReadOnlyCollection<string> Names, string? ShortForm);
using System.Collections.Generic;

namespace Cmdr;

public record CmdrAttributeUsage(IReadOnlyCollection<string> Names, string? ShortForm);
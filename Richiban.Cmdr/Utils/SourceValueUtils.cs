namespace Richiban.Cmdr;

public static class SourceValueUtils
{
    public static string SourceValue(object? value) =>
        value switch
        {
            null => "null",
            string s => $"\"{s}\"",
            _ => value.ToString() ?? "null"
        };
}
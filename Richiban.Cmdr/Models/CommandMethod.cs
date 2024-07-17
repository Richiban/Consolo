namespace Richiban.Cmdr;

internal record CommandMethod(
        string FullyQualifiedClassName,
        string MethodName)
{
    public string FullyQualifiedName { get; } = $"{FullyQualifiedClassName}.{MethodName}";
}
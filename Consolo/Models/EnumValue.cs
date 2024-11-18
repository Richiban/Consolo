namespace Consolo;

record EnumValue(string SourceName, Option<string> Description)
{
    public string HelpName => StringUtils.ToCamelCase(SourceName);
}
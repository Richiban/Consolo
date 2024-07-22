namespace Cmdr;

static class OptionExtensions
{
    public static Option<string> Trim(this Option<string> opt) => 
        opt.IsSome(out var v) ? v.Trim() : null;
}
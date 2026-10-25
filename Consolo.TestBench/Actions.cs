using Consolo;

namespace Consolo.TestBench;

public static class Commands
{
    public static string LastMessage { get; private set; } = string.Empty;

    [Consolo("echo")]
    public static void Echo(string message) => LastMessage = message;

    [Consolo("options")]
    public static void Options(string message, [Consolo(Alias = "n")] int number = 0)
    {
    }

    [Consolo("flags")]
    public static void Flags(
        string message,
        [Consolo("force", Alias = "f")] bool force = false,
        bool dryRun = false)
    {
    }

    [Consolo("multiple")]
    public static void Multiple(string first, string second, int third)
    {
    }

    [Consolo("many")]
    public static void ManyOptions(
        bool option00 = false, bool option01 = false, bool option02 = false,
        bool option03 = false, bool option04 = false, bool option05 = false,
        bool option06 = false, bool option07 = false, bool option08 = false,
        bool option09 = false, bool option10 = false, bool option11 = false,
        bool option12 = false, bool option13 = false, bool option14 = false,
        bool option15 = false, bool option16 = false, bool option17 = false,
        bool option18 = false, bool option19 = false, bool option20 = false,
        bool option21 = false, bool option22 = false, bool option23 = false,
        bool option24 = false, bool option25 = false, bool option26 = false,
        bool option27 = false, bool option28 = false, bool option29 = false,
        bool option30 = false, bool option31 = false, bool option32 = false,
        bool option33 = false, bool option34 = false, bool option35 = false,
        bool option36 = false, bool option37 = false, bool option38 = false,
        bool option39 = false, bool option40 = false, bool option41 = false,
        bool option42 = false, bool option43 = false, bool option44 = false,
        bool option45 = false, bool option46 = false, bool option47 = false,
        bool option48 = false, bool option49 = false, bool option50 = false,
        bool option51 = false, bool option52 = false, bool option53 = false,
        bool option54 = false, bool option55 = false, bool option56 = false,
        bool option57 = false, bool option58 = false, bool option59 = false,
        bool option60 = false, bool option61 = false, bool option62 = false,
        bool option63 = false, bool option64 = false)
    {
    }
}

using System;

namespace Richiban.Cmdr
{
    public class CmdrMethod : System.Attribute
    {
        public CmdrMethod(params string[] aliases)
        {
            Aliases = aliases;
        }

        public string[] Aliases { get; }
    }
}
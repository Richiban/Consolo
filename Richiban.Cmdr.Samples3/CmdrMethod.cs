using System;

namespace Richiban.Cmdr
{
    public class CmdrMethod : System.Attribute
    {
        public CmdrMethod(string name)
        {
            Name = name;
        }
        
        public string Name { get; }
    }
}
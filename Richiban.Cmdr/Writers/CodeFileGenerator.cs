using System;

namespace Richiban.Cmdr.Writers
{
    internal abstract class CodeFileGenerator
    {
        public abstract string FileName { get; }
        public abstract string GetCode();
    }
}
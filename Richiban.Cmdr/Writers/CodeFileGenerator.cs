using System;

namespace Richiban.Cmdr;

abstract class CodeFileGenerator
{
    public abstract string FileName { get; }
    public abstract string GetCode();
}
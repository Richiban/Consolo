using System;

namespace Consolo;

abstract class CodeFileGenerator
{
    public abstract string FileName { get; }
    public abstract string GetCode();
}
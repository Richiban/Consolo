using System;
using System.Collections.Generic;

namespace Richiban.Cmdr.Generators
{
    internal interface ICodeGenerator
    {
        IEnumerable<string> GetCodeLines();
    }
}
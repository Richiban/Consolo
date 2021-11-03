using System;

namespace Richiban.Cmdr.Generator
{
    internal record MethodModel
    {
        public MethodModel(string methodName)
        {
            MethodName = methodName;
        }

        public string MethodName { get; }
    }
}
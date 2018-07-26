using System;

namespace Richiban.CommandLine
{
    public interface IObjectFactory
    {
        object CreateInstance(Type typeToInstantiate);
    }
}
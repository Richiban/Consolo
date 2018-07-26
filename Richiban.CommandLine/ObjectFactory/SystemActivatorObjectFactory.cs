using System;
using System.Linq;

namespace Richiban.CommandLine
{
    internal class SystemActivatorObjectFactory : IObjectFactory
    {
        public object CreateInstance(Type typeToInstantiate)
        {
            var constructor = typeToInstantiate
                .GetConstructors()
                .Where(c => c.GetParameters().Length == 0)
                .SingleOrDefault();

            if(constructor == null)
            {
                throw new InvalidOperationException(
                    $"Cannot instantiate type {typeToInstantiate} because it does not"
                    + " have a parameterless constructor");
            }

            return constructor.Invoke(new object[0]);
        }
    }
}
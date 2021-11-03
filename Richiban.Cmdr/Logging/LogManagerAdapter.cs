using Richiban.Cmdr;
using System;

namespace Tracer
{
    static class LogManagerAdapter
    {
        public static LoggerAdapterBase GetLogger(Type type)
        {
            if (type == typeof(TypeConverterCollection))
            {
                return new TypeConverterCollectionLoggerAdapter();
            }
            else if (type == typeof(CommandLine))
            {
                return new CommandLineLoggerAdapter();
            }
            else if (type == typeof(MethodMapper))
            {
                return new MethodMapperLoggerAdapter();
            }
            else if (type == typeof(AssemblyModel))
            {
                return new AssemblyModelLoggerAdapter();
            }
            else if (type == typeof(CommandLineActionFactory))
            {
                return new CommandLineActionFactoryLoggerAdapter();
            }
            else if (type == typeof(CommandLineAction))
            {
                return new CommandLineActionLoggerAdapter();
            }
            else
            {
                return new DefaultLoggerAdapter(type);
            }
        }
    }
}

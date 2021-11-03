using System;
using Richiban.Cmdr;

namespace Tracer
{
    internal static class LogManagerAdapter
    {
        public static LoggerAdapterBase GetLogger(Type type)
        {
            if (type == typeof(TypeConverterCollection))
            {
                return new TypeConverterCollectionLoggerAdapter();
            }

            if (type == typeof(CommandLine))
            {
                return new CommandLineLoggerAdapter();
            }

            if (type == typeof(MethodMapper))
            {
                return new MethodMapperLoggerAdapter();
            }

            if (type == typeof(AssemblyModel))
            {
                return new AssemblyModelLoggerAdapter();
            }

            if (type == typeof(CommandLineActionFactory))
            {
                return new CommandLineActionFactoryLoggerAdapter();
            }

            if (type == typeof(CommandLineAction))
            {
                return new CommandLineActionLoggerAdapter();
            }

            return new DefaultLoggerAdapter(type);
        }
    }
}
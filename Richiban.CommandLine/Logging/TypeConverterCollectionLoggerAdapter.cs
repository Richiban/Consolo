using NullGuard;
using Richiban.CommandLine;
using System.Linq;

namespace Tracer
{
    class TypeConverterCollectionLoggerAdapter : LoggerAdapterBase
    {
        public override void TraceEnter(string methodInfo, [AllowNull]string[] paramNames, [AllowNull]object[] paramValues)
        {
            if (methodInfo.Contains(".ctor"))
            {
                CommandLine.Trace($"[Trace]: Registered {nameof(ITypeConverter)} instances:");

                if (paramNames != null && paramValues != null && paramValues.Length >= 1)
                {
                    TraceArgument(paramValues[0]);
                }
            }
        }

        public override void TraceLeave(string methodInfo, long startTicks, long endTicks, [AllowNull]string[] paramNames, [AllowNull]object[] paramValues)
        {
        }
    }
}
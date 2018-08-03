using NullGuard;
using Richiban.CommandLine;

namespace Tracer
{
    class CommandLineLoggerAdapter : LoggerAdapterBase
    {
        public override void TraceEnter(string methodInfo, [AllowNull] string[] paramNames, [AllowNull] object[] paramValues)
        {
            CommandLine.Trace($"[Trace]: In {GetType()}");
        }

        public override void TraceLeave(string methodInfo, long startTicks, long endTicks, [AllowNull] string[] paramNames, [AllowNull] object[] paramValues)
        {
            CommandLine.Trace($"[Trace]: Out {GetType()}");
        }
    }
}

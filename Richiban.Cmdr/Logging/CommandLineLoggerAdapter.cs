using NullGuard;
using Richiban.Cmdr;
using System;

namespace Tracer
{
    class CommandLineLoggerAdapter : LoggerAdapterBase
    {
        public override void TraceEnter(string methodInfo, [AllowNull] string[] paramNames, [AllowNull] object[] paramValues)
        {
            CommandLine.Trace($"[Trace]: Beginning CommandLine sequence {GetType()}");
        }

        public override void TraceLeave(string methodInfo, long startTicks, long endTicks, [AllowNull] string[] paramNames, [AllowNull] object[] paramValues)
        {
            CommandLine.Trace($"[Trace]: CommandLine sequence finished in {TimeSpan.FromTicks(endTicks - startTicks)}");
        }
    }
}

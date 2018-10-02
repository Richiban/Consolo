using NullGuard;
using Richiban.CommandLine;
using System;

namespace Tracer
{
    internal class CommandLineActionLoggerAdapter : LoggerAdapterBase
    {
        public override void TraceEnter(
            string methodInfo, [AllowNull]string[] paramNames, [AllowNull]object[] paramValues)
        {
            CommandLine.Trace($"[Trace]: Executing action -->");
            CommandLine.Trace("");
        }

        public override void TraceLeave(string methodInfo, long startTicks, long endTicks, 
            [AllowNull]string[] paramNames, [AllowNull]object[] paramValues)
        {
            CommandLine.Trace("");
            CommandLine.Trace($"[Trace]: <-- Action completed in {TimeSpan.FromTicks(endTicks - startTicks)}");
        }
    }
}

using Richiban.CommandLine;
using System;

namespace Tracer
{
    internal class CommandLineActionLoggerAdapter : LoggerAdapterBase
    {
        public override void TraceEnter(
            string methodInfo, string[] paramNames, object[] paramValues)
        {
            CommandLine.Trace($"[Trace]: Executing action -->");
            CommandLine.Trace("");
        }

        public override void TraceLeave(string methodInfo, long startTicks, long endTicks, 
            string[] paramNames, object[] paramValues)
        {
            CommandLine.Trace("");
            CommandLine.Trace($"[Trace]: <-- Action completed in {TimeSpan.FromTicks(endTicks - startTicks)}");
        }
    }
}
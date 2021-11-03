using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Richiban.Cmdr;

namespace Tracer
{
    internal class AssemblyModelLoggerAdapter : LoggerAdapterBase
    {
        public override void TraceEnter(
            string methodInfo,
            [AllowNull] string[] paramNames,
            [AllowNull] object[] paramValues)
        {
            if (paramValues?.Length >= 1 && paramValues[0] is IEnumerable e)
            {
                var sb = new StringBuilder();

                foreach (var item in e)
                {
                    sb.Append($"{Environment.NewLine}        {item}");
                }

                CommandLine.Trace($"[Trace]: Scanning assemblies: {sb}");
            }
        }

        public override void TraceLeave(
            string methodInfo,
            long startTicks,
            long endTicks,
            [AllowNull] string[] paramNames,
            object[] paramValues)
        {
        }
    }
}
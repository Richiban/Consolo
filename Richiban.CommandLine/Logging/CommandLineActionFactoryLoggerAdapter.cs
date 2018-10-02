using Richiban.CommandLine;
using System.Collections;
using System.Collections.Generic;

namespace Tracer
{
    internal class CommandLineActionFactoryLoggerAdapter : LoggerAdapterBase
    {
        public override void TraceEnter(
            string methodInfo, string[] paramNames, object[] paramValues)
        {
        }

        public override void TraceLeave(
            string methodInfo, long startTicks,
            long endTicks,
            string[] paramNames,
            object[] paramValues)
        {
            if (methodInfo.Contains("GetBestMatches"))
            {
                if (paramValues != null && paramValues.Length >= 1 && paramValues[0] is IEnumerable e)
                {
                    CommandLine.Trace($"[Trace]: Selecting the best matches:");

                    foreach (var item in e)
                    {
                        CommandLine.Trace($"{item}", indentationLevel: 2);
                    }
                }
            } else if (methodInfo.Contains("GetMethodMappings"))
            {
                if (paramValues != null && paramValues.Length >= 1 && paramValues[0] is IEnumerable<MethodMapping> e)
                {
                    CommandLine.Trace($"[Trace]: Generated the following Mappings:");

                    foreach (var item in e)
                    {
                        CommandLine.Trace($"{item}", indentationLevel: 2);
                    }
                }
            }
        }
    }
}
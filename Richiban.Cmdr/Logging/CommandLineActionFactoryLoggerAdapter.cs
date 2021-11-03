using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Richiban.Cmdr;

namespace Tracer
{
    internal class CommandLineActionFactoryLoggerAdapter : LoggerAdapterBase
    {
        public override void TraceEnter(
            string methodInfo,
            [AllowNull] string[] paramNames,
            [AllowNull] object[] paramValues)
        {
        }

        public override void TraceLeave(
            string methodInfo,
            long startTicks,
            long endTicks,
            [AllowNull] string[] paramNames,
            [AllowNull] object[] paramValues)
        {
            if (methodInfo.Contains("GetBestMatches"))
            {
                if (paramValues != null && paramValues.Length >= 1 &&
                    paramValues[0] is IEnumerable e)
                {
                    CommandLine.Trace("[Trace]: Selecting the best matches:");

                    foreach (var item in e)
                    {
                        CommandLine.Trace($"{item}", indentationLevel: 2);
                    }
                }
            }
            else if (methodInfo.Contains("GetMethodMappings"))
            {
                if (paramValues != null && paramValues.Length >= 1 &&
                    paramValues[0] is IEnumerable<MethodMapping> e)
                {
                    CommandLine.Trace("[Trace]: Generated the following Mappings:");

                    foreach (var item in e)
                    {
                        CommandLine.Trace($"{item}", indentationLevel: 2);
                    }
                }
            }
        }
    }
}
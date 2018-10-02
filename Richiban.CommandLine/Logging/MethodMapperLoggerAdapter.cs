
using Richiban.CommandLine;

namespace Tracer
{
    class MethodMapperLoggerAdapter : LoggerAdapterBase
    {
        public override void TraceEnter(string methodInfo, string[] paramNames, object[] paramValues)
        {
            if (paramValues?.Length >= 2)
            {
                CommandLine.Trace($"[Trace]: Attempting to map method {paramValues[0]} from arguments \"{paramValues[1]}\"");
            }
        }

        public override void TraceLeave(string methodInfo, long startTicks, long endTicks, string[] paramNames, object[] paramValues)
        {
            if (paramValues?.Length >= 1 && paramValues[0] is Option<MethodMapping> opt)
            {
                if (opt.HasValue)
                {
                    CommandLine.Trace($"         Success, method mapped");
                }
                else
                {
                    CommandLine.Trace($"         No match");
                }
            }
        }
    }
}
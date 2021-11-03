using System;
using System.Collections;
using System.Linq;
using Richiban.Cmdr;

namespace Tracer
{
    internal abstract class LoggerAdapterBase
    {
        public abstract void TraceEnter(
            string methodInfo,
            string[] paramNames,
            object[] paramValues);

        public abstract void TraceLeave(
            string methodInfo,
            long startTicks,
            long endTicks,
            string[] paramNames,
            object[] paramValues);

        protected static string GetMethodName(string methodInfo)
        {
            if (methodInfo == null)
            {
                return null;
            }

            return methodInfo.Substring(startIndex: 0, methodInfo.IndexOf(value: '('));
        }

        protected static void TraceArguments(string[] paramNames, object[] paramValues)
        {
            if (paramNames != null && paramValues != null)
            {
                foreach (var i in Enumerable.Range(
                    start: 0,
                    Math.Min(paramNames.Length, paramValues.Length)))
                {
                    TraceArgument(paramNames[i], paramValues[i]);
                }
            }
        }

        protected static void TraceArgument(string paramName, object paramValue)
        {
            if (paramValue == null)
            {
                return;
            }

            if (!(paramValue is string) && paramValue is IEnumerable e)
            {
                CommandLine.Trace($"{paramName} => ", indentationLevel: 1);

                foreach (var item in e)
                {
                    CommandLine.Trace($"{item}", indentationLevel: 2);
                }

                return;
            }

            CommandLine.Trace($"{paramName} => {paramValue}", indentationLevel: 1);
        }

        protected static void TraceArgument(object paramValue)
        {
            if (paramValue == null)
            {
                return;
            }

            if (!(paramValue is string) && paramValue is IEnumerable e)
            {
                foreach (var item in e)
                {
                    CommandLine.Trace($"{item}", indentationLevel: 2);
                }

                return;
            }

            CommandLine.Trace(paramValue, indentationLevel: 1);
        }
    }
}
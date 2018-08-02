using System;
using System.Collections.Generic;
using System.Text;

namespace Tracer
{
    public static class LogManagerAdapter
    {
        public static LoggerAdapter GetLogger(Type type) => new LoggerAdapter(type);
    }
}

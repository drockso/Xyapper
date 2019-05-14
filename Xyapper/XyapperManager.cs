using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xyapper
{
    public static class XyapperManager
    {
        public static ILogger Logger { get; set; }

        public static bool EnableLogging { get; set; }

        public static LogLevel CommandLogLevel { get; set; } = LogLevel.Trace;

        public static LogLevel ExceptionLogLevel { get; set; } = LogLevel.Error;
    }
}

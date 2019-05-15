using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xyapper
{
    /// <summary>
    /// Xyapper settings singleton
    /// </summary>
    public static class XyapperManager
    {
        /// <summary>
        /// Logger instance
        /// </summary>
        public static ILogger Logger { get; set; }

        /// <summary>
        /// Enable logging for all commands exected via Xyapper
        /// </summary>
        public static bool EnableLogging { get; set; }

        /// <summary>
        /// Log level for commands
        /// </summary>
        public static LogLevel CommandLogLevel { get; set; } = LogLevel.Trace;

        /// <summary>
        /// Log level for errors
        /// </summary>
        public static LogLevel ExceptionLogLevel { get; set; } = LogLevel.Error;
    }
}

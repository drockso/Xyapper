using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        /// Enable logging for all commands executed via Xyapper
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

        /// <summary>
        /// Format provider for conversions
        /// </summary>
        public static IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;

        /// <summary>
        /// Use more flexible type conversions. If enabled, Xyapper works a little bit slower
        /// </summary>
        public static bool UseAdvancedTypeConversions { get; set; } = true;

        /// <summary>
        /// Trim all strings from DB. UseAdvancedTypeConversions must be set to true
        /// </summary>
        public static bool TrimStrings { get; set; } = true;

        /// <summary>
        /// Command timeout for all queries
        /// </summary>
        public static int CommandTimeout { get; set; } = 1000000;
    }
}

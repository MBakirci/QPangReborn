using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;

namespace Reborn.Utils.Config
{
    public static class LogConfig
    {
        public static LoggingConfiguration Create()
        {
            var config = new LoggingConfiguration();

            config.AddTarget(new ColoredConsoleTarget
            {
                Name = "ConsoleOutput",
                Layout = Layout.FromString("${shortdate} ${pad:padding=5:inner=${level:uppercase=true}} ${message} ${exception:format=tostring}"),
                ErrorStream = false,
                UseDefaultRowHighlightingRules = true
            });

            config.AddRule(LogLevel.Trace, LogLevel.Off, "ConsoleOutput");

            return config;
        }
    }
}

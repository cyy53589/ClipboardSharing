using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace lib
{
    public class LogHelper
    {
        public static void InitLogger()
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile")
            {
                FileName = "log.txt"
            };
            var logconsole = new NLog.Targets.ConsoleTarget("ColoredConsole")
            {
                Layout = "[${date}][${level}]\t[${logger}][${message}] ${exception} ${event-properties:myProperty}"
            };

            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            // Apply config           
            NLog.LogManager.Configuration = config;
        }

    }
}

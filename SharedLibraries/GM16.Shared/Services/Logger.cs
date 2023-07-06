using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILogger = GM16.Shared.Interfaces.ILogger;

namespace GM16.Shared.Services
{
    public class Logger : ILogger
    {
        #region Fields

        private static Serilog.Core.Logger? _logger;

        #endregion

        #region Function

        public Logger()
        {
            InitializeLogger();
        }


        public void InitializeLogger()
        {
            string LogFilePath(string LogEvent)
            {
                return $@"{AppContext.BaseDirectory}Log\{DateTime.Now.ToString("yyyy_MM_dd")}\log{LogEvent}.log";
            }

            var SerilogOutputTemplate =
                "Date：{Timestamp:yyyy-MM-dd HH:mm:ss.fff}{NewLine}LogLevel：{Level}{NewLine}Message：{Message}{NewLine}{Exception}" +
                new string('-', 50) + "{NewLine}{NewLine}";
            var SerilogOutputTemplate2 =
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}";


            if (_logger == null)
                _logger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .MinimumLevel.Debug() // 所有Sink的最小记录级别
                    .WriteTo.Logger(lg =>
                        lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Debug).WriteTo.File(LogFilePath("Debug"),
                            rollingInterval: RollingInterval.Day, outputTemplate: SerilogOutputTemplate,
                            retainedFileCountLimit: 30))
                    .WriteTo.Logger(lg =>
                        lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Information).WriteTo.File(
                            LogFilePath("Information"), rollingInterval: RollingInterval.Day,
                            outputTemplate: SerilogOutputTemplate, retainedFileCountLimit: 30))
                    .WriteTo.Logger(lg =>
                        lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Warning).WriteTo.File(
                            LogFilePath("Warning"), rollingInterval: RollingInterval.Day,
                            outputTemplate: SerilogOutputTemplate, retainedFileCountLimit: 30))
                    .WriteTo.Logger(lg =>
                        lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Error).WriteTo.File(LogFilePath("Error"),
                            rollingInterval: RollingInterval.Day, outputTemplate: SerilogOutputTemplate,
                            retainedFileCountLimit: 30))
                    .WriteTo.Logger(lg =>
                        lg.Filter.ByIncludingOnly(p => p.Level == LogEventLevel.Fatal).WriteTo.File(LogFilePath("Fatal"),
                            rollingInterval: RollingInterval.Day, outputTemplate: SerilogOutputTemplate,
                            retainedFileCountLimit: 30))
                    .CreateLogger();
        }

        #endregion

        #region Properties

        public static Logger Instance
        {
            get
            {
                if (_instance == null) _instance = new Logger();

                return _instance;
            }
        }

        private static Logger? _instance;

        #endregion

        #region Methods

        public void Error(Exception exception, string messageTemplate)
        {
            
            _logger?.Error(exception, messageTemplate);
           
        }

        public void Error(Exception exception, string messageTemplate, params object[] args)
        {
            
            _logger?.Error(exception, messageTemplate, args);
           
        }

        public void Error(string messageTemplate, params object[] args)
        {
            
            _logger?.Error(messageTemplate, args);
           
        }

        public void Information(string message)
        {
            
            _logger?.Information(message);
           
        }

        public void Information(string messageTemplate, params object[] args)
        {
            
            _logger?.Information(messageTemplate, args);
           
        }

        public void Debug(string message)
        {
            
            _logger?.Debug(message);
           
        }

        public void Debug(string messageTemplate, params object[] args)
        {
            
            _logger?.Debug(messageTemplate, args);
           
        }

        public void Warning(string message)
        {
            
            _logger?.Warning(message);
           
        }

        public void Warning(string messageTemplate, params object[] args)
        {
            _logger?.Warning(messageTemplate, args);
        }

        #endregion
    }
}

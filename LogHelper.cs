using CommonLib;
using log4net;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System;
using System.Text;

public class LogHelper
{
    private const string LOG_PATTERN = "[%level|%date|%appdomain->%message]%newline";

    public static bool LogOn
    {
        get
        {
            return ConfigHelper.GetConfigBool("log", false);
        }
        set
        {
            ConfigHelper.UpdateConfig("log", value, true);
        }
    }
    public static Level LogLevel
    {
        get
        {
            return (Level)ConfigHelper.GetConfigInt("logLevel", 3);
        }
        set
        {
            ConfigHelper.UpdateConfig("logLevel", (int)value, true);
        }
    }

    public static Level GetLevelByString(string levStr)
    {
        Level level = Level.Info;
        if (!string.IsNullOrWhiteSpace(levStr))
        {
            switch (levStr.ToLower())
            {
                case "debug":
                    level = Level.Debug;
                    break;
                case "error":
                    level = Level.Error;
                    break;
                case "fatal":
                    level = Level.Fatal;
                    break;
                case "info":
                    level = Level.Info;
                    break;
                case "warn":
                    level = Level.Warn;
                    break;
            }
        }
        return level;
    }

    /// <summary>
    /// 写日志锁
    /// </summary>
    private static readonly object logLock = new object();

    public static ILog logger = null;

    public static bool WriteLog(object message, Exception ex = null, Level level = Level.Info)
    {
        if (!LogOn) return false;

        log4net.Core.Level lev = log4net.Core.Level.All;
        switch (level)
        {
            case Level.Debug:
                lev = log4net.Core.Level.Debug;
                break;
            case Level.Error:
                lev = log4net.Core.Level.Error;
                break;
            case Level.Fatal:
                lev = log4net.Core.Level.Fatal;
                break;
            case Level.Info:
                lev = log4net.Core.Level.Info;
                break;
            case Level.Warn:
                lev = log4net.Core.Level.Warn;
                break;
        }

        lock (logLock)
        {
            try
            {
                if (message == null || string.IsNullOrWhiteSpace(message.ToString())) return false;  //不写空日志
                if (logger == null)
                {
                    PatternLayout patternLayout = new PatternLayout();
                    patternLayout.ConversionPattern = LOG_PATTERN;
                    patternLayout.ActivateOptions();

                    TraceAppender tracer = new TraceAppender();
                    tracer.Layout = patternLayout;
                    tracer.ActivateOptions();

                    RollingFileAppender roller = new RollingFileAppender();
                    roller.Layout = patternLayout;
                    roller.AppendToFile = true;
                    roller.File = "Log/";
                    roller.StaticLogFileName = false;
                    roller.DatePattern = "yyyyMMdd.LOG";
                    roller.RollingStyle = RollingFileAppender.RollingMode.Date;
                    roller.MaximumFileSize = "10MB";
                    roller.MaxSizeRollBackups = 10;
                    roller.ImmediateFlush = true;
                    roller.Encoding = Encoding.UTF8;
                    roller.LockingModel = new FileAppender.MinimalLock();
                    roller.ActivateOptions();

                    Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
                    hierarchy.Name = "log";
                    hierarchy.Root.AddAppender(tracer);
                    hierarchy.Root.AddAppender(roller);
                    hierarchy.Root.Level = lev;
                    hierarchy.Configured = true;

                    logger = LogManager.GetLogger("log");
                }

                if (ex == null)
                {
                    switch (level)
                    {
                        case Level.Debug:
                            logger.Debug(message);
                            break;

                        case Level.Error:
                            logger.Error(message);
                            break;

                        case Level.Fatal:
                            logger.Fatal(message);
                            break;

                        case Level.Info:
                            logger.Info(message);
                            break;

                        case Level.Warn:
                            logger.Warn(message);
                            break;
                    }
                }
                else
                {
                    switch (level)
                    {
                        case Level.Debug:
                            logger.Debug(message, ex);
                            break;

                        case Level.Error:
                            logger.Error(message, ex);
                            break;

                        case Level.Fatal:
                            logger.Fatal(message, ex);
                            break;

                        case Level.Info:
                            logger.Info(message, ex);
                            break;

                        case Level.Warn:
                            logger.Warn(message, ex);
                            break;
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public enum Level
    {
        /// <summary>
        /// 调试级别
        /// </summary>
        Debug,

        /// <summary>
        /// 错误级别
        /// </summary>
        Error,

        /// <summary>
        /// 致命错误级别
        /// </summary>
        Fatal,

        /// <summary>
        /// 一般级别
        /// </summary>
        Info,

        /// <summary>
        /// 警告级别
        /// </summary>
        Warn
    }

    public static bool WriteInfo(object message)
    {
        return WriteLog(message, null, Level.Info);
    }

    public static bool Info(object message)
    {
        return WriteInfo(message);
    }

    public static bool WriteDebug(object message, Exception ex = null)
    {
        return WriteLog(message, ex, Level.Debug);
    }

    public static bool Debug(object message, Exception ex = null)
    {
        return WriteDebug(message, ex);
    }

    public static bool WriteWarn(object message, Exception ex = null)
    {
        return WriteLog(message, ex, Level.Warn);
    }

    public static bool Warn(object message, Exception ex = null)
    {
        return WriteWarn(message, ex);
    }

    public static bool WriteError(Exception ex, object message = null)
    {
        if (message == null || string.IsNullOrWhiteSpace(message.ToString()))
        {
            if (ex != null)
                message = ex.Message;
        }
        return WriteLog(message, ex, Level.Error);
    }

    public static bool Error(Exception ex, object message = null)
    {
        return WriteError(ex, message);
    }

    public static bool Error(object message, Exception ex = null)
    {
        return WriteError(ex, message);
    }

    public static bool WriteFatal(Exception ex, object message = null)
    {
        if (message == null || string.IsNullOrWhiteSpace(message.ToString()))
        {
            if (ex != null)
                message = ex.Message;
        }
        return WriteLog(message, ex, Level.Fatal);
    }

    public static bool Fatal(Exception ex, object message = null)
    {
        return WriteFatal(ex, message);
    }
}
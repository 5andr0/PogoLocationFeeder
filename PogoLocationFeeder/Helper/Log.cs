using log4net;
using log4net.Config;
using log4net.Layout.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net.Core;
using System.IO;
using log4net.Appender;
using PogoLocationFeeder.Common;
using PoGo.LocationFeeder.Settings;

namespace PogoLocationFeeder.Helper
{

    public static class Log
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(Log));
        public static readonly log4net.Core.Level pokemonLevel = new log4net.Core.Level(log4net.Core.Level.Info.Value + 1000, "PKMN");

        static Log()
        {
            XmlConfigurator.Configure();
        }
        const string timeFormat = "HH:mm:ss";
        private static object _MessageLock = new object();

        private static void ModuleWrite(string message, LogLevel level = LogLevel.Info, ConsoleColor color = ConsoleColor.Black)
        {
            if (GlobalSettings.Output != null)
                GlobalSettings.Output.Write(message, level, color);
        }

        private static void ModuleWrite(string message, Exception e, LogLevel level = LogLevel.Info, ConsoleColor color = ConsoleColor.Black)
        {
            if (GlobalSettings.Output != null)
                GlobalSettings.Output.Write(message+" "+e.Message, level, color);
        }

        private static void ModuleWriteFormat(string message, params object[] args)
        {
            if (GlobalSettings.Output != null)
                GlobalSettings.Output.WriteFormat(message, args);
        }

        public static void Debug(string message, params string[] args)
        {
            if (args == null)
            {
                logger.Debug(message);
                ModuleWrite(message, LogLevel.Debug);
            }
            else
            {
                logger.DebugFormat(message, args);
                ModuleWriteFormat(message, args);
            }
        }

        public static void Info(string message, params string[] args)
        {
            if (args == null)
            {
                logger.Info(message);
                ModuleWrite(message, LogLevel.Info);
            }
            else
            {
                logger.InfoFormat(message, args);
                ModuleWriteFormat(message, args);
            }
        }

        public static void Plain(string message, params  string[] args)
        {
            lock (_MessageLock)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(message, args);
                Console.ResetColor();
            }
            ModuleWrite(message);
        }

        public static void Pokemon(string message, params string[] args)
        {
            if (args == null)
            {
                logger.LogPokemon(message);
            }
            else
            {
                logger.LogPokemonFormat(message, args);
            }
        }
        public static void Warn(string message, params string[] args)
        {
            if (args == null)
            {
                logger.Warn(message);
                ModuleWrite(message, LogLevel.Warning);
            }
            else
            {
                logger.WarnFormat(message, args);
                ModuleWriteFormat(message, args);
            }
        }


        public static void Warn(string message, Exception e)
        {
            logger.Warn(message, e);
            ModuleWrite(message, e, LogLevel.Warning);
        }

        public static void Error(string message, params string[] args)
        {
            if (args == null)
            {
                logger.Error(message);
                ModuleWrite(message, LogLevel.Error);
            }
            else
            {
                logger.ErrorFormat(message, args);
                ModuleWriteFormat(message, args);
            }
        }

        public static void Error(string message, Exception e)
        {
            logger.Error(message, e);
            ModuleWrite(message, e, LogLevel.Error);
        }

        public static void Fatal(string message, params string[] args)
        {
            if (args == null)
            {
                logger.Fatal(message);
                ModuleWrite(message, LogLevel.Error);
            }
            else
            {
                logger.FatalFormat(message, args);
                ModuleWriteFormat(message, args);
            }
        }

        public static void Fatal(string message, Exception e)
        {
            logger.Fatal(message, e);
            ModuleWrite(message, e, LogLevel.Error);
        }
        private static void LogPokemon(this ILog log, string message)
        {
            log.Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType,
                pokemonLevel, message, null);
        }

        public static void LogPokemonFormat(this ILog log, string message, params object[] args)
        {
            string formattedMessage = string.Format(message, args);
            log.Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType,
                pokemonLevel, formattedMessage, null);
        }
    }

    public class CustomColoredConsoleAppender : ManagedColoredConsoleAppender
    {
        public CustomColoredConsoleAppender()
        {
            AddMapping(new ManagedColoredConsoleAppender.LevelColors
            {
                Level = Log.pokemonLevel,
                ForeColor = ConsoleColor.Green
            });
        }
    }
}

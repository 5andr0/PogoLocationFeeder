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
        const String timeFormat = "HH:mm:ss";
        private static object _MessageLock = new object();

        public static void Trace(String message, params string[] args)
        {
            logger.DebugFormat(message, args);
        }

        public static void Debug(String message, params string[] args)
        {
            logger.DebugFormat(message, args);
        }

        public static void Info(String message, params string[] args)
        {
            logger.InfoFormat( message, args);
        }

        public static void Plain(String message, params  string[] args)
        {
            lock (_MessageLock)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(message, args);
                Console.ResetColor();
            }
        }

        public static void Pokemon(String message, params string[] args)
        {

            LogPokemonFormat(logger, message, args);
        }
        public static void Warn(String message, params string[] args)
        {
            logger.WarnFormat(message, args);
        }


        public static void Warn(String message, Exception e)
        {
            logger.Warn(message, e);
        }

        public static void Error(String message, params string[] args)
        {
            logger.ErrorFormat(message, args);
        }

        public static void Error(String message, Exception e)
        {
            logger.Error(message, e);
        }

        public static void Fatal(String message, params string[] args)
        {
            logger.FatalFormat(message, args);
        }

        public static void Fatal(String message, Exception e)
        {
            logger.Fatal(message, e);
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

    public class CustomColoredConsoleAppender : ColoredConsoleAppender
    {
        public CustomColoredConsoleAppender()
        {
            AddMapping(new ColoredConsoleAppender.LevelColors
            {
                Level = Log.pokemonLevel,
                ForeColor = ColoredConsoleAppender.Colors.Green
            });
        }
    }
}

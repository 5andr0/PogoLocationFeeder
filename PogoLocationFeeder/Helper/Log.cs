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
        const string timeFormat = "HH:mm:ss";
        private static object _MessageLock = new object();

        public static void Debug(string message, params string[] args)
        {
            if (args == null)
            {
                logger.Debug(message);
            }
            else
            {
                logger.DebugFormat(message, args);
            }
        }

        public static void Info(string message, params string[] args)
        {
            if (args == null)
            {
                logger.Info(message);
            }
            else
            {
                logger.InfoFormat(message, args);
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
            }
            else
            {
                logger.WarnFormat(message, args);
            }
        }


        public static void Warn(string message, Exception e)
        {
            logger.Warn(message, e);
        }

        public static void Error(string message, params string[] args)
        {
            if (args == null)
            {
                logger.Error(message);
            }
            else
            {
                logger.ErrorFormat(message, args);
            }
        }

        public static void Error(string message, Exception e)
        {
            logger.Error(message, e);
        }

        public static void Fatal(string message, params string[] args)
        {
            if (args == null)
            {
                logger.Fatal(message);
            }
            else
            {
                logger.FatalFormat(message, args);
            }
        }

        public static void Fatal(string message, Exception e)
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

using System;
using System.Reflection;
using log4net;
using log4net.Appender;
using log4net.Core;

namespace PogoLocationFeeder.Helper
{
    public static class Log
    {
        //private const string timeFormat = "HH:mm:ss";
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Log));
        public static readonly Level pokemonLevel = new Level(Level.Info.Value + 1000, "PKMN");
        private static readonly object _MessageLock = new object();

        static Log()
        {
            //XmlConfigurator.Configure(); // we are loading from the embedded resource file App.config so we don't have to deliver the config file
        }

        public static void Debug(string message, params string[] args)
        {
            if (args == null)
            {
                Logger.Debug(message);
            }
            else
            {
                Logger.DebugFormat(message, args);
            }
        }

        public static void Info(string message, params string[] args)
        {
            if (args == null)
            {
                Logger.Info(message);
            }
            else
            {
                Logger.InfoFormat(message, args);
            }
        }

        public static void Plain(string message, params string[] args)
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
                Logger.LogPokemon(message);
            }
            else
            {
                Logger.LogPokemonFormat(message, args);
            }
        }

        public static void Warn(string message, params string[] args)
        {
            if (args == null)
            {
                Logger.Warn(message);
            }
            else
            {
                Logger.WarnFormat(message, args);
            }
        }


        public static void Warn(string message, Exception e)
        {
            Logger.Warn(message + '\n', e);
        }

        public static void Error(string message, params string[] args)
        {
            if (args == null)
            {
                Logger.Error(message);
            }
            else
            {
                Logger.ErrorFormat(message, args);
            }
        }

        public static void Error(string message, Exception e)
        {
            Logger.Error(message + '\n', e);
        }

        public static void Fatal(string message, params string[] args)
        {
            if (args == null)
            {
                Logger.Fatal(message);
            }
            else
            {
                Logger.FatalFormat(message + '\n', args);
            }
        }

        public static void Fatal(string message, Exception e)
        {
            Logger.Fatal(message + '\n', e);
        }

        private static void LogPokemon(this ILog log, string message)
        {
            log.Logger.Log(MethodBase.GetCurrentMethod().DeclaringType,
                pokemonLevel, message, null);
        }

        public static void LogPokemonFormat(this ILog log, string message, params object[] args)
        {
            var formattedMessage = string.Format(message, args);
            log.Logger.Log(MethodBase.GetCurrentMethod().DeclaringType,
                pokemonLevel, formattedMessage, null);
        }
    }

    public class CustomColoredConsoleAppender : ManagedColoredConsoleAppender
    {
        public CustomColoredConsoleAppender()
        {
            AddMapping(new LevelColors
            {
                Level = Log.pokemonLevel,
                ForeColor = ConsoleColor.Green
            });
        }
    }
}
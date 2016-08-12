/*
PogoLocationFeeder gathers pokemon data from various sources and serves it to connected clients
Copyright (C) 2016  PogoLocationFeeder Development Team <admin@pokefeeder.live>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation, either version 3 of the
License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

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
        public static readonly Level PokemonLevel = new Level(Level.Info.Value + 1000, "PKMN");
        public static readonly Level TraceLevel = new Level(Level.Debug.Value - 1000, "TRACE");

        private static readonly object _MessageLock = new object();

        static Log()
        {
            //XmlConfigurator.Configure(); // we are loading from the embedded resource file App.config so we don't have to deliver the config file
        }
        public static void Trace(string message, params string[] args)
        {
            if (args == null)
            {
                Trace(message);
            }
            else
            {
                TraceFormat(Logger, message, args);
            }
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

        public static void Debug(string message, Exception e)
        {
            Logger.Debug(message, e);
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
                PokemonLevel, message, null);
        }

        public static void LogPokemonFormat(this ILog log, string message, params object[] args)
        {
            var formattedMessage = string.Format(message, args);
            log.Logger.Log(MethodBase.GetCurrentMethod().DeclaringType,
                PokemonLevel, formattedMessage, null);
        }

        private static void Trace(this ILog log, string message)
        {
            log.Logger.Log(MethodBase.GetCurrentMethod().DeclaringType,
                TraceLevel, message, null);
        }

        public static void TraceFormat(this ILog log, string message, params object[] args)
        {
            var formattedMessage = string.Format(message, args);
            log.Logger.Log(MethodBase.GetCurrentMethod().DeclaringType,
                TraceLevel, formattedMessage, null);
        }
    }

    public class CustomColoredConsoleAppender : ManagedColoredConsoleAppender
    {
        public CustomColoredConsoleAppender()
        {
            AddMapping(new LevelColors
            {
                Level = Log.PokemonLevel,
                ForeColor = ConsoleColor.Green
            });
        }
    }
}

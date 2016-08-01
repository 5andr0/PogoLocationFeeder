using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PogoLocationFeeder.Helper
{
    public class Log
    {
        const String timeFormat = "HH:mm:ss";
        private static object _MessageLock = new object();
        public static void debug(String message)
        {
            debug(message, null);
        }

        public static void debug(String message, params string[] args)
        {
            writeMessage("[DEBUG] " + message, ConsoleColor.DarkCyan, args);
        }

        public static void info(String message, params string[] args)
        {
            writeMessage("[INFO] " + message, ConsoleColor.DarkCyan, args);
        }

        public static void plain(String message, params  string[] args)
        {
            lock (_MessageLock)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(message, args);
                Console.ResetColor();
            }
        }

        public static void pokemon(String message, params string[] args)
        {
            writeMessage("[PKMN] " + message, ConsoleColor.Green, args);
        }
        public static void warn(String message, params string[] args)
        {
            writeMessage("[WARN] " + message, ConsoleColor.DarkYellow, args);
        }

        public static void error(String message, params string[] args)
        {
            writeMessage("[ERROR] " + message, ConsoleColor.Red, args);
        }
        private static void writeMessage(String message, string[] args)
        {
            lock (_MessageLock)
            {
                writeToConsole(message, args);
            }
        }

        private static void writeMessage(String message, ConsoleColor consoleColor, params string[] args)
        {
            lock (_MessageLock)
            {
                Console.ForegroundColor = consoleColor;
                writeToConsole(message, args);
                Console.ResetColor();
            }
        }

        private static void writeToConsole(String message, params string[] args)
        {
            Console.WriteLine($"[{DateTime.Now.ToString(timeFormat)}]" + message, args);

        }
    }

}

using System;

namespace ArcUpdater.CommandLine
{
    internal static class ConsoleHelper
    {
        public static void WriteErrorLine(string value)
        {
            WriteError(value);
            Console.WriteLine();
        }

        public static void WriteError(string value)
        {
            ConsoleColor previousColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(value);
            Console.ForegroundColor = previousColor;
        }
    }
}

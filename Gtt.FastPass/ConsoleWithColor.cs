using System;
using System.Collections.Generic;
using System.Text;

namespace Gtt.FastPass
{
    public class ConsoleWithColor : IDisposable
    {
        private ConsoleColor currentForeground;
        private ConsoleColor currentBackground;
        public ConsoleWithColor(ConsoleColor color)
        {
            currentForeground = Console.ForegroundColor;
            currentBackground = Console.BackgroundColor;

            Console.ForegroundColor = color;
        }

        public ConsoleWithColor(ConsoleColor foregroundColor, ConsoleColor backgroundColor)
        {
            currentForeground = Console.ForegroundColor;
            currentBackground = Console.BackgroundColor;

            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
        }

        public void WriteLine()
        {
            Console.WriteLine();
        }

        public void WriteLine(string message)
        {
            Console.WriteLine(message);
        }

        public void Write(string message)
        {
            Console.Write(message);
        }

        public void Write(string message, ConsoleColor color)
        {
            using (var cw = new ConsoleWithColor(color))
            {
                cw.Write(message);
            }
        }

        public void Dispose()
        {
            Console.BackgroundColor = currentBackground;
            Console.ForegroundColor = currentForeground;
        }
    }
}

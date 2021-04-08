using System;
using System.Collections.Generic;
using System.Text;

namespace Gtt.FastPass
{
    public class ConsoleWithColor : IDisposable
    {
        private readonly ConsoleColor _currentForeground;
        private readonly ConsoleColor _currentBackground;
        private readonly StringBuilder _sb = new StringBuilder();
        public ConsoleWithColor(ConsoleColor color)
        {
            _currentForeground = Console.ForegroundColor;
            _currentBackground = Console.BackgroundColor;

            Console.ForegroundColor = color;
        }

        public ConsoleWithColor(ConsoleColor foregroundColor, ConsoleColor backgroundColor)
        {
            _currentForeground = Console.ForegroundColor;
            _currentBackground = Console.BackgroundColor;

            Console.ForegroundColor = foregroundColor;
            Console.BackgroundColor = backgroundColor;
        }

        public void WriteLine()
        {
            _sb.AppendLine();
            Console.WriteLine();
        }

        public void WriteLine(string message)
        {
            _sb.AppendLine(message);
            Console.WriteLine(message);
        }

        public void Write(string message)
        {
            _sb.Append(message);
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
            Console.BackgroundColor = _currentBackground;
            Console.ForegroundColor = _currentForeground;
        }
    }
}

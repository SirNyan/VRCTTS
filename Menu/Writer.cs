using System;
using System.Collections.Generic;
using System.Text;

namespace VRCTTS
{
    class Writer
    {
        public void writeAt(int x, int y, string message, ConsoleColor foreground = ConsoleColor.White, ConsoleColor background = ConsoleColor.Black)
        {
            Console.ForegroundColor = foreground;
            Console.BackgroundColor = background;
            Console.SetCursorPosition(x, y);
            Console.Write(message);
            Console.ResetColor();
        }

        public void ClearLine(int line, int pos_x = 0)
        {
            Console.SetCursorPosition(pos_x, line);
            Console.Write(new string(' ', Console.WindowWidth-pos_x));
        }
    }
}

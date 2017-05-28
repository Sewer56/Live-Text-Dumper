using System;

namespace VisualizerCurrentTime
{
    public static class ConsoleX
    {
        public static void WriteSystemLine(string TextToWrite)
        {
            var OldBGColour = Console.BackgroundColor;
            var OldFGColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.SetCursorPosition((Console.WindowWidth - TextToWrite.Length) / 2, Console.CursorTop);
            Console.WriteLine(TextToWrite);
            Console.BackgroundColor = OldBGColour;
            Console.ForegroundColor = OldFGColour;
        }

        public static void WriteDebugHeader(string TextToWrite)
        {
            var OldBGColour = Console.BackgroundColor;
            var OldFGColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine(TextToWrite);
            Console.BackgroundColor = OldBGColour;
            Console.ForegroundColor = OldFGColour;
        }

        public static void WriteDebugMessage(string TextToWrite)
        {
            var OldFGColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(TextToWrite);
            Console.ForegroundColor = OldFGColour;
        }

        public static void WriteLineCenter(string TextToWrite)
        {
            Console.SetCursorPosition((Console.WindowWidth - TextToWrite.Length) / 2, Console.CursorTop);
            Console.WriteLine(TextToWrite);
        }
    }
}

using System;

namespace VisualizerCurrentTime
{
    /// <summary>
    /// Customised Console Class which allows for personal flavoured console theming.
    /// </summary>
    public static class CustomConsole
    {
        /// <summary>
        /// Writes to the center of the console with inverted colours.
        /// </summary>
        public static void WriteSystemLine(string TextToWrite)
        {
            // Backup the old console colours.
            var oldBackgroundColour = System.Console.BackgroundColor;
            var oldForegroundColour = System.Console.ForegroundColor;

            // Invert the background and foreground console colours.
            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.BackgroundColor = ConsoleColor.Black;
            
            // Write centered text to the console.
            System.Console.SetCursorPosition((System.Console.WindowWidth - TextToWrite.Length) / 2, System.Console.CursorTop);
            System.Console.WriteLine(TextToWrite);

            // Restore the original console colours.
            System.Console.BackgroundColor = oldBackgroundColour;
            System.Console.ForegroundColor = oldForegroundColour;
        }

        /// <summary>
        /// Prints a debug title header to the console. 
        /// </summary>
        /// <param name="TextToWrite"></param>
        public static void WriteDebugHeader(string TextToWrite)
        {
            // Backup the old console colours.
            var oldBackgroundColour = System.Console.BackgroundColor;
            var oldForegroundColour = System.Console.ForegroundColor;

            // Invert the colours of the console.
            System.Console.ForegroundColor = ConsoleColor.Black;
            System.Console.BackgroundColor = ConsoleColor.White;

            // Write the text to the console.
            System.Console.WriteLine(TextToWrite);

            // Restore the original console colours.
            System.Console.BackgroundColor = oldBackgroundColour;
            System.Console.ForegroundColor = oldForegroundColour;
        }

        /// <summary>
        /// Writes a debug message to the console, custom console styling.
        /// </summary>
        public static void WriteDebugMessage(string TextToWrite)
        {
            // Backup the old console colour.
            var oldForegroundColour = System.Console.ForegroundColor;

            // Set the new colour to Cyan
            System.Console.ForegroundColor = ConsoleColor.Cyan;

            // Print to the console screen.
            System.Console.WriteLine(TextToWrite);

            // Restore the old original console colour.
            System.Console.ForegroundColor = oldForegroundColour;
        }

        /// <summary>
        /// Outputs a line to the center to the screen.
        /// </summary>
        public static void WriteLineCenter(string TextToWrite)
        {
            System.Console.SetCursorPosition((System.Console.WindowWidth - TextToWrite.Length) / 2, System.Console.CursorTop);
            System.Console.WriteLine(TextToWrite);
        }
    }
}

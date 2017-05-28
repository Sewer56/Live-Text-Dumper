using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace VisualizerCurrentTime
{
    class Program
    {
        // Debug mode.

        // This will be enecessary to convert KeyCodes to Chars.
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int ToUnicode(
            uint virtualKeyCode,
            uint scanCode,
            byte[] keyboardState,
            StringBuilder receivingBuffer,
            int bufferSize,
            uint flags
        );

        // I/O
        public static string FileInformationExportPath = ""; // Where the current info goes.
        public static string FileLiveTextExportPath = ""; // Where the current text goes.
        public static bool OptionExportInformation = false; // Am I writing to a file?
        public static bool OptionExportText = false; // Am I writing to a file?
        public static bool OptionPrintText = false; // Am I gonna print to the console screen?
        public static System.Windows.Forms.KeysConverter TextKeyConverter = new System.Windows.Forms.KeysConverter(); // Converts WinForm key IDs to text keys.

        // Export Information
        public static string SongTitle; // Current Song Title
        public static string CurrentTime; // What time is it?
        public static StringBuilder CurrentOnScreenText = new StringBuilder(100); // What text am I displaying?
        public static string TextToPrint;

        // Miscallenous
        public static Process[] MusicBeeProcess; // List of active processes.
        public static bool IsDumpingInfo = true; // Am I dumping info?
        public static bool IsCurrentlyTyping; // Am I currently typing right now?
        public static bool IsHoldingShift; // Am I holding shift?
        public static bool IsResettingAssignments; // Am I resetting hotkeys?
        public static List<int> HotkeyIDs = new List<int>(); // Stores all assigned IDs?
        public static bool ExtraDebugging; // Display extra debugging info?


        // Main Method, it all starts here.
        static void Main(string[] args)
        {
            WriteIntroduction();
            AssignSettings();
            AssignKeys();

            // Introduce another thread, purpose: Faster export of text!
            Thread DumpTextToFileThread = new Thread
            (
                 new ThreadStart(DumpTextToFile)
            );
            DumpTextToFileThread.Start();


            // Main loop!
            while (IsDumpingInfo == true)
            {
                GetInformation();
                SetTextToPrint();

                if (OptionPrintText == true) { PrintInformation(); }
                DumpInformationToFile();

                Thread.Sleep(998);              
            }
        }

        // Write information to file
        public static void DumpInformationToFile()
        {
                try
                {
                    if (OptionExportInformation == true) { File.WriteAllText(FileInformationExportPath, TextToPrint); }
                }
                catch (Exception Nope)
                { }
        }

        // Write text
        public static void DumpTextToFile()
        {
            while (IsDumpingInfo == true)
            {
                try
                {
                    if (OptionExportText == true) { File.WriteAllText(FileLiveTextExportPath, Convert.ToString(CurrentOnScreenText)); }
                }
                catch (Exception Nope) 
                { }
                #if DEBUG
                if (ExtraDebugging) Console.WriteLine("Dumping Text!");
                #endif
                Thread.Sleep(50);
            }
        }

        public static void AssignSettings()
        {
            string InputResponse = "";

            // File Path for Information Export
            while (FileInformationExportPath.Length == 0)
            {
                Console.Write("Please set the file path to output info to ['none' for no export]: ");
                FileInformationExportPath = Console.ReadLine();
                if (FileInformationExportPath != "none" && FileInformationExportPath.Length != 0) { OptionExportInformation = true; }
            }

            // File Path for Text Export
            while (FileLiveTextExportPath.Length == 0)
            {
                Console.Write("Please set the file path to output text to ['none' for no export]: ");
                FileLiveTextExportPath = Console.ReadLine();
                if (FileLiveTextExportPath != "none" && FileLiveTextExportPath.Length != 0) { OptionExportText = true; }
            }

            // Print to screen?
            while (InputResponse != "Y" && InputResponse != "N")
            {
                Console.Write("Do you want to also print to the screen? [Y/N]: ");
                InputResponse = Console.ReadLine();
                if (InputResponse == "Y") { OptionPrintText = true; }
                else if (InputResponse == "N") { OptionPrintText = false; }
            }

            InputResponse = "";
            // Extra debugging?
            while (InputResponse != "Y" && InputResponse != "N")
            {
                Console.Write("Do you want to print extra debugging information? [Y/N]: ");
                InputResponse = Console.ReadLine();
                if (InputResponse == "Y") { ExtraDebugging = true; }
                else if (InputResponse == "N") { ExtraDebugging = false; }
            }
        }

        public static void AssignKeys()
        {
            // Only hijack used hotkeys.
            HotKeyManager.RegisterHotKey(Keys.D4, KeyModifiers.Alt);
            HotKeyManager.RegisterHotKey(Keys.D5, KeyModifiers.Alt);
            HotKeyManager.RegisterHotKey(Keys.D6, KeyModifiers.Alt);
            HotKeyManager.RegisterHotKey(Keys.D7, KeyModifiers.Alt);

            // Fire if hotkey is struck.
            HotKeyManager.HotKeyPressed += new EventHandler<HotKeyEventArgs>(ResolveHotkey);
        }

        public static void AssignAllKeys()
        {
            // Assign EVERY KEY (originally was just base A-Z but then we have symbols etc.
            foreach (Keys Keyboardkey in Enum.GetValues(typeof(Keys)))
            {
                HotKeyManager.RegisterHotKey(Keyboardkey, KeyModifiers.NoRepeat);
                HotKeyManager.RegisterHotKey(Keyboardkey, KeyModifiers.Alt);
                HotKeyManager.RegisterHotKey(Keyboardkey, KeyModifiers.Control);
                HotKeyManager.RegisterHotKey(Keyboardkey, KeyModifiers.Shift);
                HotKeyManager.RegisterHotKey(Keyboardkey, KeyModifiers.Windows);
            }

            // THIS HIJACKS THE ENTIRE KEYBOARD!
        }

        // Back to factory settings
        public static void ResetKeyAssignment()
        {
            if (IsResettingAssignments == false)
            {
                IsResettingAssignments = true;
                for (int x = 0; x < HotkeyIDs.Count; x++)
                {
                    HotKeyManager.UnregisterHotKey(HotkeyIDs[x]);
                    if (ExtraDebugging) Console.WriteLine("Key unmapped: " + HotkeyIDs[x]);
                }

                HotkeyIDs.Clear();

                if (ExtraDebugging)
                {
                    foreach (int HotkeyID in HotkeyIDs)
                    {
                        Console.WriteLine("This key shouldn't be mapped: " + HotkeyID + " | If you see this, tell Sewer he ****** up.");
                    }
                }

                // Only hijack used hotkeys.
                HotKeyManager.RegisterHotKey(Keys.D4, KeyModifiers.Alt);
                HotKeyManager.RegisterHotKey(Keys.D5, KeyModifiers.Alt);
                HotKeyManager.RegisterHotKey(Keys.D6, KeyModifiers.Alt);
                HotKeyManager.RegisterHotKey(Keys.D7, KeyModifiers.Alt);

                Thread.Sleep(400);
                IsResettingAssignments = false;
            }
        }

        public static void WriteIntroduction()
        {
            ConsoleX.WriteSystemLine("------------------------------------");
            ConsoleX.WriteSystemLine("Live Text Information Dumper");
            ConsoleX.WriteSystemLine("Quick Utility for Personal Purposes\n");

            ConsoleX.WriteSystemLine("Start Typing Text to File: Alt + 4");
            ConsoleX.WriteSystemLine("Stop Typing Text: Alt + 5");
            ConsoleX.WriteSystemLine("Clear Typed Text: Alt + 6");
            ConsoleX.WriteSystemLine("End the Utility: Alt + 7 (Disabled)");
            ConsoleX.WriteSystemLine("------------------------------------");
        }

        public static void ResolveHotkey(object sender, HotKeyEventArgs HotkeyArguments)
        {
            if (ExtraDebugging) ConsoleX.WriteDebugHeader("Input Received!");

            // Check for known hotkeys
            if (HotkeyArguments.Key == Keys.D4 && HotkeyArguments.Modifiers == KeyModifiers.Alt) { StartTyping(); }
            else if (HotkeyArguments.Key == Keys.D5 && HotkeyArguments.Modifiers == KeyModifiers.Alt) { StopTyping(); }
            else if (HotkeyArguments.Key == Keys.D6 && HotkeyArguments.Modifiers == KeyModifiers.Alt) { ClearTyping(); }
            else if (HotkeyArguments.Key == Keys.D7 && HotkeyArguments.Modifiers == KeyModifiers.Alt) { ApplicationExit(); }

            // Else if in text editor or typing mode
            else if (IsCurrentlyTyping == true)
            {

                // Validate special control keys!
                if (HotkeyArguments.Key == Keys.Back) { if (CurrentOnScreenText.Length > 0) { CurrentOnScreenText.Remove(CurrentOnScreenText.Length - 1, 1); } ConsoleX.WriteDebugMessage("Text Removed, New Text: " + CurrentOnScreenText); }
                else if (HotkeyArguments.Key == Keys.Space) { CurrentOnScreenText.Append(" "); }
                else
                {
                    // Are we holding shift?
                    if (HotkeyArguments.Modifiers == KeyModifiers.Shift) { IsHoldingShift = true; } else { IsHoldingShift = false; }

                    // Okay, time for Unicode conversion!
                    string PressedKey = GetCharactersFromKeys(HotkeyArguments.Key, IsHoldingShift);

                    // If it is a control character we probably do not want it.
                    if (PressedKey.Length == 1)
                    {
                        CurrentOnScreenText.Append(PressedKey);
                        if (ExtraDebugging) ConsoleX.WriteDebugMessage("Current Text: " + CurrentOnScreenText + " | " + "Pressed Key: " + HotkeyArguments.Key + " | " + "Resolved Key: " + PressedKey);
                        else { Console.Clear(); ConsoleX.WriteDebugMessage("Current Text: " + CurrentOnScreenText); }
                    }
                }
            }
        }

        public static void SetTextToPrint()
        {
            TextToPrint = CurrentTime + " | " + SongTitle;
        }

        public static void PrintCurrentLiveText()
        {
            Console.WriteLine(CurrentOnScreenText);
        }

        public static void StartTyping()
        {
            ConsoleX.WriteDebugHeader("Start Typing!");
            IsCurrentlyTyping = true;
            AssignAllKeys();

            Thread LiveTypingThread = new Thread
            (
                new ThreadStart(TextTypingHandler)    
            );
        }

        public static void TextTypingHandler()
        {
            while (IsCurrentlyTyping == true)
            {
                // Poll if typing every 500ms, variable changed externally, this is on another thread.
                Thread.Sleep(500);
            }
        }

        public static void StopTyping()
        {
            ConsoleX.WriteDebugHeader("Stop Typing!");
            IsCurrentlyTyping = false;
            ResetKeyAssignment();
        }

        public static void ClearTyping()
        {
            ConsoleX.WriteDebugHeader("Clear The Text!");
            CurrentOnScreenText.Remove(0, CurrentOnScreenText.Length);
        }

        public static void ApplicationExit()
        {
            ConsoleX.WriteDebugHeader("Goodbye!");
            //Application.Exit();
        }

        public static void PrintInformation()
        {
            Console.WriteLine(TextToPrint);
        }

        public static void GetCurrentTime()
        {
            CurrentTime = Convert.ToString(DateTime.Now.ToString("HH:mm:ss"));
        }

        public static void GetInformation()
        {
            GetSongTitle();
            GetCurrentTime();
        }

        public static void GetSongTitle()
        {
            try
            {
                MusicBeeProcess = Process.GetProcessesByName("MusicBee");
                if (MusicBeeProcess.Length != 0) { SongTitle = MusicBeeProcess[0].MainWindowTitle.Substring(0, MusicBeeProcess[0].MainWindowTitle.Length - 11); }
            }
            catch (Exception Nope)
            {

            }
        }

        static string GetCharactersFromKeys(Keys keys, bool shift)
        {
            var buf = new StringBuilder(256);
            var keyboardState = new byte[256];
            if (shift)
            {
                keyboardState[(int)Keys.ShiftKey] = 0xff;
            }
            ToUnicode((uint)keys, 0, keyboardState, buf, 256, 0);
            return buf.ToString();
        }
    }
}

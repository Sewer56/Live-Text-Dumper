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
        ///////////////////
        // I/O
        ///////////////////

        /// <summary>
        /// Specifies the path to where the current information regarding time, music playback, etc. is exported to.
        /// </summary>
        private static string informationExportPath = "";

        /// <summary>
        /// Specifies the path where the currently typed text is continously saved to.
        /// </summary>
        private static string textExportPath = "";

        /// <summary>
        /// Specifies whether the program is currently writing individual information to a file.
        /// </summary>
        private static bool optionExportInformation = false;

        /// <summary>
        /// Specifies whether the program is currently writing user specified text to a file.
        /// </summary>
        private static bool optionExportText = false;

        /// <summary>
        /// Specifies whether the program will addiitonally print to the console screen as well as to the file (Debugging Use).
        /// </summary>
        private static bool optionPrintText = false;

        /// <summary>
        /// Allows for the conversion of the individual WinForms key IDs to text.
        /// </summary>
        private static System.Windows.Forms.KeysConverter keyConverter = new System.Windows.Forms.KeysConverter();

        /////////////////////
        // Information Export
        /////////////////////

        /// <summary>
        /// Specifies the current local time.
        /// </summary>
        private static string currentTime;

        /// <summary>
        /// String builder for the current text that is displayed onscreen via dumping to file and being read in by Open Source Broadcaster.
        /// </summary>
        private static StringBuilder currentVisibleTextBuilder = new StringBuilder(100);

        /// <summary>
        /// String representing the text that is to be exported in the information text file, generally contains current time and song.
        /// </summary>
        private static string textToPrint;

        ///////////////
        // Miscallenous
        ///////////////

        /// <summary>
        /// Declares whether the information is being actively dumped.
        /// </summary>
        private  static bool isDumpingInfo = true;

        /// <summary>
        /// Declares whether the user is actively typing into the set text file.
        /// </summary>
        private static bool isTyping;

        /// <summary>
        /// Declares whether the user is currently holding the SHIFT button.
        /// </summary>
        private static bool isHoldingShift;

        /// <summary>
        /// Declares whether the user is currently resetting any of the set hotkeys.
        /// </summary>
        private static bool isResettingHotkeys;

        /// <summary>
        /// Stores all of the assigned button IDs associated with hotkeys.
        /// </summary>
        public static List<int> hotkeyIDs = new List<int>();

        /// <summary>
        /// Decides whether extra debugging information should be shown and printed to screen.
        /// </summary>
        private static bool extraDebugging; // Display extra debugging info?

        //////////
        // Modules
        //////////

        /// <summary>
        /// Retrieves the currently playing song title from a running MusicBee instance.
        /// </summary>
        private static MusicBee musicBeeModule = new MusicBee();

        /// <summary>
        /// The main entry method for the program in which the execution cycle starts from.
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            // Displays the header of the console application, providing the necessary text and instructions for usage.
            WriteIntroduction();

            // Queries the user the necessary settings such as whether X/Y/Z should be exported or handled.
            AssignSettings();

            // Sets the hotkeys to be used within the space of the application.
            AssignKeys();

            // Introduce another thread which will expor the currently written text at a regular time interval. 
            Thread DumpTextToFileThread = new Thread ( new ThreadStart(DumpTextToFile) );
            DumpTextToFileThread.Start();

            // The main loop of the application!
            while (isDumpingInfo == true)
            {
                // Retrieves the current time, currently playing song and currently typed in text.
                GetInformation();

                // Formats the information that is to be written to the information text file.
                FormatInformationToPrint();

                // If enabled, prints the currently typed in text to the console.
                if (optionPrintText == true) { PrintText(); }

                // Writes out the content of the current playing song and the time to a text file.
                DumpInformationToFile();

                // Sleep for an approximate second of time before updating the information.
                Thread.Sleep(999);              
            }
        }

        /// <summary>
        /// Displays the introduction to the program, providing the necessary information about how to use the program.
        /// </summary>
        private static void WriteIntroduction()
        {
            CustomConsole.WriteSystemLine("------------------------------------");
            CustomConsole.WriteSystemLine("Live Text Information Dumper");
            CustomConsole.WriteSystemLine("Quick Utility for Personal Purposes\n");

            CustomConsole.WriteSystemLine("Start Typing Text to File: Alt + 4");
            CustomConsole.WriteSystemLine("Stop Typing Text: Alt + 5");
            CustomConsole.WriteSystemLine("Clear Typed Text: Alt + 6");
            CustomConsole.WriteSystemLine("End the Utility: Alt + 7 (Disabled)");
            CustomConsole.WriteSystemLine("------------------------------------");
        }
        
        /// <summary>
        /// Queries the user for the individual settings
        /// </summary>
        private static void AssignSettings()
        {
            // File Path for Information Export
            RequestInformationExportPath();

            // File Path for Text Export
            RequestTextExportPath();

            // Print to screen?
            RequestPrintToScreen();

            // Extra debugging?
            RequestExtraDebugging();
        }

        /// <summary>
        /// Requests the user to set the information export path where the current time and song title will be exported.
        /// </summary>
        private static void RequestInformationExportPath()
        {
            // Loop until either set or not set.
            while (informationExportPath.Length == 0)
            {
                // Display the prompt to the user.
                System.Console.Write("Please set the file path to output info to ['none' for no export]: ");

                // Read the text.
                informationExportPath = System.Console.ReadLine();

                // Enable the flag to enable the exporting of information to the text file.
                if (informationExportPath != "none" && informationExportPath.Length != 0) { optionExportInformation = true; }
            }
        }

        /// <summary>
        /// Requests the user to set the text export path whereby the user's currently typed in text will be written to.
        /// </summary>
        private static void RequestTextExportPath()
        {
            // Loop until either set or not set.
            while (textExportPath.Length == 0)
            {
                // Display the prompt to the user.
                System.Console.Write("Please set the file path to output text to ['none' for no export]: ");

                // Read the text.
                textExportPath = System.Console.ReadLine();

                // Enable the flag to enable the exporting of text to the text file.
                if (textExportPath != "none" && textExportPath.Length != 0) { optionExportText = true; }
            }
        }

        /// <summary>
        /// Requests the user whether the currently typed in text should also be printed to the screen.
        /// </summary>
        private static void RequestPrintToScreen()
        {
            // Stores the user's response - the text that the user submits to console.
            string response = "";

            // Loop until either set or not set.
            while (response != "Y" && response != "N")
            {
                // Display the prompt to the user.
                System.Console.Write("Do you want to also print to the screen? [Y/N]: ");

                // Read the text.
                response = System.Console.ReadLine();

                // Toggle the flag to enable/disable the exporting of text to the text file.
                if (response == "Y") { optionPrintText = true; }
                else if (response == "N") { optionPrintText = false; }
            }
        }

        /// <summary>
        /// Requests the user wishes to see any extra debugging information displayed on the screen.
        /// </summary>
        private static void RequestExtraDebugging()
        {
            // Stores the user's response - the text that the user submits to console.
            string response = "";

            // Loop until either set or not set.
            while (response != "Y" && response != "N")
            {
                // Display the prompt to the user.
                System.Console.Write("Do you want to print extra debugging information? [Y/N]: ");

                // Read the text.
                response = System.Console.ReadLine();

                // Toggle the flag to enable/disable extra debugging information in the console.
                if (response == "Y") { extraDebugging = true; }
                else if (response == "N") { extraDebugging = false; }
            }
        }

        /// <summary>
        /// Retrieves the current "information" which is to be written to the text file.
        /// </summary>
        private static void GetInformation()
        {
            musicBeeModule.GetSongTitle();
            GetCurrentTime();
        }

        /// <summary>
        /// Formats the information that is to be written to the information text file.
        /// </summary>
        private static void FormatInformationToPrint() { textToPrint = currentTime + " | " + musicBeeModule.songTitle; }

        /// <summary>
        /// Retrieves the current local system time.
        /// </summary>
        private static void GetCurrentTime() { currentTime = Convert.ToString(DateTime.Now.ToString("HH:mm:ss")); }

        /// <summary>
        /// Prints the currently typed in text to the console.
        /// </summary>
        private static void PrintText() { System.Console.WriteLine(textToPrint); }

        /// <summary>
        /// Prints the current typed in text to the console window.
        /// </summary>
        private static void PrintCurrentLiveText() { System.Console.WriteLine(currentVisibleTextBuilder); }

        /// <summary>
        /// Writes out the content of the current playing song and the time to a text file.
        /// </summary>
        private static void DumpInformationToFile()
        {
            try { if (optionExportInformation == true) { File.WriteAllText(informationExportPath, textToPrint); } }
            catch { }
        }

        /// <summary>
        /// Writes out the content of the currently typed in text to the text file
        /// </summary>
        private static void DumpTextToFile()
        {
            // An infinite loop to export the currently typed in text to file at more regular time intervals.
            while (isDumpingInfo == true)
            {
                // Export the text to file if the option is set.
                try
                {
                    if (optionExportText == true) { File.WriteAllText(textExportPath, Convert.ToString(currentVisibleTextBuilder)); }
                    else { return; } // If we are not exporting, return and finish the thread.
                }
                catch { }

                // Wait for next loop iteration.
                Thread.Sleep(50);
            }
        }

        /// <summary>
        /// Registers the hotkeys to be used within the application, hijacks all of the key inputs from the keyboard.
        /// </summary>
        private static void AssignKeys()
        {
            // Only hijack used hotkeys.
            HotkeyManager.RegisterHotKey(Keys.D4, KeyModifiers.Alt);
            HotkeyManager.RegisterHotKey(Keys.D5, KeyModifiers.Alt);
            HotkeyManager.RegisterHotKey(Keys.D6, KeyModifiers.Alt);
            HotkeyManager.RegisterHotKey(Keys.D7, KeyModifiers.Alt);

            // Fire if hotkey is struck.
            HotkeyManager.hotkeyPressed += new EventHandler<HotKeyEventArgs>(ResolveHotkey);
        }

        /// <summary>
        /// Assigns every key available on the keyboard to enable text typing onto the screen.
        /// </summary>
        private static void AssignAllKeys()
        {
            // Assign EVERY KEY (originally was just base A-Z but then we have symbols etc.
            foreach (Keys keyboardKey in Enum.GetValues(typeof(Keys)))
            {
                HotkeyManager.RegisterHotKey(keyboardKey, KeyModifiers.NoRepeat);
                HotkeyManager.RegisterHotKey(keyboardKey, KeyModifiers.Alt);
                HotkeyManager.RegisterHotKey(keyboardKey, KeyModifiers.Control);
                HotkeyManager.RegisterHotKey(keyboardKey, KeyModifiers.Shift);
                HotkeyManager.RegisterHotKey(keyboardKey, KeyModifiers.Windows);
            }

            // THIS HIJACKS THE ENTIRE KEYBOARD!
        }

        /// <summary>
        /// Once the user stops typing, disable the hijacking of all of keyboard input,  
        /// </summary>
        private static void ResetKeyAssignment()
        {
            // If the user is not currently resetting the hotkeys.
            if (isResettingHotkeys == false)
            {
                // Toggle flag for resetting of the hotkeys.
                isResettingHotkeys = true;

                // Unregister every hotkey action for all of the assigned keys.
                for (int x = 0; x < hotkeyIDs.Count; x++)
                {
                    HotkeyManager.UnregisterHotKey(hotkeyIDs[x]);
                    if (extraDebugging) System.Console.WriteLine("Key unmapped: " + hotkeyIDs[x]);
                }

                // Clear all of the assigned hotkey IDs.
                hotkeyIDs.Clear();

                // Retrieves the list of still assigned keys within the application.
                CheckAssignedKeys();

                // Only hijack used hotkeys.
                HotkeyManager.RegisterHotKey(Keys.D4, KeyModifiers.Alt);
                HotkeyManager.RegisterHotKey(Keys.D5, KeyModifiers.Alt);
                HotkeyManager.RegisterHotKey(Keys.D6, KeyModifiers.Alt);
                HotkeyManager.RegisterHotKey(Keys.D7, KeyModifiers.Alt);

                // Sleep Arbitrarily
                Thread.Sleep(400);
                isResettingHotkeys = false;
            }
        }

        /// <summary>
        /// Retrieves the list of still assigned keys within the application if the user requested extra debugging.
        /// </summary>
        private static void CheckAssignedKeys()
        {
            if (extraDebugging)
            {
                foreach (int HotkeyID in hotkeyIDs)
                {
                    System.Console.WriteLine("This key shouldn't be mapped: " + HotkeyID + " | If you see this, tell Sewer he ****** up.");
                }
            }
        }

        /// <summary>
        /// Resolves the currently pressed key and performs an appropriate event.
        /// </summary>
        private static void ResolveHotkey(object sender, HotKeyEventArgs HotkeyArguments)
        {
            // Prints to the console if extra debugging is enabled.
            if (extraDebugging) CustomConsole.WriteDebugHeader("Input Received!");

            // Check for known hotkeys
            if (HotkeyArguments.Key == Keys.D4 && HotkeyArguments.Modifiers == KeyModifiers.Alt) { StartTyping(); }
            else if (HotkeyArguments.Key == Keys.D5 && HotkeyArguments.Modifiers == KeyModifiers.Alt) { StopTyping(); }
            else if (HotkeyArguments.Key == Keys.D6 && HotkeyArguments.Modifiers == KeyModifiers.Alt) { ClearTyping(); }
            else if (HotkeyArguments.Key == Keys.D7 && HotkeyArguments.Modifiers == KeyModifiers.Alt) { ApplicationExit(); }

            // Else if in text editor or typing mode
            else if (isTyping) { TextEditorTypingMode(HotkeyArguments); }
        }

        /// <summary>
        /// Resolves
        /// </summary>
        private static void TextEditorTypingMode(HotKeyEventArgs HotkeyArguments)
        {
            // Validate special control keys!
            if (HotkeyArguments.Key == Keys.Back) { RemoveLastCharacter(); }
            else if (HotkeyArguments.Key == Keys.Space) { currentVisibleTextBuilder.Append(" "); }
            else
            {
                // Are we holding shift?
                if (HotkeyArguments.Modifiers == KeyModifiers.Shift) { isHoldingShift = true; } else { isHoldingShift = false; }

                // Convert pressed key to Unicode conversion!
                string PressedKey = GetCharactersFromKeys(HotkeyArguments.Key, isHoldingShift);

                // If it is a control character, discard the current input.
                if (PressedKey.Length == 1)
                {
                    currentVisibleTextBuilder.Append(PressedKey);
                    if (extraDebugging) CustomConsole.WriteDebugMessage("Current Text: " + currentVisibleTextBuilder + " | " + "Pressed Key: " + HotkeyArguments.Key + " | " + "Resolved Key: " + PressedKey);
                    else { System.Console.Clear(); CustomConsole.WriteDebugMessage("Current Text: " + currentVisibleTextBuilder); }
                }
            }
        }

        /// <summary>
        /// Removes the last character from the local stringbuilder instance
        /// </summary>
        private static void RemoveLastCharacter()
        {
            // Remove the last character from the StringBuilder.
            if (currentVisibleTextBuilder.Length > 0) { currentVisibleTextBuilder.Remove(currentVisibleTextBuilder.Length - 1, 1); }

            // Display new message.
            CustomConsole.WriteDebugMessage("Text Removed, New Text: " + currentVisibleTextBuilder);
        }

        /// <summary>
        /// Starts the typing sequence for custom text.
        /// </summary>
        private static void StartTyping()
        {
            CustomConsole.WriteDebugHeader("Start Typing!");
            isTyping = true;
            AssignAllKeys();
        }

        /// <summary>
        /// Finishes the typing sequence for custom text.
        /// </summary>
        private static void StopTyping()
        {
            CustomConsole.WriteDebugHeader("Stop Typing!");
            isTyping = false;
            ResetKeyAssignment();
        }

        /// <summary>
        /// Clears the currently typed in text.
        /// </summary>
        private static void ClearTyping()
        {
            CustomConsole.WriteDebugHeader("Clear The Text!");
            currentVisibleTextBuilder.Remove(0, currentVisibleTextBuilder.Length);
        }

        /// <summary>
        /// Exits the application.
        /// </summary>
        private static void ApplicationExit()
        {
            CustomConsole.WriteDebugHeader("Goodbye!");
            Application.Exit();
        }

        /// <summary>
        /// Converts the typed in characters to their unicode representation.
        /// </summary>
        private static string GetCharactersFromKeys(Keys keys, bool shift)
        {
            // Buffer for strings.
            StringBuilder stringBuffer = new StringBuilder(256);

            // Represents the current keyboard state, the buttons that are currently pressed.
            byte[] keyboardState = new byte[256];

            // If the shift button is held, register the key in the keyboard state array.
            if (shift) { keyboardState[(int)Keys.ShiftKey] = 0xff; }
            
            // Convert the pressed key to Unicode and export to the buffer.
            ToUnicode((uint)keys, 0, keyboardState, stringBuffer, 256, 0);

            // Return the typed in character.
            return stringBuffer.ToString();
        }

        /// <summary>
        /// Translates the specified virtual-key code and keyboard state to the corresponding Unicode character or characters.
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int ToUnicode(uint virtualKeyCode, uint scanCode, byte[] keyboardState, StringBuilder receivingBuffer, int bufferSize, uint flags);
    }
}

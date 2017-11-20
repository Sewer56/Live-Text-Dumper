using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;

namespace VisualizerCurrentTime
{
    public static class HotkeyManager
    {
        /// <summary>
        /// Represents an event handler which is executed if any of the mapped keys are pressed.
        /// </summary>
        public static event EventHandler<HotKeyEventArgs> hotkeyPressed;

        // Delegates for registering and unregistering the hotkey to a window.
        delegate void RegisterHotKeyDelegate(IntPtr hwnd, int id, uint modifiers, uint key);
        delegate void UnRegisterHotKeyDelegate(IntPtr hwnd, int id);

        // Window and message window pointers.
        private static volatile MessageWindow messageWindow;
        private static volatile IntPtr messageWindowHandle;
        private static ManualResetEvent windowReadyEvent = new ManualResetEvent(false);

        /// <summary>
        /// Constructor, starts a thread which instantiates a window which listens only to messages.
        /// </summary>
        static HotkeyManager()
        {
            Thread messageLoop = new Thread(delegate () { Application.Run(new MessageWindow()); });
            messageLoop.Name = "MessageLoopThread";
            messageLoop.IsBackground = true;
            messageLoop.Start();
        }

        /// <summary>
        /// Registers a hotkey to be recognized by the Windows forms based input handler.
        /// </summary>
        public static int RegisterHotKey(Keys key, KeyModifiers modifiers)
        {
            // Wait for the window to become correctly instantiated.
            windowReadyEvent.WaitOne();

            // Increment an ID.
            int id = System.Threading.Interlocked.Increment(ref _id);

            // Invoke on the MessageWindow a registration of a new hotkey with the given key and modifier combination.
            messageWindow.Invoke(new RegisterHotKeyDelegate(RegisterHotKeyInternal), messageWindowHandle, id, (uint)modifiers, (uint)key);

            // Add to the list of registered hotkeys.
            Program.hotkeyIDs.Add(id);

            // Return.
            return id;
        }

        /// <summary>
        /// Unregisters a mapped hotkey.
        /// </summary>
        /// <param name="id"></param>
        public static void UnregisterHotKey(int id) { messageWindow.Invoke(new UnRegisterHotKeyDelegate(UnRegisterHotKeyInternal), messageWindowHandle, id); }

        /// <summary>
        /// Internal method responsible for the registering of a hotkey.
        /// </summary>
        private static void RegisterHotKeyInternal(IntPtr hwnd, int id, uint modifiers, uint key) { RegisterHotKey(hwnd, id, modifiers, key); }

        /// <summary>
        /// Internal method responsible for the unregistering of a hotkey.
        /// </summary>
        private static void UnRegisterHotKeyInternal(IntPtr hwnd, int id) { UnregisterHotKey(messageWindowHandle, id); }

        /// <summary>
        /// Runs the eventhandler when a key is pressed.
        /// </summary>
        private static void OnHotKeyPressed(HotKeyEventArgs e) { hotkeyPressed(null, e); }

        /// <summary>
        /// Defines a message only window, receives messages but does not display anything graphically.
        /// </summary>
        private class MessageWindow : Form
        {
            /// <summary>
            /// Constant representing a hotkey.
            /// </summary>
            private const int WM_HOTKEY = 0x312;

            /// <summary>
            /// Constructor, instantiates the message window.
            /// </summary>
            public MessageWindow()
            {
                // "this" refers to the HotkeyManager.
                messageWindow = this;
                
                // Assign the handle of the message only window.
                messageWindowHandle = this.Handle;

                // Set the event, allowing for actions based upon the initialization of the window to be performed.
                windowReadyEvent.Set();
            }

            /// <summary>
            /// Override the method responsible for processing of the messages that are sent to a window.
            /// </summary>
            protected override void WndProc(ref Message m)
            {
                // If the message is a hotkey, create the arguments to the message and pass onto the HotkeyManager.
                if (m.Msg == WM_HOTKEY)
                {
                    HotKeyEventArgs e = new HotKeyEventArgs(m.LParam);
                    OnHotKeyPressed(e);
                }

                base.WndProc(ref m);
            }

            /// <summary>
            /// Ensures that the window is never displayed onto the screens.
            /// </summary>
            protected override void SetVisibleCore(bool value) {  base.SetVisibleCore(false); }
        }

        ////////////////////
        /// P/Invoke Imports
        ////////////////////
        [DllImport("user32", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private static int _id = 0;
    }


    public class HotKeyEventArgs : EventArgs
    {
        public readonly Keys Key;
        public readonly KeyModifiers Modifiers;

        public HotKeyEventArgs(Keys key, KeyModifiers modifiers)
        {
            this.Key = key;
            this.Modifiers = modifiers;
        }

        public HotKeyEventArgs(IntPtr hotKeyParam)
        {
            uint param = (uint)hotKeyParam.ToInt64();
            Key = (Keys)((param & 0xffff0000) >> 16);
            Modifiers = (KeyModifiers)(param & 0x0000ffff);
        }
    }

    [Flags]
    public enum KeyModifiers
    {
        Alt = 1,
        Control = 2,
        Shift = 4,
        Windows = 8,
        NoRepeat = 0x4000
    }
}

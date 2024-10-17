namespace KeyLogger.View;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using KeyLogger.Core;
using KeyLogger.Core.Extensions;
using KeyLogger.Option;

using FillColor = System.Windows.Media.Color;
using NotifyIcon = System.Windows.Forms.NotifyIcon;
using Keys = System.Windows.Forms.Keys;
using ContextMenuStrip = System.Windows.Forms.ContextMenuStrip;
using Thread = System.Threading.Thread;

/// <summary>
/// Interaction logic for MainWindow.xaml.
/// </summary>
public sealed partial class MainWindow : Window, IDisposable, INotifyPropertyChanged
{
    private static readonly SolidColorBrush WhiteBrush = new(Colors.White);
    private static readonly SolidColorBrush GrayBrush = new(FillColor.FromRgb(136, 136, 136));
    private static readonly List<string> ModifierStringRepresentations = ["⇧", "⌃", "⌥", "⌘"];

    private readonly User32.LowLevelHook callback;
    private readonly IntPtr hookId = IntPtr.Zero;
    private readonly NotifyIcon notifyIcon;

    #region Configurables
    private readonly int timerMax;
    private readonly int timerTick;
    private readonly FixedSizedQueue<string> queue;
    private readonly Timer idleTimer;
    #endregion

    #region States
    private int timerCountdown;
    private bool isQueueCleared = false;
    private bool isDisposed = false;
    private bool isShiftPressed = false;
    private bool isCtrlPressed = false;
    private bool isAltPressed = false;
    private bool isWinPressed = false;
    private string keyStrokeDisplay = string.Empty;
    private bool isPaused;
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    /// <param name="settings">The default settings.</param>
    public MainWindow(DefaultSettings settings)
    {
        Guard.ThrowIfNull(settings);

        this.timerMax = settings.IdleTimedOutInMilliseconds;
        this.timerTick = settings.TimerTickInMilliseconds;
        this.queue = new(settings.MaximumKeystrokeDisplayCount);
        this.idleTimer = new(settings.TimerTickInMilliseconds);

        this.DataContext = this;
        this.Topmost = true;

        this.callback = this.HookCallback;
        this.hookId = SetHook(this.callback);

        this.idleTimer.Elapsed += this.OnTimedEvent!;
        this.timerCountdown = this.timerMax;

        var contextMenuStrip = new ContextMenuStrip();
        contextMenuStrip.Items.Add("Minimize", null, this.OnClickToolStripMinimize!);
        contextMenuStrip.Items.Add("About", null, this.OnClickToolStripMinimize!);
        contextMenuStrip.Items.Add("Pause", null, this.OnClickToolStripPause!);
        contextMenuStrip.Items.Add("Exit", null, this.OnClickToolStripExit!);

        this.notifyIcon = new NotifyIcon
        {
            BalloonTipText = "The item has been minimized.",
            Text = "KeyLogger",
            Icon = new System.Drawing.Icon("Assets/icon.ico"),
            Visible = true,
            ContextMenuStrip = contextMenuStrip,
        };
        this.notifyIcon.Click += new EventHandler(this.OnClickNotifyIcon!);

        this.InitializeComponent();

        Task.Delay(settings.StartupDelayInMilliseconds).GetAwaiter().GetResult();
        this.idleTimer.Start();
    }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets the key stroke to be displayed on the text box.
    /// </summary>
    public string KeyStrokeDisplay
    {
        get => this.keyStrokeDisplay;

        set
        {
            this.keyStrokeDisplay = value;

            var args = new PropertyChangedEventArgs(nameof(this.KeyStrokeDisplay));
            this.PropertyChanged?.Invoke(this, args);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!this.isDisposed)
        {
            User32.UnhookWindowsHookEx(this.hookId);
            this.queue.Dispose();
            this.notifyIcon?.Dispose();
            this.isDisposed = true;
        }
    }

    private static IntPtr SetHook(User32.LowLevelHook proc)
    {
        using var curProcess = Process.GetCurrentProcess();
        using var curModule = curProcess.MainModule;

        return User32.SetWindowsHookEx(13, proc, User32.GetModuleHandle(curModule!.ModuleName), 0);
    }

    private void OnClickToolStripMinimize(object sender, EventArgs e)
    {
        this.Hide();
        this.WindowState = WindowState.Minimized;
    }

    private void OnClickNotifyIcon(object sender, EventArgs e)
    {
        this.Show();
        this.WindowState = WindowState.Normal;
    }

    private void OnClickToolStripExit(object sender, EventArgs e)
    {
        this.Close();
    }

    private void OnClickToolStripPause(object sender, EventArgs e)
    {
        this.isPaused = !this.isPaused;
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (this.isPaused)
        {
            return User32.CallNextHookEx(this.hookId, nCode, wParam, lParam);
        }

        if (nCode >= 0 && (wParam == (IntPtr)0x0100 || wParam == (IntPtr)0x0104)) // WM_KEYDOWN message
        {
            bool capsLockActive = false;

            var shiftKeyState = User32.GetAsyncKeyState(Keys.ShiftKey);
            if (shiftKeyState.FirstBitIsTurnedOn())
            {
                this.isShiftPressed = true;
                this.UpdateShiftUI();
            }

            var ctrlKeyState = User32.GetAsyncKeyState(Keys.ControlKey);
            if (ctrlKeyState.FirstBitIsTurnedOn())
            {
                this.isCtrlPressed = true;
                this.UpdateCtrlUI();
            }

            var altKeyState = User32.GetAsyncKeyState(Keys.LMenu);
            if (altKeyState.FirstBitIsTurnedOn())
            {
                this.isAltPressed = true;
                this.UpdateAltUI();
            }

            var winKeyState = User32.GetAsyncKeyState(Keys.LWin);
            if (winKeyState.FirstBitIsTurnedOn())
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

            var currentKeyPress = new KeyPress((Keys)vkCode, this.isShiftPressed, capsLockActive).ToString();

            if (!currentKeyPress.Equals(this.queue.PeekLast)
                || !ModifierStringRepresentations.Contains(currentKeyPress))
            {
                this.queue.Enqueue(currentKeyPress);
                this.KeyStrokeDisplay = string.Join(string.Empty, this.queue.GetAll);
            }

            this.isQueueCleared = false;

            this.timerCountdown = this.timerMax;
        }
        else if (nCode >= 0 && (wParam == (IntPtr)0x0101 || wParam == (IntPtr)0x0104)) // WM_KEYUP message
        {
            if (this.isShiftPressed)
            {
                var shiftKeyState = User32.GetAsyncKeyState(Keys.ShiftKey);
                if (!shiftKeyState.FirstBitIsTurnedOn())
                {
                    this.isShiftPressed = false;
                    this.UpdateShiftUI();
                }
            }

            if (this.isCtrlPressed)
            {
                var ctrlKeyState = User32.GetAsyncKeyState(Keys.ControlKey);
                if (!ctrlKeyState.FirstBitIsTurnedOn())
                {
                    this.isCtrlPressed = false;
                    this.UpdateCtrlUI();
                }
            }

            if (this.isAltPressed)
            {
                var altKeyState = User32.GetAsyncKeyState(Keys.LMenu);
                if (!altKeyState.FirstBitIsTurnedOn())
                {
                    this.isAltPressed = false;
                    this.UpdateAltUI();
                }
            }

            if (this.isWinPressed)
            {
                var winKeyState = User32.GetAsyncKeyState(Keys.LWin);
                if (!winKeyState.FirstBitIsTurnedOn())
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

    private void OnTimedEvent(object source, ElapsedEventArgs e)
    {
        if (this.timerCountdown > 0)
        {
            this.timerCountdown -= this.timerTick;
        }
        else
        {
            if (!this.isQueueCleared)
            {
                this.queue.Clear();
                this.isQueueCleared = true;
                this.KeyStrokeDisplay = string.Empty;
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

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            this.DragMove();
        }
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {
        this.Dispose();
    }

    private void Window_StateChanged(object sender, EventArgs e)
    {
        if (this.WindowState == WindowState.Minimized)
        {
            this.Hide();
            this.notifyIcon.ShowBalloonTip(2000);
        }
    }
}

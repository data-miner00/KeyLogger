namespace KeyLogger;

using Autofac;
using KeyLogger.View;
using System;
using System.Windows;

/// <summary>
/// Interaction logic for App.xaml.
/// </summary>
public sealed partial class App : Application, IDisposable
{
    private readonly IContainer container = ContainerConfig.Configure();

    private bool isDisposed;

    /// <inheritdoc/>
    public void Dispose()
    {
        if (this.isDisposed)
        {
            return;
        }

        this.container.Dispose();
        this.isDisposed = true;
    }

    /// <inheritdoc/>
    protected override void OnStartup(StartupEventArgs e)
    {
        var mainWindow = this.container.Resolve<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    /// <inheritdoc/>
    protected override void OnExit(ExitEventArgs e)
    {
        this.Dispose();

        base.OnExit(e);
    }
}

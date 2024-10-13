namespace KeyLogger;

using Autofac;
using KeyLogger.View;

/// <summary>
/// IoC container configuration definition.
/// </summary>
internal static class ContainerConfig
{
    /// <summary>
    /// Configures the IoC container.
    /// </summary>
    /// <returns>The configured IoC container.</returns>
    public static IContainer Configure()
    {
        var builder = new ContainerBuilder();

        builder.RegisterType<MainWindow>().SingleInstance();

        return builder.Build();
    }
}

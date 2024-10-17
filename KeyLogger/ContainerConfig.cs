namespace KeyLogger;

using Autofac;
using KeyLogger.Option;
using KeyLogger.View;
using Microsoft.Extensions.Configuration;
using System;

/// <summary>
/// IoC container configuration definition.
/// </summary>
internal static class ContainerConfig
{
    private const string JsonFileName = "settings.json";

    /// <summary>
    /// Configures the IoC container.
    /// </summary>
    /// <returns>The configured IoC container.</returns>
    public static IContainer Configure()
    {
        var builder = new ContainerBuilder();

        builder.RegisterSettingsFile();

        builder.RegisterType<MainWindow>().SingleInstance();
        builder.RegisterType<Lazy<HelpWindow>>();
        builder.Register<Func<HelpWindow>>(ctx => () => new HelpWindow());

        return builder.Build();
    }

    private static ContainerBuilder RegisterSettingsFile(this ContainerBuilder builder)
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile(JsonFileName);

        var config = configBuilder.Build();

        var defaultSettings = config.GetSection(nameof(DefaultSettings)).Get<DefaultSettings>()
            ?? throw new InvalidOperationException("The settings file is missing.");

        builder.RegisterInstance(defaultSettings);

        return builder;
    }
}

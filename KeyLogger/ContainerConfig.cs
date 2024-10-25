namespace KeyLogger;

using Autofac;
using KeyLogger.Option;
using KeyLogger.View;
using Microsoft.Extensions.Configuration;
using System;
using Serilog;

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

        builder
            .RegisterSettingsFile()
            .RegisterLogging()
            .RegisterWindows();

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
        builder.RegisterInstance(config);

        return builder;
    }

    private static ContainerBuilder RegisterWindows(this ContainerBuilder builder)
    {
        builder.RegisterType<MainWindow>().SingleInstance();
        builder.RegisterType<Lazy<HelpWindow>>();
        builder.Register<Func<HelpWindow>>(ctx => () => new HelpWindow());

        return builder;
    }

    private static ContainerBuilder RegisterLogging(this ContainerBuilder builder)
    {
        builder.Register(ctx =>
        {
            var configuration = ctx.Resolve<IConfigurationRoot>();

            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            return logger;
        }).As<ILogger>();

        return builder;
    }
}

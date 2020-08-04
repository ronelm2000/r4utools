using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Lamar;
using Microsoft.Extensions.DependencyInjection;
using Montage.RebirthForYou.Tools.GUI.ModelViews;
using ReactiveUI;
using Serilog;
using Serilog.Events;
using System;

namespace Montage.RebirthForYou.Tools.GUI
{
    public class App : Application
    {
        private ILogger _logger = (Serilog.Log.Logger = BootstrapLogging().CreateLogger());
        private IContainer _container = BootstrapIOC();

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = _container.GetInstance<MainWindow>();
            }

            base.OnFrameworkInitializationCompleted();
        }

        private static IContainer BootstrapIOC()
        {
            return new Container(x =>
            {
                //x.AddLogging(l => l.AddSerilog(Serilog.Log.Logger, dispose: true));
                x.AddSingleton<ILogger>(Serilog.Log.Logger);
                x.Scan(s =>
                {
                    s.AssemblyContainingType<Montage.RebirthForYou.Tools.GUI.Program>();
                    s.AssemblyContainingType<Montage.RebirthForYou.Tools.CLI.Program>();
                    s.WithDefaultConventions();
                    s.RegisterConcreteTypesAgainstTheFirstInterface();
                });

            });
        }
        public static LoggerConfiguration BootstrapLogging()
        {
            return new LoggerConfiguration().MinimumLevel.Is(LogEventLevel.Debug)
                            .WriteTo.Debug(
                                restrictedToMinimumLevel: LogEventLevel.Debug,
                                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext:l}] {Message}{NewLine}{Exception}"
                            )
                            .WriteTo.File(
                                "./r4utools.out.log",
                                restrictedToMinimumLevel: LogEventLevel.Information,
                                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext:l}] {Message}{NewLine}{Exception}"
                                );
        }
    }
}

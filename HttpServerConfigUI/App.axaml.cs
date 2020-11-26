using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HttpServerConfigUI.ViewModels;
using HttpServerConfigUI.Views;
using Vlix.HttpServer;

namespace HttpServerConfigUI
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new HttpServerVM(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}

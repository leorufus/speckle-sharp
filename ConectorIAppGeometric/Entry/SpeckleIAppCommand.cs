using Avalonia;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using DesktopUI2;
using DesktopUI2.ViewModels;
using DesktopUI2.Views;
using IA.Model;

namespace Speckle.ConnectorIAppGeometric
{
    public class SpeckleIAppCommand
    {
        public static Window MainWindow { get; private set; }
        public static ConnectorBindingsIAppGeometric Bindings { get; set; } 

        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
      .UsePlatformDetect()
      .With(new SkiaOptions { MaxGpuResourceSizeBytes = 8096000 })
      .With(new Win32PlatformOptions { AllowEglInitialization = true, EnableMultitouch = false })
      .LogToTrace()
      .UseReactiveUI();

        public static void StartOrShowPanel(Project project)
        {
            Bindings = new ConnectorBindingsIAppGeometric(project);

            if (MainWindow == null)
            {
                BuildAvaloniaApp().Start(AppMain, null);
            }

            MainWindow.Show();
        }

        private static void AppMain(Application app, string[] args)
        {
            var viewModel = new MainWindowViewModel(Bindings);
            MainWindow = new MainWindow
            {
                DataContext = viewModel
            };

            app.Run(MainWindow);
        }
    }
}

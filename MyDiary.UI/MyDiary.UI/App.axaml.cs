using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using MyDiary.UI.ViewModels;
using MyDiary.UI.Views;
using System;

namespace MyDiary.UI
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            //if(OperatingSystem.IsWindows())
            //{
                //Resources.Add("ContentControlThemeFontFamily", new FontFamily("Î¢ÈíÑÅºÚ"));
            //}
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainViewVM()
                };
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            {
                singleViewPlatform.MainView = new MainView
                {
                    DataContext = new MainViewVM()
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
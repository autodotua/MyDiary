using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;
using Mapster;
using MyDiary.Models;
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
            if (OperatingSystem.IsWindows())
            {
                Resources.Add("ContentControlThemeFontFamily", new FontFamily("Microsoft YaHei"));
            }
            else if (OperatingSystem.IsBrowser())
            {
                Resources.Add("ContentControlThemeFontFamily", new FontFamily("avares://MyDiary.UI/Assets#Microsoft YaHei"));
            }

            TypeAdapterConfig.GlobalSettings.NewConfig<TextElementInfo, TextElement>()
                 .Map(dest => dest.TextColor,
                 src => System.Drawing.Color.FromArgb(src.TextColor.A, src.TextColor.R, src.TextColor.G, src.TextColor.B));
            TypeAdapterConfig.GlobalSettings.NewConfig<TextElement, TextElementInfo>()
                .Map(dest => dest.TextColor,
                src => Avalonia.Media.Color.FromArgb(src.TextColor.A, src.TextColor.R, src.TextColor.G, src.TextColor.B));
            TypeAdapterConfig.GlobalSettings.NewConfig<TableCellInfo, TableCell>()
                 .Map(dest => dest.TextColor,
                 src => System.Drawing.Color.FromArgb(src.TextColor.A, src.TextColor.R, src.TextColor.G, src.TextColor.B));
            TypeAdapterConfig.GlobalSettings.NewConfig<TableCell, TableCellInfo>()
                .Map(dest => dest.TextColor,
                src => Avalonia.Media.Color.FromArgb(src.TextColor.A, src.TextColor.R, src.TextColor.G, src.TextColor.B));
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
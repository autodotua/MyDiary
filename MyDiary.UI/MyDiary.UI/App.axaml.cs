using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using MyDiary.Managers.Services;
using MyDiary.Models;
using MyDiary.UI.ViewModels;
using MyDiary.UI.Views;
using MyDiary.WordParser;
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
                Resources.Add("ContentControlThemeFontFamily", new FontFamily("Microsoft YaHei UI"));
            }
            else if (OperatingSystem.IsBrowser())
            {
                Resources.Add("ContentControlThemeFontFamily", new FontFamily("avares://MyDiary.UI/Assets#Microsoft YaHei"));
            }
        }
        public static ServiceProvider ServiceProvider { get; private set; }
        private void InitializeServices()
        {
            ServiceCollection services = new ServiceCollection();
            services.AddDbContext<DiaryDbContext>();
            services.AddTransient<BinaryManager>();
            services.AddTransient<DocumentManager>();
            services.AddTransient<PresetStyleManager>();
            services.AddTransient<TagManager>();
            services.AddTransient<WordReader>();
            if (OperatingSystem.IsBrowser())
            {
                services.AddSingleton<IDataProvider, WebDataProvider>();
            }
            else
            {
                services.AddSingleton<IDataProvider, LocalDataProvider>();
            }

            ServiceProvider = services.BuildServiceProvider();
        }

        private static void InitializeMapster()
        {
            TypeAdapterConfig.GlobalSettings.NewConfig<TextElementInfo, TextParagraph>()
                 .Map(dest => dest.TextColor,
                 src => System.Drawing.Color.FromArgb(src.TextColor.A, src.TextColor.R, src.TextColor.G, src.TextColor.B));
            TypeAdapterConfig.GlobalSettings.NewConfig<TextParagraph, TextElementInfo>()
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
            InitializeMapster();
            InitializeServices();

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
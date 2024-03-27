using Avalonia;
using Avalonia.Controls;
using Avalonia.Rendering;
using Avalonia.Threading;
using MyDiary.Models;
using MyDiary.WordParser;
using MyDiary.Managers.Services;
using MyDiary.UI.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace MyDiary.UI.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
        }


        private async void UserControl_Loaded(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
#if DEBUG
            await WordReader.TestAsync();
#endif
            datePicker.SelectedDate = NullableDate.Today;
        }

        private void Button_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo()
            {
                UseShellExecute = true,
                FileName = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)
            });
        }
    }
}
using Avalonia.Controls;
using MyDiary.Models;
using MyDiary.WordParser;
using System.Diagnostics;
using System.IO;

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
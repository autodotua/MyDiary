using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.Threading.Tasks;

namespace MyDiary.UI.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
            Task.Delay(2000).ContinueWith(t =>
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    datePicker.SelectedDate = DateTime.Now.AddDays(-1000);
                });


            });
        }
    }
}
using Avalonia;
using Avalonia.Controls;
using Avalonia.Rendering;
using Avalonia.Threading;
using MyDiary.Core.Services;
using MyDiary.UI.ViewModels;
using System;
using System.Threading.Tasks;

namespace MyDiary.UI.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
        }


        private void UserControl_Loaded(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            datePicker.SelectedDate = DateTime.Today;
        }
    }
}
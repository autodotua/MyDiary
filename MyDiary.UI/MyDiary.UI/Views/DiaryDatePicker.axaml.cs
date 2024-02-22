using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using MyDiary.UI.ViewModels;
using System;

namespace MyDiary.UI.Views;

public partial class DiaryDatePicker : UserControl
{
    private bool isChangingSelectedDate = false;
    public static readonly StyledProperty<DateTime?> SelectedDateProperty
        = AvaloniaProperty.Register<DiaryDatePicker, DateTime?>(nameof(SelectedDate));

    public DateTime? SelectedDate
    {
        get => GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }
    private DiaryDatePickerVM viewModel = new DiaryDatePickerVM();
    public DiaryDatePicker()
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public event EventHandler<AvaloniaPropertyChangedEventArgs> SelectedDateChanged;

    private bool pauseSelectedDatePropertyChangedProcess = false;

    private void Button_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SelectedDate = DateTime.Today - TimeSpan.FromDays(1000);
    }
}
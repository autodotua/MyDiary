using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using MyDiary.UI.ViewModels;
using System;

namespace MyDiary.UI.Views;

public partial class DiaryDatePicker : UserControl
{
    public static readonly StyledProperty<DateTime?> SelectedDateProperty
        = AvaloniaProperty.Register<DiaryDatePicker, DateTime?>(nameof(SelectedDate));

    private DiaryDatePickerVM viewModel = new DiaryDatePickerVM();

    public DiaryDatePicker()
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public event EventHandler<AvaloniaPropertyChangedEventArgs> SelectedDateChanged;

    public DateTime? SelectedDate
    {
        get => GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }
}
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using MyDiary.Core.Models;
using MyDiary.UI.ViewModels;
using System;

namespace MyDiary.UI.Views;

public partial class DiaryDatePicker : UserControl
{
    public static readonly StyledProperty<NullableDate> SelectedDateProperty
        = AvaloniaProperty.Register<DiaryDatePicker, NullableDate>(nameof(SelectedDate));

    private DiaryDatePickerVM viewModel = new DiaryDatePickerVM();

    public DiaryDatePicker()
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public event EventHandler<AvaloniaPropertyChangedEventArgs> SelectedDateChanged;

    public NullableDate SelectedDate
    {
        get => GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }
}
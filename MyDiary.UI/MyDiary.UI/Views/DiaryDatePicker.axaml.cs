using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MyDiary.UI.ViewModels;
using System;

namespace MyDiary.UI.Views;

public partial class DiaryDatePicker : UserControl
{
    private bool isChangingSelectedDate = false;
    public static readonly StyledProperty<DateTime?> SelectedDateProperty
        = AvaloniaProperty.Register<DiaryDatePicker, DateTime?>(nameof(SelectedDate), DateTime.Today);

    public DateTime? SelectedDate
    {
        get => GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }
    private DiaryDatePickerVM viewModel = new DiaryDatePickerVM();
    public DiaryDatePicker()
    {
        DataContext = viewModel;
        viewModel.PropertyChanged += ViewModel_PropertyChanged;
        InitializeComponent();
    }

    private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (isChangingSelectedDate)
        {
            return;
        }
        isChangingSelectedDate = true;
        SelectedDate = viewModel.Day.HasValue ? new DateTime(viewModel.Year, viewModel.Month, viewModel.Day.Value) : null;
        isChangingSelectedDate = false;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if(change.Property==SelectedDateProperty)
        {
            if (isChangingSelectedDate)
            {
                return;
            }
            isChangingSelectedDate = true;
            if (SelectedDate.HasValue)
            {
                viewModel.Year = SelectedDate.Value.Year;
                viewModel.Month = SelectedDate.Value.Month;
                viewModel.Day = SelectedDate.Value.Day;
            }
            else
            {
                viewModel.Day = null;
            }
            isChangingSelectedDate = false;
        }
    }
}
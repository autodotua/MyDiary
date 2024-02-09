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
        = AvaloniaProperty.Register<DiaryDatePicker, DateTime?>(nameof(SelectedDate), DateTime.Today,
            coerce: (s, v) =>
            {
                var obj = s as DiaryDatePicker;
                if (obj.isChangingSelectedDate)
                {
                    return v;
                }
                obj.isChangingSelectedDate = true;
                if (v.HasValue)
                {
                    obj.viewModel.Year = v.Value.Year;
                    obj.viewModel.Month = v.Value.Month;
                    obj.viewModel.Day = v.Value.Day;
                }
                else
                {
                    obj.viewModel.Day = null;
                }
                obj.isChangingSelectedDate = false;
                return v;
            });

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
}
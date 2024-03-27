using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyDiary.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace MyDiary.UI.ViewModels
{
    public partial class DiaryDatePickerVM : ViewModelBase
    {
        public DiaryDatePickerVM()
        {
            OnPropertyChanged(nameof(Month));
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Date))]
        private int year = DateTime.Today.Year;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Date))]
        private int? month = DateTime.Today.Month;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Date))]
        private int? day = DateTime.Today.Day;

        public NullableDate Date
        {
            get => new NullableDate(Year, Month, Day);
            set
            {
                Year = value.Year;
                Month = value.Month;
                Day = value.Day;
            }
        }

        public List<int> Years { get; } = Enumerable.Range(2000, DateTime.Today.Year - 2000 + 1).ToList();
        public List<int> Months { get; } = Enumerable.Range(1, 12).ToList();
        public ObservableCollection<int> Days { get; } = new ObservableCollection<int>();

        [RelayCommand]
        private void WholeYearButtonClick()
        {
            Month = null;
        }

        [RelayCommand]
        private void WholeMonthButtonClick()
        {
            Day = null;
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            switch (e.PropertyName)
            {
                case nameof(Year):
                case nameof(Month):
                    if (Month.HasValue)
                    {
                        Days.Clear();
                        foreach (var i in Enumerable.Range(1, DateTime.DaysInMonth(Year, Month.Value)))
                        {
                            Days.Add(i);
                        }
                    }
                    else
                    {
                        Day = null;
                    }
                    break;
            }
        }
    }
}
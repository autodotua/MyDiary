using CommunityToolkit.Mvvm.ComponentModel;
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
        private int year = DateTime.Today.Year;
        [ObservableProperty]
        private int month = DateTime.Today.Month;
        [ObservableProperty]
        private int? day = DateTime.Today.Day;

        public List<int> Years { get; } = Enumerable.Range(2000, DateTime.Today.Year - 2000 + 1).ToList();
        public List<int> Months { get; } = Enumerable.Range(1, 12).ToList();
        public ObservableCollection<int> Days { get; } = new ObservableCollection<int>();

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            switch (e.PropertyName)
            {
                case nameof(Year):
                case nameof(Month):
                    Days.Clear();
                    foreach (var i in Enumerable.Range(1, DateTime.DaysInMonth(Year, Month)))
                    {
                        Days.Add(i);
                    }
                    break;
            }
        }
    }
}

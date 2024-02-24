using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using MyDiary.UI.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace MyDiary.UI.ViewModels
{
    public partial class DiaryPadVM : ViewModelBase
    {
        public DiaryPadVM()
        {
        }

        [ObservableProperty]
        private string title;
        [ObservableProperty]
        private ObservableCollection<string> tags;
        [ObservableProperty]
        private string selectedTag;
    }
}

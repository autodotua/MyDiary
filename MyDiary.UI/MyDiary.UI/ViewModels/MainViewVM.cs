using CommunityToolkit.Mvvm.ComponentModel;
using MyDiary.Models;
using MyDiary.Managers.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MyDiary.UI.ViewModels
{
    public partial class MainViewVM : ViewModelBase
    {
        [ObservableProperty]
        private DateTime? date;
        [ObservableProperty]
        private bool isLoading;
        [ObservableProperty]
        private IList<Block> document;

    }
}

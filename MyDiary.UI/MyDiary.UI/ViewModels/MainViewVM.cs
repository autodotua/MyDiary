using CommunityToolkit.Mvvm.ComponentModel;
using MyDiary.Core.Models;
using MyDiary.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MyDiary.UI.ViewModels
{
    public partial class MainViewVM : ViewModelBase
    {
        DoumentManager dataService = new DoumentManager();

        [ObservableProperty]
        private DateTime? date;
        [ObservableProperty]
        private bool isLoading;
        [ObservableProperty]
        private IList<Block> document;

    }
}

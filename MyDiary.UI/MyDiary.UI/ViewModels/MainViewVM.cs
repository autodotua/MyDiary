using CommunityToolkit.Mvvm.ComponentModel;
using MyDiary.Models;
using System;
using System.Collections.Generic;

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
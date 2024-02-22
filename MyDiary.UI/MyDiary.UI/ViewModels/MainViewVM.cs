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
        DataService dataService = new DataService();


        [ObservableProperty]
        private DateTime? date;
        [ObservableProperty]
        private bool isLoading;
        [ObservableProperty]
        private IList<DocumentPart> document;

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(Date))
            {
                IsLoading = true;
                if (Date.HasValue)
                {
                    Document = dataService.GetDocument(Date.Value);
                }
                else
                {
                    Document = null;
                }
                IsLoading = false;
            }
        }
    }
}

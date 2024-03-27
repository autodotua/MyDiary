using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

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
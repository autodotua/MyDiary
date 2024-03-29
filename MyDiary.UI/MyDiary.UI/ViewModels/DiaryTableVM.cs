using CommunityToolkit.Mvvm.ComponentModel;

namespace MyDiary.UI.ViewModels
{
    public partial class DiaryTableVM : ViewModelBase
    {
        [ObservableProperty]
        private string title;
    }
}
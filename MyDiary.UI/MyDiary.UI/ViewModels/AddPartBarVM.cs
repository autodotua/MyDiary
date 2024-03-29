using CommunityToolkit.Mvvm.ComponentModel;

namespace MyDiary.UI.ViewModels
{
    public partial class AddPartBarVM : ViewModelBase
    {
        [ObservableProperty]
        private int row = 3;

        [ObservableProperty]
        private int column = 3;
    }
}
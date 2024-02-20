using CommunityToolkit.Mvvm.ComponentModel;

namespace MyDiary.UI.ViewModels
{
    public partial class EditBarInfo : TextElementInfo
    {
        [ObservableProperty]
        private bool cellsMerged;
        [ObservableProperty]
        private bool canMergeCell;
    }
}

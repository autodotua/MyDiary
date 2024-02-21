using CommunityToolkit.Mvvm.ComponentModel;

namespace MyDiary.UI.ViewModels
{
    public partial class EditBarInfo : TextElementInfo
    {
        [ObservableProperty]
        private bool cellsMerged;
        [ObservableProperty]
        private bool canMergeCell;

        //这一部分修改一下，多一个TextElementInfo[]，如果表格多选就多传几个进来。然后再来个TextElementInfo用于绑定。
    }
}

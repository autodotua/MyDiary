using Avalonia.Input;
using MyDiary.UI.ViewModels;

namespace MyDiary.UI.Views.DiaryDocElement;

public partial class DiaryTextBox : DiaryTextBoxBase, IDiaryElement
{
    public DiaryTextBox()
    {
        TextData = new TextElementInfo();
        InitializeComponent();
    }

    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);
        RaiseEditBarVMUpdated();
    }

    public override EditBarVM GetEditData()
    {
        TextData.CanBackColorChange = false;
        return new EditBarVM([TextData]);
    }
}
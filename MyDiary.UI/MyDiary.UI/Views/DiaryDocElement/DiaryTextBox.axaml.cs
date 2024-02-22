using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using MyDiary.UI.ViewModels;
using System;

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
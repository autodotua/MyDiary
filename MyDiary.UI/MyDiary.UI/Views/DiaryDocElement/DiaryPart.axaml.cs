using Avalonia.Controls;
using Avalonia.Input;

namespace MyDiary.UI.Views.DiaryDocElement;

public partial class DiaryPart : ContentControl
{
    private static int currentZIndex = 0;

    public DiaryPart()
    {
        InitializeComponent();
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        //使鼠标下面的DiaryPart永远在最上层
        ZIndex = ++currentZIndex;
    }
}
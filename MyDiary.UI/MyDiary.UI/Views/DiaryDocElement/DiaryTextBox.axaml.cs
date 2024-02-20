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
        InitializeComponent();
     
    }
    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);
        RaiseEditBarInfoUpdated();
    }

    public override EditBarInfo GetEditBarInfo()
    {
        var ep = new EditBarInfo()
        {
            CanMergeCell = false,
            Bold = FontWeight > FontWeight.Normal,
            FontSize = FontSize,
            Italic = FontStyle == FontStyle.Italic
        };

        ep.PropertyChanged += (s, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(ep.Bold):
                    FontWeight = ep.Bold ? FontWeight.Bold : FontWeight.Normal;
                    break;
                case nameof(ep.Italic):
                    FontStyle = ep.Italic ? FontStyle.Italic : FontStyle.Normal;
                    break;
                case nameof(ep.FontSize):
                    FontSize = ep.FontSize;
                    break;
                case nameof(ep.Alignment):
                    TextAlignment = ep.TextAlignment;
                    break;
            }
        };

        return ep;
    }

}
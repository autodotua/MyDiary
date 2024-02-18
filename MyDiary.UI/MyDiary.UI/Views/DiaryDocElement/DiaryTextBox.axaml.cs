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
public partial class DiaryTextBox : TextBox, IDiaryElement
{
    public DiaryTextBox()
    {
        InitializeComponent();
    }
    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        base.OnGotFocus(e);
        EditPropertiesUpdated?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler EditPropertiesUpdated;

    public EditProperties GetEditProperties()
    {
        var ep = new EditProperties()
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
            }
        };

        return ep;
    }
}
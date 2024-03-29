using Avalonia;
using Avalonia.Controls;
using MyDiary.UI.ViewModels;
using System;

namespace MyDiary.UI.Views;

public partial class EditBar : UserControl
{
    public EditBar()
    {
        DataContext = new EditBarVM([new TextElementInfo()]);
        InitializeComponent();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DataContextProperty)
        {
            if (change.OldValue is IDisposable d)
            {
                d.Dispose();
            }
        }
    }
}
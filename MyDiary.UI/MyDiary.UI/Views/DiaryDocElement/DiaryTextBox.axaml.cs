using Avalonia;
using Avalonia.Controls;
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

    public event EventHandler EditPropertiesUpdated;

    public EditProperties GetEditProperties()
    {
        throw new NotImplementedException();
    }
}
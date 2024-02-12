using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using MyDiary.UI.ViewModels;
using System;
using System.Linq;

namespace MyDiary.UI.Views.DiaryDocElement;

public partial class AddPartBar : Grid
{
    public AddPartBar()
    {
        InitializeComponent();
    }

    private void InsertTextButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        DiaryPad.GetDiaryPad(this).InsertElementAfter<DiaryTextBox>(this);
    }
    private async void InsertImageButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var files = await TopLevel.GetTopLevel(this).StorageProvider.OpenFilePickerAsync(
         new Avalonia.Platform.Storage.FilePickerOpenOptions
         {
             FileTypeFilter = new[] { FilePickerFileTypes.ImageAll }
         });
        if (files.Count > 0)
        {
            var image = DiaryPad.GetDiaryPad(this).InsertElementAfter<DiaryImage>(this);
            image.ImageSource = new Bitmap(files[0].Path.LocalPath);
        }
    }
    private void InsertTableButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
    }
}
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using MyDiary.UI.ViewModels;

namespace MyDiary.UI.Views.DiaryDocElement;

public partial class AddPartBar : Grid
{
    public AddPartBar()
    {
        DataContext = new AddPartBarVM();
        InitializeComponent();
    }

    private async void InsertTextButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
      var txt=  DiaryPad.GetDiaryPad(this).CreateAndInsertElementBelow<DiaryTextBox>(this);
        txt.LoadData(await App.ServiceProvider.GetService<IDataProvider>().GetPresetStyleByLevelAsync(0));
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
            var image = DiaryPad.GetDiaryPad(this).CreateAndInsertElementBelow<DiaryImage>(this);
            await image.LoadImageFromFileAsync(files[0].Path.LocalPath);
        }
    }

    private void CreateTableButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var table = DiaryPad.GetDiaryPad(this).CreateAndInsertElementBelow<DiaryTable>(this);
        table.MakeEmptyTable((DataContext as AddPartBarVM).Row, (DataContext as AddPartBarVM).Column);
    }
}
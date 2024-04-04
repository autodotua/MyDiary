using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using MyDiary.Managers.Services;
using MyDiary.Models;
using MyDiary.UI.ViewModels;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MyDiary.UI.Views.DiaryDocElement;

public partial class DiaryImage : Grid, IDiaryElement
{
    private DiaryImageVM viewModel = new DiaryImageVM();

    public DiaryImage()
    {
        DataContext = viewModel = new DiaryImageVM();
        InitializeComponent();
    }

    public event EventHandler NotifyEditDataUpdated;

    public Block GetData()
    {
        return new MyDiary.Models.Image()
        {
            Caption = viewModel.Title,
            DataId = viewModel.ImageDataId
        };
    }

    public EditBarVM GetEditData()
    {
        throw new NotImplementedException();
    }

    public async void LoadData(Block data)
    {
        var imageData = data as MyDiary.Models.Image;
        viewModel.Title = imageData.Caption;
        viewModel.ImageDataId = imageData.DataId;
        if (imageData.DataId.HasValue)
        {
            viewModel.ImageData = await App.ServiceProvider.GetRequiredService<IDataProvider>().GetBinaryAsync(imageData.DataId.Value);
        }
    }

    private async void ChangeSourceButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var files = await TopLevel.GetTopLevel(this).StorageProvider.OpenFilePickerAsync(
         new FilePickerOpenOptions
         {
             FileTypeFilter = [FilePickerFileTypes.ImageAll]
         });
        if (files.Count > 0)
        {
            await LoadImageFromFileAsync(files[0].Path.LocalPath);
        }
    }

    public async Task LoadImageFromFileAsync(string filePath)
    {
        viewModel.ImageData = await File.ReadAllBytesAsync(filePath);

        viewModel.ImageDataId = await App.ServiceProvider.GetRequiredService<IDataProvider>().AddBinaryAsync(viewModel.ImageData);
    }

    private void DeleteButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ((Parent as Control).Parent as StackPanel).Children.Remove(this.GetParentDiaryPart());
    }
}
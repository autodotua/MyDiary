using Avalonia.Controls;
using Avalonia.Platform.Storage;
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
        return new Models.Image()
        {
            Title = viewModel.Title,
            DataId = viewModel.ImageDataId
        };
    }

    public EditBarVM GetEditData()
    {
        throw new NotImplementedException();
    }

    public async void LoadData(Block data)
    {
        var binaryManager = new BinaryManager();
        var imageData = data as Models.Image;
        viewModel.Title = imageData.Title;
        viewModel.ImageDataId = imageData.DataId;
        if (imageData.DataId.HasValue)
        {
            viewModel.ImageData = await DataManager.Manager.GetBinaryAsync(imageData.DataId.Value);
        }
    }

    private async void ChangeSourceButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var files = await TopLevel.GetTopLevel(this).StorageProvider.OpenFilePickerAsync(
         new Avalonia.Platform.Storage.FilePickerOpenOptions
         {
             FileTypeFilter = new[] { FilePickerFileTypes.ImageAll }
         });
        if (files.Count > 0)
        {
            await LoadImageFromFileAsync(files[0].Path.LocalPath);
        }
    }

    public async Task LoadImageFromFileAsync(string filePath)
    {
        viewModel.ImageData = await File.ReadAllBytesAsync(filePath);

        viewModel.ImageDataId = await DataManager.Manager.AddBinaryAsync(viewModel.ImageData);
    }

    private void DeleteButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ((Parent as Control).Parent as StackPanel).Children.Remove(this.GetParentDiaryPart());
    }
}
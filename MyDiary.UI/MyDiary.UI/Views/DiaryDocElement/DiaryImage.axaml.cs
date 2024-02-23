using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using MyDiary.Core.Models;
using MyDiary.Core.Services;
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
        return new Core.Models.Image()
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
        var imageData = data as Core.Models.Image;
        viewModel.Title = imageData.Title;
        viewModel.ImageDataId = imageData.DataId;
        if (imageData.DataId.HasValue)
        {
            viewModel.ImageData = await binaryManager.GetBinaryAsync(imageData.DataId.Value);
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

        using var bm = new BinaryManager();
        viewModel.ImageDataId = await bm.AddBinaryAsync(viewModel.ImageData);
    }

    private void DeleteButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ((Parent as Control).Parent as StackPanel).Children.Remove(this.GetParentDiaryPart());
    }
}
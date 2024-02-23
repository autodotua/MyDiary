using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using MyDiary.Core.Models;
using MyDiary.UI.ViewModels;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MyDiary.UI.Views.DiaryDocElement;

public partial class DiaryImage : Grid, IDiaryElement
{
    //public static readonly StyledProperty<IImage> ImageSourceProperty = Avalonia.Controls.Image.SourceProperty.AddOwner<DiaryImage>(new StyledPropertyMetadata<IImage>());

    //public static readonly StyledProperty<string> TitleProperty =
    //    AvaloniaProperty.Register<DiaryImage, string>(nameof(Title), "Í¼Ãû");

    private DiaryImageVM viewModel = new DiaryImageVM();
    public DiaryImage()
    {
        DataContext = viewModel = new DiaryImageVM();
        InitializeComponent();
    }
    public event EventHandler NotifyEditDataUpdated;

    //public IImage ImageSource
    //{
    //    get => GetValue(ImageSourceProperty);
    //    set => SetValue(ImageSourceProperty, value);
    //}

    //public string Title
    //{
    //    get => this.GetValue(TitleProperty);
    //    set => SetValue(TitleProperty, value);
    //}
    public Block GetData()
    {
        return new Core.Models.Image()
        {
            Title = viewModel.Title,
            Data = viewModel.ImageData
        };
    }

    public EditBarVM GetEditData()
    {
        throw new NotImplementedException();
    }

    public void LoadData(Block data)
    {
        var imageData = data as Core.Models.Image;
        viewModel.Title = imageData.Title;
        viewModel.ImageData = imageData.Data;
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
    }

    private void DeleteButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ((Parent as Control).Parent as StackPanel).Children.Remove(this.GetParentDiaryPart());
    }
}
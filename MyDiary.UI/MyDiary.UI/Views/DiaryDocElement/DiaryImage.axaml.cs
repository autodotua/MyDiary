using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using MyDiary.UI.ViewModels;
using System;

namespace MyDiary.UI.Views.DiaryDocElement;

public partial class DiaryImage : Grid, IDiaryElement
{
    public DiaryImage()
    {
        InitializeComponent();
    }


    public static readonly StyledProperty<IImage> ImageSourceProperty = Image.SourceProperty.AddOwner<DiaryImage>(new StyledPropertyMetadata<IImage>(coerce: (s, v) =>
    {
        return v;
    }));

    public event EventHandler EditPropertiesUpdated;

    public IImage ImageSource
    {
        get => GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ImageSourceProperty)
        {
            image.Source = ImageSource;
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
            image.Source = new Bitmap(files[0].Path.LocalPath);
        }
    }

    private void DeleteButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ((Parent as Control).Parent as StackPanel).Children.Remove(this.GetParentDiaryPart());
    }

    public EditProperties GetEditProperties()
    {
        throw new NotImplementedException();
    }
}
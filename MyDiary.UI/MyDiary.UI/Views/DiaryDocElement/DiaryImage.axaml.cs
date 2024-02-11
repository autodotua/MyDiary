using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using MyDiary.UI.ViewModels;
using System;

namespace MyDiary.UI.Views.DiaryDocElement;

public partial class DiaryImage : Grid
{ 
    public DiaryImage()
    {
        InitializeComponent();
    }


    public static readonly StyledProperty<IImage> ImageSourceProperty=Image.SourceProperty.AddOwner<DiaryImage>(new StyledPropertyMetadata<IImage>(coerce: (s, v) =>
    {
        return v;
    }));

    public IImage ImageSource
    {
        get => GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if(change.Property== ImageSourceProperty)
        {
            image.Source = ImageSource;
        }
    }

    private void DeleteButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ((Parent as Control).Parent as StackPanel).Children.Remove(this.AsDiaryPart());
    }
}
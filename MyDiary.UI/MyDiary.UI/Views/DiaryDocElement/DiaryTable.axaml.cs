using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using MyDiary.UI.ViewModels;
using System;
using System.Linq.Expressions;
using Avalonia.Layout;

namespace MyDiary.UI.Views.DiaryDocElement;

public partial class DiaryTable : Grid, IDiaryElement
{
    public DiaryTable()
    {
        InitializeComponent();
    }


    private async void ChangeSourceButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        //var files = await TopLevel.GetTopLevel(this).StorageProvider.OpenFilePickerAsync(
        // new Avalonia.Platform.Storage.FilePickerOpenOptions
        // {
        //     FileTypeFilter = new[] { FilePickerFileTypes.ImageAll }
        // });

    }

    private void DeleteButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ((Parent as Control).Parent as StackPanel).Children.Remove(this.GetParentDiaryPart());
    }

    public void SetSize(int row, int column)
    {
        grd.ColumnDefinitions.Clear();
        grd.RowDefinitions.Clear();
        grd.ColumnDefinitions.Add(new ColumnDefinition(10, GridUnitType.Pixel));
        for (int j = 0; j <= column; j++)
        {
            if (j > 0)
            {
                grd.ColumnDefinitions.Add(new ColumnDefinition(64, GridUnitType.Pixel));
            }

            grd.ColumnDefinitions.Add(new ColumnDefinition(4, GridUnitType.Pixel));

            var splitter = new GridSplitter() { Background = Brushes.Transparent };
            SetRowSpan(splitter, int.MaxValue);
            SetColumn(splitter, j * 2 + 1);
            //if (j == 0 || j == column)
            //{
            //    splitter[!BackgroundProperty] = new DynamicResourceExtension("Foreground0");
            //}
            grd.Children.Add(splitter);

            var rect = new Panel()
            {
                Width = 1,
                HorizontalAlignment = j == 0 ? HorizontalAlignment.Left : (j == column ? HorizontalAlignment.Right : HorizontalAlignment.Center),
                VerticalAlignment = VerticalAlignment.Stretch
            };
            SetRow(rect, 1);
            SetRowSpan(rect, row * 2 + 1);
            SetColumn(rect, j * 2 + 1);
            rect[!BackgroundProperty] = new DynamicResourceExtension("Foreground0");
            grd.Children.Add(rect);
        }
        grd.ColumnDefinitions.Add(new ColumnDefinition(10, GridUnitType.Pixel));


        grd.RowDefinitions.Add(new RowDefinition(10, GridUnitType.Pixel));
        for (int i = 0; i <= row; i++)
        {
            if (i > 0)
            {
                grd.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Auto));
            }
            grd.RowDefinitions.Add(new RowDefinition(4, GridUnitType.Pixel));

            var splitter = new GridSplitter() { Background = Brushes.Transparent };
            SetRow(splitter, i * 2 + 1);
            //SetColumnSpan(splitter, int.MaxValue); if (i == 0 || i == row)
            //{
            //    splitter[!BackgroundProperty] = new DynamicResourceExtension("Foreground0");
            //}
            grd.Children.Add(splitter);

            var rect = new Panel()
            {
                Height = 1,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = i == 0 ? VerticalAlignment.Top : (i == row ? VerticalAlignment.Bottom : VerticalAlignment.Center),
            };
            SetRow(rect, i * 2 + 1);
            SetColumn(rect, 1);
            SetColumnSpan(rect, column * 2 + 1);
            rect[!BackgroundProperty] = new DynamicResourceExtension("Foreground0");
            grd.Children.Add(rect);
        }
        grd.RowDefinitions.Add(new RowDefinition(10, GridUnitType.Pixel));

        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < column; j++)
            {
                var txt = new DiaryTextBox()
                {
                    CornerRadius = new CornerRadius(0),
                };
                SetRow(txt, i * 2 + 2);
                SetColumn(txt, j * 2 + 2);

                grd.Children.Add(txt);
            }
        }
    }
}
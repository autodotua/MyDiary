using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using MyDiary.UI.ViewModels;
using System;

namespace MyDiary.UI.Views;

public partial class EditBar : UserControl
{
    /// <summary>
    /// EditProperties StyledProperty definition
    /// </summary>
    public static readonly StyledProperty<EditProperties> EditPropertiesProperty =
        AvaloniaProperty.Register<EditBar, EditProperties>(nameof(EditProperties), coerce: (s, n) =>
        {
            return n;
        });

    EditProperties viewModel = new EditProperties();
    public EditBar()
    {
        DataContext = viewModel;
        InitializeComponent();
    }

    /// <summary>
    /// Gets or sets the EditProperties property. This StyledProperty
    /// indicates ....
    /// </summary>
    public EditProperties EditProperties
    {
        get => GetValue(EditPropertiesProperty);
        set => SetValue(EditPropertiesProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if(change.Property==EditPropertiesProperty)
        {
            //viewModel.EditProperties = change.NewValue as EditBarVM;
        }
    }
}
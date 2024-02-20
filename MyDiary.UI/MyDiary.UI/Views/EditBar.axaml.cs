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
    public static readonly StyledProperty<EditBarInfo> EditInfoProperty =
        AvaloniaProperty.Register<EditBar, EditBarInfo>(nameof(EditInfo), coerce: (s, n) =>
        {
            return n;
        });

    EditBarVM viewModel = new EditBarVM();
    public EditBar()
    {
        DataContext = viewModel;
        InitializeComponent();
    }

    /// <summary>
    /// Gets or sets the EditProperties property. This StyledProperty
    /// indicates ....
    /// </summary>
    public EditBarInfo EditInfo
    {
        get => GetValue(EditInfoProperty);
        set => SetValue(EditInfoProperty, value);
    }
}
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace MyDiary.UI.ViewModels
{
    public partial class TextElementInfo : ViewModelBase
    {
        public TextElementInfo()
        {
        }

        [ObservableProperty]
        private string text;
        [ObservableProperty]
        private double fontSize = 14;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FontWeight))]
        private bool bold;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FontStyle))]
        private bool italic;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TextAlignment))]
        private int alignment;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Foreground))]
        private Color textColor = Colors.White;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Foreground))]
        private bool useDefaultTextColor = true;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Background))]
        private Color backColor = Colors.Black;
        [ObservableProperty]
        private bool canBackColorChange = true;

        public IBrush Foreground
        {
            get
            {
                if (UseDefaultTextColor)
                {
                    return Application.Current.FindResource("Foreground0") as IBrush;
                }
                else
                {
                    return new ImmutableSolidColorBrush(TextColor);
                }
            }
        }
        public SolidColorBrush Background
        {
            get => new SolidColorBrush(BackColor);
        }
        public FontStyle FontStyle
        {
            get => Italic ? FontStyle.Italic : FontStyle.Normal;
        }
        public FontWeight FontWeight
        {
            get => Bold ? FontWeight.Bold : FontWeight.Normal;
        }
        public TextAlignment TextAlignment
        {
            get => Alignment switch
            {
                0 => TextAlignment.Left,
                1 => TextAlignment.Center,
                2 => TextAlignment.Right,
                _ => throw new NotImplementedException()
            };
        }
    }
}

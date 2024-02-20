using Avalonia.Controls.Primitives;
using Avalonia.Media;
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

        public FontStyle FontStyle
        {
            get => Italic ? FontStyle.Italic : FontStyle.Normal;
            set => Italic = value == FontStyle.Italic;
        }
        public FontWeight FontWeight
        {
            get => Bold ? FontWeight.Bold : FontWeight.Normal;
            set => Bold = value > FontWeight.Normal;
        }

    }
}

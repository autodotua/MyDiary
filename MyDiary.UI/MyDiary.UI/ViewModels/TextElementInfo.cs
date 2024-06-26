﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using MyDiary.Managers.Services;
using MyDiary.Models;
using System;
using System.ComponentModel;

namespace MyDiary.UI.ViewModels
{
    public partial class TextElementInfo : ViewModelBase
    {
        //private static readonly TypeAdapterConfig mapsterConfig = new TypeAdapterConfig();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TextAlignment))]
        private int alignment;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Background))]
        private Color backColor = Colors.Black;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FontWeight))]
        private bool bold;

        [ObservableProperty]
        private bool canBackColorChange = true;

        [ObservableProperty]
        private double fontSize = 14;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(FontStyle))]
        private bool italic;

        [ObservableProperty]
        private string text;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Foreground))]
        private Color textColor = Colors.White;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Foreground))]
        private bool useDefaultTextColor = true;

        [ObservableProperty]
        private int level;

        static TextElementInfo()
        {
        }

        public TextElementInfo()
        {
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
        public static T FromModel<T>(TextParagraph model) where T : TextElementInfo
        {
            return model.Adapt<T>();
        }
        public static T FromModel<T>(TextStyle model) where T : TextElementInfo
        {
            return model.Adapt<T>();
        }

        public TextParagraph ToModel()
        {
            return this.Adapt<TextParagraph>();
        }

        protected override async void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(Level))
            {
                var preset = await App.ServiceProvider.GetRequiredService<PresetStyleManager>().GetByLevelAsync(Level);
                preset.Adapt(this);
            }
        }
    }
}
using Avalonia.Controls.Primitives;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace MyDiary.UI.ViewModels
{
    public partial class EditProperties : ViewModelBase
    {
        public EditProperties()
        {
        }

        [ObservableProperty]
        private double fontSize;
        [ObservableProperty]
        private bool bold;
        [ObservableProperty]
        private bool italic;
        [ObservableProperty]
        private bool cellsMerged;
        [ObservableProperty]
        private bool canMergeCell;
    }
}

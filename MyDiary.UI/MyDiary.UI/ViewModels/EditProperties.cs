using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDiary.UI.ViewModels
{
    public partial class EditProperties : ViewModelBase
    {
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
        [ObservableProperty]
        private bool enableBar = true;
    }
}

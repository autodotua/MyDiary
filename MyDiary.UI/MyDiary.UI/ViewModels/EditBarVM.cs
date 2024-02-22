using Avalonia.Media;
using Avalonia.Media.Fonts;
using CommunityToolkit.Mvvm.ComponentModel;
using FzLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace MyDiary.UI.ViewModels
{
    public partial class EditBarVM : ViewModelBase, IDisposable
    {
        [ObservableProperty]
        private bool cellsMerged;
        [ObservableProperty]
        private bool canMergeCell;
        [ObservableProperty]
        private IList<TextElementInfo> textDatas;
        [ObservableProperty]
        private TextElementInfo textData;

        public EditBarVM(IList<TextElementInfo> textDatas, bool canMergeCell = false, bool cellsMerged = false)
        {
            TextDatas = textDatas;
            CellsMerged = cellsMerged;
            CanMergeCell = canMergeCell;
            if (TextDatas == null)
            {
                return;
            }
            if (TextDatas.Count == 0)
            {
                TextData = null;
            }
            else
            {
                TextData = TextDatas[0];
                TextData.PropertyChanged += TextData_PropertyChanged;
            }
        }

        public void Dispose()
        {
            if (TextData != null)
            {
                TextData.PropertyChanged -= TextData_PropertyChanged;
            }
        }

        private void TextData_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Debug.Assert(TextDatas != null && TextDatas.Count > 0);
            if (TextDatas.Count == 1)
            {
                Debug.Assert(TextData == TextDatas[0]);
                return;
            }
            //TextData内属性改变时，同时通知所有TextDatas的属性
            var property = TextData.GetType().GetProperty(e.PropertyName);
            var value = property.GetValue(TextData, null);
            if (property.CanWrite)
            {
                foreach (var d in TextDatas)
                {
                    if (TextData != d)
                    {
                        property.SetValue(d, value);
                    }
                }
            }
        }
    }
}

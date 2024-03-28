using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

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

        [ObservableProperty]
        private IList<int> levels;

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
            LoadDbDataAsync().Wait();
        }

        public async Task LoadDbDataAsync()
        {
            var styles = await App.ServiceProvider.GetRequiredService<IDataProvider>().GetPresetStylesAsync();
            Levels = styles.Keys.OrderBy(p => p).ToList();
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
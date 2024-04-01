using Avalonia.Controls;
using Avalonia.Platform.Storage;
using FzLib.Avalonia.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using MyDiary.Models;
using MyDiary.UI.ViewModels;
using MyDiary.WordParser;
using System;
using System.IO;
using System.Linq;

namespace MyDiary.UI.Views.Dialogs
{
    public partial class ImportWordDialog : DialogHost
    {
        ImportWordViewModel viewModel = new ImportWordViewModel();
        public ImportWordDialog()
        {
            DataContext = viewModel;
            InitializeComponent();
        }

        protected override void OnCloseButtonClick()
        {
            Close();
        }

        protected override async void OnPrimaryButtonClick()
        {
            try
            {
                IsEnabled = false;
                CheckForms();

                var options = new WordParserOptions(viewModel.Year.Value, viewModel.Segments.Select(p => p.Content).ToList());
                WordReader wr = App.ServiceProvider.GetRequiredService<WordReader>();
                var docs = await wr.ParseAsync(viewModel.File, options);
                if (docs.Count == 0)
                {
                    throw new Exception("没有导入任何数据");
                }
                viewModel.Message = $"成功导入{docs.Count}条数据";
            }
            catch (Exception ex)
            {
                viewModel.Message = ex.Message;
            }
            finally
            {
                IsEnabled = true;
            }
        }

        private void CheckForms()
        {
            if (string.IsNullOrEmpty(viewModel.File))
            {
                throw new Exception("导入的文件地址为空");
            }
            if (!File.Exists(viewModel.File))
            {
                throw new Exception("导入的文件不存在");
            }
            if (!viewModel.Year.HasValue)
            {
                throw new Exception("年份为空");
            }
            foreach (var segment in viewModel.Segments)
            {
                var seg = segment.Content;
                if (string.IsNullOrWhiteSpace(seg.TargetTag))
                {
                    throw new Exception($"文档部分{segment.Index}的标签为空");
                }
                if (string.IsNullOrWhiteSpace(seg.TitleInDocument))
                {
                    throw new Exception($"文档部分{segment.Index}的文档中标题为空");
                }
                switch (seg.TimeUnit)
                {
                    case TimeUnit.Year:
                        break;
                    case TimeUnit.Day:
                        if (seg.DayNumberingType == NumberingType.OutlineTitle && string.IsNullOrWhiteSpace(seg.DayPattern))
                        {
                            throw new Exception($"文档部分{segment.Index}的日标题正则为空");
                        }
                        goto case TimeUnit.Month;
                    case TimeUnit.Month:
                        if (string.IsNullOrWhiteSpace(seg.MonthPattern))
                        {
                            throw new Exception($"文档部分{segment.Index}的月标题正则为空");
                        }
                        break;
                }
            }
        }

        private async void BrowseButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var files = await TopLevel.GetTopLevel(this).StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions()
            {
                AllowMultiple = false,
                FileTypeFilter = [new FilePickerFileType("Word 文档") {
                    MimeTypes=["application/msword"],
                    Patterns=["*.docx"]
                }]
            });
            if (files.Count > 0)
            {
                viewModel.File = files[0].TryGetLocalPath();
            }
        }
    }
}

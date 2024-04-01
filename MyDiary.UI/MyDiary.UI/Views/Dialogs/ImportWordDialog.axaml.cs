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
                    throw new Exception("û�е����κ�����");
                }
                viewModel.Message = $"�ɹ�����{docs.Count}������";
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
                throw new Exception("������ļ���ַΪ��");
            }
            if (!File.Exists(viewModel.File))
            {
                throw new Exception("������ļ�������");
            }
            if (!viewModel.Year.HasValue)
            {
                throw new Exception("���Ϊ��");
            }
            foreach (var segment in viewModel.Segments)
            {
                var seg = segment.Content;
                if (string.IsNullOrWhiteSpace(seg.TargetTag))
                {
                    throw new Exception($"�ĵ�����{segment.Index}�ı�ǩΪ��");
                }
                if (string.IsNullOrWhiteSpace(seg.TitleInDocument))
                {
                    throw new Exception($"�ĵ�����{segment.Index}���ĵ��б���Ϊ��");
                }
                switch (seg.TimeUnit)
                {
                    case TimeUnit.Year:
                        break;
                    case TimeUnit.Day:
                        if (seg.DayNumberingType == NumberingType.OutlineTitle && string.IsNullOrWhiteSpace(seg.DayPattern))
                        {
                            throw new Exception($"�ĵ�����{segment.Index}���ձ�������Ϊ��");
                        }
                        goto case TimeUnit.Month;
                    case TimeUnit.Month:
                        if (string.IsNullOrWhiteSpace(seg.MonthPattern))
                        {
                            throw new Exception($"�ĵ�����{segment.Index}���±�������Ϊ��");
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
                FileTypeFilter = [new FilePickerFileType("Word �ĵ�") {
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

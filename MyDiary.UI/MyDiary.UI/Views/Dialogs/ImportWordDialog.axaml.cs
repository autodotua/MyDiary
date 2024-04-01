using Avalonia.Controls;
using Avalonia.Platform.Storage;
using FzLib.Avalonia.Dialogs;
using MyDiary.UI.ViewModels;
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

        protected override void OnPrimaryButtonClick()
        {

        }

        private async void BrowseButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
           var files=await TopLevel.GetTopLevel(this).StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions()
            {
                AllowMultiple = false,
                FileTypeFilter = [new FilePickerFileType("Word ÎÄµµ") {
                    MimeTypes=["application/msword"],
                    Patterns=["*.docx"]
                }]
            });
            if(files.Count>0)
            {
                viewModel.File = files[0].TryGetLocalPath();
            }
        }
    }
}

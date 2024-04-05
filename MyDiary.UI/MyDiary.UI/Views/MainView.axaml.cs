using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using MyDiary.Managers.Services;
using MyDiary.Models;
using MyDiary.WordParser;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace MyDiary.UI.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
        }

        private
#if DEBUG
            async
#endif
            void UserControl_Loaded(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
#if DEBUG
            //var options = new WordParserOptions(2024,
            //  [new WordParserDiarySegment() {
            //        TargetTag="�ռ�",
            //        TitleInDocument="�ռ�"
            //    }]
            //);     
            await App.ServiceProvider.GetRequiredService<DocumentManager>().ClearDocumentsAsync();
            var options = new WordParserOptions(2024,
              [new WordParserDiarySegment() {
                    TargetTag="�����ܽ�",
                    TitleInDocument="�����ܽ�",
                    TimeUnit=TimeUnit.Year
                }]
            );
            var wr = App.ServiceProvider.GetRequiredService<WordReader>();
            await wr.ParseAsync(@"C:\Users\autod\Desktop\2023.docx", options);
#endif
            datePicker.SelectedDate = NullableDate.Today;
        }

        private void Button_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo()
            {
                UseShellExecute = true,
                FileName = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)
            });
        }
    }
}
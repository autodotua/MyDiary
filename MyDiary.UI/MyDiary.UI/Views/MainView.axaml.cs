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

        private async void UserControl_Loaded(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
#if DEBUG

            DocumentManager dm = App.ServiceProvider.GetRequiredService<DocumentManager>();
            await dm.ClearDocumentsAsync();
            var wr = App.ServiceProvider.GetRequiredService<WordReader>();


            //var options = new WordParserOptions(2024, [
            //        new WordParserDiarySegment(){
            //            TitleInDocument="�ռ�",
            //            TargetTag="�ռ�",
            //            TimeUnit=TimeUnit.Day,
            //            DayNumberingType=NumberingType.ParagraphNumbering,
            //            MonthPattern="(?<month>[0-1]?[0-9])��",
            //        },
            //          new WordParserDiarySegment(){
            //                    TitleInDocument="������־",
            //                    TargetTag="������־",
            //                    TimeUnit=TimeUnit.Day,
            //                    DayNumberingType=NumberingType.ParagraphNumbering,
            //            MonthPattern="(?<month>[0-1]?[0-9])��",
            //                },  new WordParserDiarySegment(){
            //                    TitleInDocument="��ְ��־",
            //                    TargetTag="��ְ��־",
            //                    TimeUnit=TimeUnit.Day,
            //                    DayNumberingType=NumberingType.ParagraphNumbering,
            //            MonthPattern="(?<month>[0-1]?[0-9])��",
            //                },
            //          new WordParserDiarySegment(){
            //                    TargetTag="�����ܽ�",
            //                    TitleInDocument="�¼�",
            //                    TimeUnit=TimeUnit.Year,
            //                    DayNumberingType=NumberingType.ParagraphNumbering,
            //                },
            //    ]);
            //await wr.ParseAsync(@"C:\Users\autod\OneDrive\��������\�ռ�\2020.docx", options);

            //var options = new WordParserOptions(2024, [
            //    new WordParserDiarySegment(){
            //            TitleInDocument="�ռ�",
            //            TargetTag="�ռǣ��򵥣�",
            //            TimeUnit=TimeUnit.Day,
            //            DayNumberingType=NumberingType.ParagraphNumbering,
            //            MonthPattern="(?<month>[0-1]?[0-9])��",
            //        },       new WordParserDiarySegment(){
            //            TitleInDocument="�ռ�",
            //            TargetTag="�ռ�",
            //            TimeUnit=TimeUnit.Day,
            //            DayNumberingType=NumberingType.OutlineTitle,
            //            MonthPattern="(?<month>[0-1]?[0-9])��",
            //            DayPattern=@"[0-9]{1,2}\-(?<day>[0-9]{1,2})[~��0-9 ]*(?<title>.*)"
            //        },
            //          new WordParserDiarySegment(){
            //                    TargetTag="�����ܽ�",
            //                    TitleInDocument="����ܽ�",
            //                    TimeUnit=TimeUnit.Year,
            //                    LargestInnerLevel=2
            //                },
            //    ]);
            //await wr.ParseAsync(@"C:\Users\autod\OneDrive\��������\�ռ�\2017.docx", options); 
            
            var options = new WordParserOptions(2024, [
                new WordParserDiarySegment(){
                        TitleInDocument="�ռ�",
                        TargetTag="�ռ�",
                        TimeUnit=TimeUnit.Day,
                        DayNumberingType=NumberingType.OutlineTitle,
                        MonthPattern="(?<month>[0-1]?[0-9])",
                        DayPattern=@"(?<day>[0-9]{1,2})[-��0-9 ]*(?<title>.*)"
                    },
                      new WordParserDiarySegment(){
                                TargetTag="�����ܽ�",
                                TitleInDocument="����ܽ�",
                                TimeUnit=TimeUnit.Year,
                                LargestInnerLevel=2
                            },
                ]);
            await wr.ParseAsync(@"C:\Users\autod\OneDrive\��������\�ռ�\2016.docx", options);
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
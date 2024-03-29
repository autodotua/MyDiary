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
            //            TitleInDocument="日记",
            //            TargetTag="日记",
            //            TimeUnit=TimeUnit.Day,
            //            DayNumberingType=NumberingType.ParagraphNumbering,
            //            MonthPattern="(?<month>[0-1]?[0-9])月",
            //        },
            //          new WordParserDiarySegment(){
            //                    TitleInDocument="科研日志",
            //                    TargetTag="科研日志",
            //                    TimeUnit=TimeUnit.Day,
            //                    DayNumberingType=NumberingType.ParagraphNumbering,
            //            MonthPattern="(?<month>[0-1]?[0-9])月",
            //                },  new WordParserDiarySegment(){
            //                    TitleInDocument="求职日志",
            //                    TargetTag="求职日志",
            //                    TimeUnit=TimeUnit.Day,
            //                    DayNumberingType=NumberingType.ParagraphNumbering,
            //            MonthPattern="(?<month>[0-1]?[0-9])月",
            //                },
            //          new WordParserDiarySegment(){
            //                    TargetTag="年终总结",
            //                    TitleInDocument="事件",
            //                    TimeUnit=TimeUnit.Year,
            //                    DayNumberingType=NumberingType.ParagraphNumbering,
            //                },
            //    ]);
            //await wr.ParseAsync(@"C:\Users\autod\OneDrive\旧事重提\日记\2020.docx", options);

            //var options = new WordParserOptions(2024, [
            //    new WordParserDiarySegment(){
            //            TitleInDocument="日记",
            //            TargetTag="日记（简单）",
            //            TimeUnit=TimeUnit.Day,
            //            DayNumberingType=NumberingType.ParagraphNumbering,
            //            MonthPattern="(?<month>[0-1]?[0-9])月",
            //        },       new WordParserDiarySegment(){
            //            TitleInDocument="日记",
            //            TargetTag="日记",
            //            TimeUnit=TimeUnit.Day,
            //            DayNumberingType=NumberingType.OutlineTitle,
            //            MonthPattern="(?<month>[0-1]?[0-9])月",
            //            DayPattern=@"[0-9]{1,2}\-(?<day>[0-9]{1,2})[~、0-9 ]*(?<title>.*)"
            //        },
            //          new WordParserDiarySegment(){
            //                    TargetTag="年终总结",
            //                    TitleInDocument="年度总结",
            //                    TimeUnit=TimeUnit.Year,
            //                    LargestInnerLevel=2
            //                },
            //    ]);
            //await wr.ParseAsync(@"C:\Users\autod\OneDrive\旧事重提\日记\2017.docx", options); 
            
            var options = new WordParserOptions(2024, [
                new WordParserDiarySegment(){
                        TitleInDocument="日记",
                        TargetTag="日记",
                        TimeUnit=TimeUnit.Day,
                        DayNumberingType=NumberingType.OutlineTitle,
                        MonthPattern="(?<month>[0-1]?[0-9])",
                        DayPattern=@"(?<day>[0-9]{1,2})[-、0-9 ]*(?<title>.*)"
                    },
                      new WordParserDiarySegment(){
                                TargetTag="年终总结",
                                TitleInDocument="年度总结",
                                TimeUnit=TimeUnit.Year,
                                LargestInnerLevel=2
                            },
                ]);
            await wr.ParseAsync(@"C:\Users\autod\OneDrive\旧事重提\日记\2016.docx", options);
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
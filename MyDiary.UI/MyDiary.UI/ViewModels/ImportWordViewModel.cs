using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyDiary.Models;
using MyDiary.WordParser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDiary.UI.ViewModels
{
    public partial class ImportWordViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string file;

        [ObservableProperty]
        private int? year;

        [ObservableProperty]
        private string message;

        public ImportWordViewModel()
        {
            Segments.CollectionChanged += Segments_CollectionChanged;
        }

        public ObservableCollection<WordParserDiarySegmentViewModel> Segments { get; } = [new WordParserDiarySegmentViewModel()];

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.PropertyName == nameof(File))
            {
                if (System.IO.File.Exists(File))
                {
                    if (int.TryParse(System.IO.Path.GetFileNameWithoutExtension(File), out int year))
                    {
                        if (year > 0 && year <= DateTime.Now.Year)
                        {
                            Year = year;
                        }
                    }
                }
            }
        }

        [RelayCommand]
        private void AddSegment(WordParserDiarySegment segment)
        {
            Segments.Add(new WordParserDiarySegmentViewModel());
        }

        [RelayCommand]
        private void RemoveSegment(WordParserDiarySegmentViewModel segment)
        {
            Segments.Remove(segment);
        }

        private void Segments_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            for (int i = 0; i < Segments.Count; i++)
            {
                Segments[i].Index = i + 1;
            }
        }
        public partial class WordParserDiarySegmentViewModel : ViewModelBase
        {
            [ObservableProperty]
            private int index = 1;

            [ObservableProperty]
            private WordParserDiarySegment content = new WordParserDiarySegment();

            public IEnumerable NumberingTypes => Enum.GetValues<NumberingType>();

            public IEnumerable TimeUnits => Enum.GetValues<TimeUnit>();
        }
    }
}

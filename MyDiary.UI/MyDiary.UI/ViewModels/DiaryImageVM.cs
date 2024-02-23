using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using System.IO;

namespace MyDiary.UI.ViewModels
{
    public partial class DiaryImageVM : ViewModelBase
    {
        public IImage ImageSource
        {
            get
            {
                if(ImageData==null)
                {
                    return null;
                }
                using var ms = new MemoryStream(ImageData);
                return new Bitmap(ms);
            }
        }
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ImageSource))]
        private byte[] imageData;
        [ObservableProperty]
        private string title;
        public int? ImageDataId { get; set; }
    }
}

using MyDiary.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDiary.Core.Services
{
    public class DataService
    {
        public IList<DocumentPart> GetDocument(DateTime date)
        {
            return new List<DocumentPart>()
            {
                new TextElement(){Text="hello",UseDefaultTextColor=true}
            };
        }
    }
}

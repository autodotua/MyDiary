using MyDiary.UI.ViewModels;
using System;

namespace MyDiary.UI.Views.DiaryDocElement;

public interface IDiaryElement
{
    public event EventHandler EditPropertiesUpdated;
    public EditProperties GetEditProperties();
}

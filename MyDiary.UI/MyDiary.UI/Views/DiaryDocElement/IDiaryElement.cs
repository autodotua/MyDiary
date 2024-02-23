using MyDiary.Core.Models;
using MyDiary.UI.ViewModels;
using System;

namespace MyDiary.UI.Views.DiaryDocElement;

public interface IDiaryElement
{
    public event EventHandler NotifyEditDataUpdated;
    public EditBarVM GetEditData();

    public void LoadData(Block data);

    public Block GetData();
}

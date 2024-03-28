using MyDiary.Models;

namespace MyDiary.Managers.Services
{
    public class PresetStyleManager(DiaryDbContext db) 
    {
        private readonly DiaryDbContext db = db;

        public TextStyle GetByLevel(int level)
        {
            return db.PresetStyles.FirstOrDefault(p => p.Level == level && !p.IsDeleted)?.Style;
        }

        public IDictionary<int, TextStyle> GetAll()
        {
            return db.PresetStyles.Where(p => !p.IsDeleted && p.Style != null).ToDictionary(p => p.Level, p => p.Style);
        }
    }
}
using Microsoft.EntityFrameworkCore;
using MyDiary.Models;

namespace MyDiary.Managers.Services
{
    public class PresetStyleManager(DiaryDbContext db)
    {
        private readonly DiaryDbContext db = db;

        public async Task<TextStyle> GetByLevelAsync(int level)
        {
            return (await db.PresetStyles.FirstOrDefaultAsync(p => p.Level == level && !p.IsDeleted))?.Style;
        }

        public async Task<IDictionary<int, TextStyle>> GetAllAsync()
        {
            return await db.PresetStyles.Where(p => !p.IsDeleted && p.Style != null).ToDictionaryAsync(p => p.Level, p => p.Style);
        }
    }
}
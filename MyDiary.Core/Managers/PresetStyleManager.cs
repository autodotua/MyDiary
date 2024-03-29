using Microsoft.EntityFrameworkCore;
using MyDiary.Models;

namespace MyDiary.Managers.Services
{
    public class PresetStyleManager(DiaryDbContext db)
    {
        private readonly DiaryDbContext db = db;

        private static Dictionary<int, TextStyle> cache;

        public async Task<TextStyle> GetByLevelAsync(int level)
        {
            if (cache == null)
            {
                await GetAllAsync();
            }
            if (cache.TryGetValue(level, out TextStyle value))
            {
                return value;
            }
            return null;
        }

        public async Task<IDictionary<int, TextStyle>> GetAllAsync()
        {
            cache ??= await db.PresetStyles.Where(p => !p.IsDeleted && p.Style != null).ToDictionaryAsync(p => p.Level, p => p.Style);
            return cache;
        }
    }
}
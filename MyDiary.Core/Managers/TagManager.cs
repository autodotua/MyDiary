using Microsoft.EntityFrameworkCore;
using MyDiary.Models;

namespace MyDiary.Managers.Services
{
    public class TagManager : IDisposable
    {
        private DiaryDbContext db = DiaryDbContext.GetNew();

        public void Dispose()
        {
            db?.Dispose();
        }

        public async Task<IList<string>> GetAllAsync(TimeUnit timeUnit)
        {
            var tags = await db.Tags
                .Where(p => !p.IsDeleted && p.TimeUnit == timeUnit)
                .Select(p => p.Name)
                .ToListAsync();
            return tags;
        }

        public async Task AddTagAsync(string tagName, TimeUnit timeUnit)
        {
            if (string.IsNullOrEmpty(tagName))
            {
                throw new ArgumentNullException(nameof(tagName), "标签名称不能为空。");
            }

            var existingTag = await db.Tags.FirstOrDefaultAsync(p => p.Name == tagName && !p.IsDeleted);

            if (existingTag != null)
            {
                throw new InvalidOperationException("标签已存在。");
            }

            db.Tags.Add(new Tag() { Name = tagName, TimeUnit = timeUnit });
            await db.SaveChangesAsync();
        }

        public async Task DeleteTagAsync(string tagName, TimeUnit timeUnit)
        {
            if (string.IsNullOrEmpty(tagName))
            {
                throw new ArgumentNullException(nameof(tagName), "标签名称不能为空。");
            }

            var tagToDelete = await db.Tags.FirstOrDefaultAsync(
                p => p.Name == tagName && p.TimeUnit == timeUnit && !p.IsDeleted)
                ?? throw new InvalidOperationException("未找到要删除的标签。");

            tagToDelete.IsDeleted = true;
            db.Entry(tagToDelete).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }
    }
}
using Microsoft.EntityFrameworkCore;
using MyDiary.Core.Models;

namespace MyDiary.Core.Services
{
    public class TagManager : IDisposable
    {
        public static string DefaultTagName { get; set; } = "日记";
        DiaryDbContext db = DiaryDbContext.GetNew();
        public void Dispose()
        {
            db?.Dispose();
        }

        public async Task<IList<string>> GetAllAsync()
        {
            var tags = await db.Tags
                .Where(p => !p.IsDeleted)
                .Select(p => p.Name)
                .ToListAsync();
            if (tags.Count == 0)
            {
                db.Tags.Add(new Tag() { Name = DefaultTagName });
                await db.SaveChangesAsync();
                tags.Add(DefaultTagName);
            }
            return tags;
        }

        public async Task AddTagAsync(string tagName)
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

            db.Tags.Add(new Tag() { Name = tagName });
            await db.SaveChangesAsync();
        }

        public async Task DeleteTagAsync(string tagName)
        {
            if (string.IsNullOrEmpty(tagName))
            {
                throw new ArgumentNullException(nameof(tagName), "标签名称不能为空。");
            }

            var tagToDelete = await db.Tags.FirstOrDefaultAsync(p => p.Name == tagName && !p.IsDeleted);

            if (tagToDelete == null)
            {
                throw new InvalidOperationException("未找到要删除的标签。");
            }

            tagToDelete.IsDeleted = true;
            db.Entry(tagToDelete).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }
    }

}

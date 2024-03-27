using Microsoft.EntityFrameworkCore;
using MyDiary.Models;
using System.Diagnostics;

namespace MyDiary.Managers.Services
{
    public class DocumentManager : IDisposable
    {
        private DiaryDbContext db = DiaryDbContext.GetNew();

        public void Dispose()
        {
            db?.Dispose();
        }

#if DEBUG

        public async Task ClearDocumentsAsync()
        {
            await db.Database.ExecuteSqlRawAsync($"delete from [{nameof(db.Documents)}]");
        }

#endif

        public async Task<Document> GetDocumentAsync(NullableDate date, string tag)
        {
            var docs = db.Documents
                .Where(p => p.Year == date.Year)
                .Where(p => p.Month == date.Month)
                .Where(p => p.Day == date.Day)
                .Where(p => p.Tag == tag)
                .Where(p => !p.IsDeleted);

            if (docs.Any())
            {
                Debug.Assert(docs.Count() == 1);
                return (await docs.FirstAsync());
            }
            return null;
        }

        public async Task SetDocumentAsync(NullableDate date, string tag, IList<Block> blocks, string title)
        {
            if (tag == TagManager.DefaultTagName)
            {
                tag = null;
            }
            var docs = db.Documents
                .Where(p => p.Year == date.Year)
                .Where(p => p.Month == date.Month)
                .Where(p => p.Day == date.Day)
                .Where(p => p.Tag == tag)
                .Where(p => !p.IsDeleted);

            if (docs.Any())
            {
                var doc = await docs.FirstAsync();
                doc.Blocks = blocks;
                doc.Title = title;
                db.Entry(doc).State = EntityState.Modified;
            }
            else
            {
                var doc = new Document()
                {
                    Year = date.Year,
                    Month = date.Month,
                    Day = date.Day,
                    Tag = tag,
                    Blocks = blocks,
                    Title = title
                };
                db.Documents.Add(doc);
            }
            await db.SaveChangesAsync();
        }

        public async Task SetDocumentsAsync(IList<Document> documents)
        {
            var tags = await db.Tags
                .Where(p => !p.IsDeleted)
                .ToListAsync();
            foreach (var doc in documents)
            {
                var existedDoc = await db.Documents.Where(p =>
                p.Year == doc.Year
                && p.Month == doc.Month
                && p.Day == doc.Day
                && p.Tag == doc.Tag
                && !p.IsDeleted)
                    .FirstOrDefaultAsync();
                if (existedDoc != null)
                {
                    existedDoc.Blocks = doc.Blocks;
                    existedDoc.Title = doc.Title;
                    db.Entry(existedDoc).State = EntityState.Modified;
                }
                else
                {
                    var timeUnit = doc.Month.HasValue ? (doc.Day.HasValue ? TimeUnit.Day : TimeUnit.Month) : TimeUnit.Year;
                    if (!tags.Any(p => p.Name == doc.Tag && p.TimeUnit == timeUnit))
                    {
                        Tag tag = new Tag()
                        {
                            Name = doc.Tag,
                            TimeUnit = timeUnit,
                        };
                        db.Tags.Add(tag);
                        tags.Add(tag);
                    }
                    db.Documents.Add(doc);
                }
            }
            await db.SaveChangesAsync();
        }
    }
}
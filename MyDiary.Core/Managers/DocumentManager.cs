using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyDiary.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDiary.Managers.Services
{
    public class DocumentManager : IDisposable
    {
        DiaryDbContext db = DiaryDbContext.GetNew();

        public void Dispose()
        {
            db?.Dispose();
        }

        public async Task<Document> GetDocumentAsync(DateTime date, string tag)
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
                Debug.Assert(docs.Count() == 1);
                return (await docs.FirstAsync());
            }
            return null;
        }
        public async Task SetDocumentAsync(DateTime date, string tag, IList<Block> blocks, string title)
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
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyDiary.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDiary.Core.Services
{
    public class DoumentManager
    {
        DiaryDbContext db = DiaryDbContext.GetNew();
        public async Task<IList<Block>> GetDocumentAsync(DateTime date, string tag)
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
                return (await docs.FirstAsync()).Blocks;
            }
            return null;
        }
        public async Task SetDocumentAsync(DateTime date, string tag, IList<Block> blocks)
        {
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
                    Blocks = blocks
                };
                db.Documents.Add(doc);
            }
            await db.SaveChangesAsync();
        }
    }
}

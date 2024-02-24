using MyDiary.Models;

namespace MyDiary.Managers.Services
{
    public class BinaryManager : IDisposable
    {
        DiaryDbContext db = DiaryDbContext.GetNew();
        public async Task<byte[]> GetBinaryAsync(int id)
        {
            var item = await db.Binaries.FindAsync(id);
            return item?.Data;
        }
        public async Task<int> AddBinaryAsync(byte[] data)
        {
            var binary = new Binary() { Data = data };
            db.Binaries.Add(binary);
            await db.SaveChangesAsync();
            return binary.Id;
        }
        public Task<int> UpdateBinaryAsync(int id, byte[] data)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}

using MyDiary.Managers.Services;
using MyDiary.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LocalDataManager : IDataManager
{
    private BinaryManager binaryManager = new BinaryManager();
    private DocumentManager documentManager = new DocumentManager();
    private TagManager tagManager = new TagManager();

    public Task<int> AddBinaryAsync(byte[] data)
    {
        return binaryManager.AddBinaryAsync(data);
    }

    public Task AddTagAsync(string tagName, TimeUnit timeUnit)
    {
        return tagManager.AddTagAsync(tagName, timeUnit);
    }

    public Task DeleteTagAsync(string tagName, TimeUnit timeUnit)
    {
        return tagManager.DeleteTagAsync(tagName, timeUnit);
    }

    public void Dispose()
    {
        binaryManager.Dispose();
        documentManager.Dispose();
        tagManager.Dispose();
    }

    public Task<byte[]> GetBinaryAsync(int id)
    {
        return binaryManager.GetBinaryAsync(id);
    }

    public Task<Document> GetDocumentAsync(NullableDate date, string tag)
    {
        return documentManager.GetDocumentAsync(date, tag);
    }

    public Task<IList<string>> GetTagsAsync(TimeUnit timeUnit)
    {
        return tagManager.GetAllAsync(timeUnit);
    }

    public Task SetDocumentAsync(NullableDate date, string tag, IList<Block> blocks, string title)
    {
        return documentManager.SetDocumentAsync(date, tag, blocks, title);
    }

    public Task UpdateBinaryAsync(int id, byte[] data)
    {
        return binaryManager.UpdateBinaryAsync(id, data);
    }
}
using MyDiary.Managers.Services;
using MyDiary.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
public class LocalDataManager : IDataManager
{
    BinaryManager binaryManager = new BinaryManager();
    DocumentManager documentManager = new DocumentManager();
    TagManager tagManager = new TagManager();
    public Task<int> AddBinaryAsync(byte[] data)
    {
        return binaryManager.AddBinaryAsync(data);
    }

    public Task AddTagAsync(string tagName)
    {
        return tagManager.AddTagAsync(tagName);
    }

    public Task DeleteTagAsync(string tagName)
    {
        return DeleteTagAsync(tagName);
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

    public Task<Document> GetDocumentAsync(DateTime date, string tag)
    {
        return documentManager.GetDocumentAsync(date, tag);
    }

    public Task<IList<string>> GetTagsAsync()
    {
        return tagManager.GetAllAsync();
    }

    public Task SetDocumentAsync(DateTime date, string tag, IList<Block> blocks, string title)
    {
        return documentManager.SetDocumentAsync(date, tag, blocks, title);
    }

    public Task UpdateBinaryAsync(int id, byte[] data)
    {
        return binaryManager.UpdateBinaryAsync(id, data);
    }
}

﻿using MyDiary.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
public interface IDataManager : IDisposable
{
    Task<int> AddBinaryAsync(byte[] data);
    Task AddTagAsync(string tagName);
    Task DeleteTagAsync(string tagName);
    Task<byte[]> GetBinaryAsync(int id);
    Task<Document> GetDocumentAsync(DateTime date, string tag);
    Task<IList<string>> GetTagsAsync();
    Task SetDocumentAsync(DateTime date, string tag, IList<Block> blocks, string title);
    Task UpdateBinaryAsync(int id, byte[] data);
}
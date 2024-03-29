using MyDiary.Models;
using MyDiary.Models.Converters;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class WebDataProvider : IDataProvider
{
    private readonly HttpClient HttpClient = new HttpClient();
    private const string BaseApiUrl = "https://localhost:7135/api";

    private const string DocumentEndpoint = "Document";
    private const string SetDocumentEndpoint = "Document";
    private const string AddTagEndpoint = "Tag";
    private const string GetTagsEndpoint = "Tag";
    private const string UpdateBinaryEndpoint = "Binary";
    private const string GetBinaryEndpoint = "Binary";
    private const string AddBinaryEndpoint = "Binary";
    private const string DeleteTagEndpoint = "Tag";

    public async Task<Document> GetDocumentAsync(NullableDate date, string tag)
    {
        var endpoint = $"{DocumentEndpoint}?date={date:yyyy-MM-dd}&tag={tag}";
        return await GetAsync<Document>(endpoint);
    }

    public async Task<int> AddBinaryAsync(byte[] data)
    {
        var endpoint = AddBinaryEndpoint;
        return await PostAsync<int>(endpoint, data);
    }

    public async Task SetDocumentAsync(NullableDate date, string tag, IList<Block> blocks, string title)
    {
        var endpoint = $"{SetDocumentEndpoint}?date={date:yyyy-MM-dd}&tag={tag}&title={title}";
        await PostAsync(endpoint, blocks);
    }

    public async Task<byte[]> GetBinaryAsync(int id)
    {
        var endpoint = $"{GetBinaryEndpoint}/{id}";
        var response = await HttpClient.GetAsync($"{BaseApiUrl}/{endpoint}");

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsByteArrayAsync();
        }
        else
        {
            throw new Exception($"API请求失败，状态码：{response.StatusCode}");
        }
    }

    public async Task<IList<string>> GetTagsAsync(TimeUnit timeUnit)
    {
        var endpoint = $"{GetTagsEndpoint}?timeUnit={timeUnit}";
        return await GetAsync<IList<string>>(endpoint);
    }

    public async Task AddTagAsync(string tagName, TimeUnit timeUnit)
    {
        await PostAsync(AddTagEndpoint, new { tagName, timeUnit });
    }

    public async Task UpdateBinaryAsync(int id, byte[] data)
    {
        var endpoint = $"{UpdateBinaryEndpoint}/{id}";
        await PutAsync(endpoint, data);
    }

    public async Task DeleteTagAsync(string tagName, TimeUnit timeUnit)
    {
        var endpoint = $"{DeleteTagEndpoint}?tagName={tagName}&timeUnit={timeUnit}";
        await DeleteAsync(endpoint);
    }

    private async Task<T> GetAsync<T>(string endpoint) where T : class
    {
        var response = await HttpClient.GetAsync($"{BaseApiUrl}/{endpoint}");

        if (response.IsSuccessStatusCode)
        {
            using var responseStream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<T>(responseStream, EfJsonConverter<object>.jsonOptions);
        }
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        else
        {
            throw new Exception($"API请求失败，状态码：{response.StatusCode}");
        }
    }

    private async Task PostAsync(string endpoint, object data)
    {
        var jsonContent = new StringContent(JsonSerializer.Serialize(data, EfJsonConverter<object>.jsonOptions), Encoding.UTF8, "application/json");
        var response = await HttpClient.PostAsync($"{BaseApiUrl}/{endpoint}", jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"API请求失败，状态码：{response.StatusCode}");
        }
    }

    private async Task<T> PostAsync<T>(string endpoint, object data)
    {
        var jsonContent = new StringContent(JsonSerializer.Serialize(data, EfJsonConverter<object>.jsonOptions), Encoding.UTF8, "application/json");
        var response = await HttpClient.PostAsync($"{BaseApiUrl}/{endpoint}", jsonContent);
        return await JsonSerializer.DeserializeAsync<T>(await response.Content.ReadAsStreamAsync(), EfJsonConverter<object>.jsonOptions);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"API请求失败，状态码：{response.StatusCode}");
        }
    }

    private async Task PutAsync(string endpoint, object data)
    {
        var jsonContent = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
        var response = await HttpClient.PutAsync($"{BaseApiUrl}/{endpoint}", jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"API请求失败，状态码：{response.StatusCode}");
        }
    }

    private async Task DeleteAsync(string endpoint)
    {
        var response = await HttpClient.DeleteAsync($"{BaseApiUrl}/{endpoint}");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"API请求失败，状态码：{response.StatusCode}");
        }
    }

    public void Dispose()
    {
        HttpClient.Dispose();
    }

    public Task<TextStyle> GetPresetStyleByLevelAsync(int level)
    {
        throw new NotImplementedException();
    }

    public Task<IDictionary<int, TextStyle>> GetPresetStylesAsync()
    {
        throw new NotImplementedException();
    }
}
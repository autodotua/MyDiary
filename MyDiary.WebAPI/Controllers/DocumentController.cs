using Microsoft.AspNetCore.Mvc;
using MyDiary.Models;
using MyDiary.Managers.Services;
using MyDiary.Models.Converters;
using static System.Reflection.Metadata.BlobBuilder;
using MyDiary.Core.Models;

namespace MyDiary.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly DocumentManager documentManager;
        public DocumentController(DocumentManager documentManager)
        {
            this.documentManager = documentManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetDocumentAsync(NullableDate date, string tag)
        {
            try
            {
                var document = await documentManager.GetDocumentAsync(date, tag);
                if (document != null)
                {
                    return Ok(System.Text.Json.JsonSerializer.Serialize(document, EfJsonConverter<object>.jsonOptions));
                }
                else
                {
                    return NotFound(); // 或返回更合适的状态码
                }
            }
            catch (Exception ex)
            {
                // 记录异常
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetDocumentAsync(NullableDate date, string tag, string title)
        {
            try
            {
                using StreamReader reader = new StreamReader(HttpContext.Request.Body);
                string requestBody = await reader.ReadToEndAsync();
                var blocks = System.Text.Json.JsonSerializer.Deserialize<List<Block>>(requestBody, EfJsonConverter<object>.jsonOptions);
                await documentManager.SetDocumentAsync(date, tag, blocks, title);
                return Ok(); // 或返回更合适的响应
            }
            catch (Exception ex)
            {
                // 记录异常
                return StatusCode(500, ex.Message);
            }
        }
    }
}

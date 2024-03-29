using Microsoft.AspNetCore.Mvc;
using MyDiary.Managers.Services;
using MyDiary.Models;

namespace MyDiary.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagController : ControllerBase
    {
        private readonly TagManager tagManager;

        public TagController(TagManager tagManager)
        {
            this.tagManager = tagManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTagsAsync(TimeUnit timeUnit)
        {
            try
            {
                var tags = await tagManager.GetAllAsync(timeUnit);
                return Ok(tags);
            }
            catch (Exception ex)
            {
                // 记录异常
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddTagAsync(string tagName, TimeUnit timeUnit)
        {
            try
            {
                await tagManager.AddTagAsync(tagName, timeUnit);
                return Ok(); // 或返回更合适的响应
            }
            catch (ArgumentNullException ex)
            {
                // 参数为空异常
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                // 操作无效异常（例如标签已存在）
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                // 记录异常
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{tagName}")]
        public async Task<IActionResult> DeleteTagAsync(string tagName, TimeUnit timeUnit)
        {
            try
            {
                await tagManager.DeleteTagAsync(tagName, timeUnit);
                return Ok(); // 或返回更合适的响应
            }
            catch (ArgumentNullException ex)
            {
                // 参数为空异常
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                // 操作无效异常（例如标签不存在）
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                // 记录异常
                return StatusCode(500, ex.Message);
            }
        }
    }
}

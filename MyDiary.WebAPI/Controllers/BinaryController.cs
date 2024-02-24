using Microsoft.AspNetCore.Mvc;
using MyDiary.Managers.Services;

namespace MyDiary.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BinaryController : ControllerBase
    {
        private readonly BinaryManager binaryManager;

        public BinaryController(BinaryManager binaryManager)
        {
            this.binaryManager = binaryManager;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBinaryAsync(int id)
        {
            try
            {
                var binaryData = await binaryManager.GetBinaryAsync(id);

                if (binaryData != null)
                {
                    return File(binaryData, "application/octet-stream");
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
        public async Task<IActionResult> AddBinaryAsync([FromBody] byte[] data)
        {
            try
            {
                var binaryId = await binaryManager.AddBinaryAsync(data);
                return Ok(binaryId); // 或返回更合适的响应
            }
            catch (Exception ex)
            {
                // 记录异常
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBinaryAsync(int id, [FromBody] byte[] data)
        {
            try
            {
                // 实现更新二进制数据的逻辑
                await binaryManager.UpdateBinaryAsync(id, data);
                return Ok(); // 或返回更合适的响应
            }
            catch (NotImplementedException ex)
            {
                // 未实现的异常
                return StatusCode(501, ex.Message);
            }
            catch (Exception ex)
            {
                // 记录异常
                return StatusCode(500, ex.Message);
            }
        }
    }
}

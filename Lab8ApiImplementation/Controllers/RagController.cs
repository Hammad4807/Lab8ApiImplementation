using Lab8ApiImplementation.Dtos;
using Lab8ApiImplementation.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lab8ApiImplementation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RagController : ControllerBase
    {
        private readonly IRagService _ragService;

        public RagController(IRagService ragService)
        {
            _ragService = ragService;
        }

        // POST /api/rag/ask
        // RagRequestValidator checks the request before this method runs
        [HttpPost("ask")]
        public async Task<IActionResult> Ask(RagRequest request)
        {
            RagResponse response = await _ragService.AskAsync(request.Question);
            return Ok(response);
        }
    }
}

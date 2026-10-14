using Lab8ApiImplementation.Dtos;
using Lab8ApiImplementation.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lab8ApiImplementation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        // POST /api/chat
        // ChatRequestValidator checks the request before this method runs
        [HttpPost]
        public async Task<IActionResult> Send(ChatRequest request)
        {
            ChatResponse response = await _chatService.SendAsync(request.Messages);
            return Ok(response);
        }
    }
}

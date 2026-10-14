using Microsoft.AspNetCore.Mvc;

namespace Lab8ApiImplementation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CrashController : ControllerBase
    {
        // GET /api/crash
        // Throws an error on purpose to test ErrorHandlingMiddleware (no try-catch here)
        [HttpGet]
        public IActionResult Crash()
        {
            throw new Exception("Test error from Crash endpoint.");
        }
    }
}

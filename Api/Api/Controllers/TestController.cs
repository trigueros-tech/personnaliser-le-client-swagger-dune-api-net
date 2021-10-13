using Api.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        [RequireRight("CanReadTest")]
        public string Get()
        {
            var claim = User.FindFirst("preferred_username");
            return $"Hello {claim?.Value}";
        }
    }
}
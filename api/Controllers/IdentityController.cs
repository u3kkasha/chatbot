using Microsoft.AspNetCore.Mvc;
using Chatbot.Modules.Identity;

namespace Chatbot.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class IdentityController : ControllerBase
{
    private readonly IdentityModule _module = new();

    [HttpGet]
    public IActionResult Get() => Ok(_module.GetType().Name);
}

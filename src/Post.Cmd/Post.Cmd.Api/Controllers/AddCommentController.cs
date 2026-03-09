using CQRS.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Post.Cmd.Api.Commands;

namespace Post.Cmd.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AddCommentController(ILogger<AddCommentController> _logger, ICommandDispatcher _commandDispatcher) : ControllerBase
{
    [HttpPost("{id}")]
    public async Task<ActionResult> AddCommentAsync(AddCommentCommand command)
    {
        return Ok();
    }
}

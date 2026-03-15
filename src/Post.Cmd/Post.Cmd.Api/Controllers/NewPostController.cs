using CQRS.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Post.Cmd.Api.Commands;
using Post.Common.DTOs;

namespace Post.Cmd.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class NewPostController(ILogger<NewPostController> logger, ICommandDispatcher commandDispacther) : ControllerBase
{
    private readonly ILogger<NewPostController> _logger = logger;
    private readonly ICommandDispatcher _commandDispatcher = commandDispacther;

    [HttpPost]
    public async Task<ActionResult> NewPostAsync(NewPostCommand command)
    {
        command.Id = Guid.NewGuid();
        try
        {
            await _commandDispatcher.SendAsync(command);

            return StatusCode(StatusCodes.Status201Created, new BaseResponse
            {
                Message = "New post creation request completed successfully."
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Client made a bad request.");
            return BadRequest(new BaseResponse
            {
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            const string SAFE_ERROR_MESSAGE = "Error while processing request to create a new post";
            _logger.LogError(ex, SAFE_ERROR_MESSAGE);

            return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
            {
                Message = SAFE_ERROR_MESSAGE
            });
        }
    }
}

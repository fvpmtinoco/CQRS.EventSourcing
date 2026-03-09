using CQRS.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Post.Cmd.Api.Commands;
using Post.Cmd.Api.DTOs;
using Post.Common.DTOs;

namespace Post.Cmd.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AddCommentController(ILogger<AddCommentController> _logger, ICommandDispatcher _commandDispatcher) : ControllerBase
{
    [HttpPut("{id}")]
    public async Task<ActionResult> AddCommentAsync(Guid id, AddCommentCommand command)
    {
        command.Id = id;
        try
        {
            await _commandDispatcher.SendAsync(command);

            return StatusCode(StatusCodes.Status201Created, new NewPostCommand
            {
                Message = "New comment creation request completed successfully."
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
            const string SAFE_ERROR_MESSAGE = "Error while processing request to add a comment to a post";
            _logger.LogError(ex, SAFE_ERROR_MESSAGE);

            return StatusCode(StatusCodes.Status500InternalServerError, new NewPostResponse
            {
                Id = command.Id,
                Message = SAFE_ERROR_MESSAGE
            });
        }
    }
}

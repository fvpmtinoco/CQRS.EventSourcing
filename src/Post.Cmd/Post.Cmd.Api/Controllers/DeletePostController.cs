using CQRS.Core.Exceptions;
using CQRS.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Post.Cmd.Api.Commands;
using Post.Common.DTOs;

namespace Post.Cmd.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class DeletePostController(ICommandDispatcher commandDispatcher, ILogger<DeletePostController> logger) : ControllerBase
{
    private readonly ICommandDispatcher _commandDispatcher = commandDispatcher;
    private readonly ILogger<DeletePostController> _logger = logger;

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePostAsync(Guid id, DeletePostCommand command)
    {
        command.Id = id;
        try
        {
            await _commandDispatcher.SendAsync(command);
            return StatusCode(StatusCodes.Status200OK, new BaseResponse
            {
                Message = "Post delete request completed successfully!"
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Client made a bad request while trying to delete a post.");
            return BadRequest(new BaseResponse
            {
                Message = ex.Message
            });
        }
        catch (AggregateNotFoundException ex)
        {
            _logger.LogError(ex, "Could not retrieve aggregate, client passed an incorrect post Id targetting the aggregate.");
            return NotFound(new BaseResponse
            {
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            const string SAFE_ERROR_MESSAGE = "Error while processing request to delete a post";
            _logger.LogError(ex, SAFE_ERROR_MESSAGE);
            return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
            {
                Message = SAFE_ERROR_MESSAGE
            });
        }
    }
}

using CQRS.Core.Exceptions;
using CQRS.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Post.Cmd.Api.Commands;
using Post.Common.DTOs;

namespace Post.Cmd.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class EditMessageController(ILogger<EditMessageController> logger, ICommandDispatcher commandDispacther) : ControllerBase
{
    private readonly ILogger<EditMessageController> _logger = logger;
    private readonly ICommandDispatcher _commandDispatcher = commandDispacther;

    [HttpPut("{id}")]
    public async Task<ActionResult> EditMessageAsync(Guid id, EditMessageCommand command)
    {
        try
        {
            command.Id = id;
            await _commandDispatcher.SendAsync(command);

            return Ok(new BaseResponse
            {
                Message = "Message edit request completed successfully!"
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
            const string SAFE_ERROR_MESSAGE = "Error while processing request to edit the message of a post";
            _logger.LogError(ex, SAFE_ERROR_MESSAGE);

            return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
            {
                Message = SAFE_ERROR_MESSAGE
            });
        }
    }
}

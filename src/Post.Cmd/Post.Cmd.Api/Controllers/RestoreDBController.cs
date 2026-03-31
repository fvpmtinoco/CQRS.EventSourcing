using CQRS.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Post.Cmd.Api.Commands;
using Post.Common.DTOs;

namespace Post.Cmd.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class RestoreDBController(ILogger<RestoreDBController> logger, ICommandDispatcher commandDispacther) : ControllerBase
{
    private readonly ILogger<RestoreDBController> _logger = logger;
    private readonly ICommandDispatcher _commandDispatcher = commandDispacther;

    [HttpPost]
    public async Task<ActionResult> RestoreReadDbAsync()
    {
        try
        {
            await _commandDispatcher.SendAsync(new RestoreDBCommand());

            return StatusCode(StatusCodes.Status200OK, new BaseResponse
            {
                Message = "Restore read Db request completed successfully."
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
            const string SAFE_ERROR_MESSAGE = "Error while processing request to restore read database";
            _logger.LogError(ex, SAFE_ERROR_MESSAGE);

            return StatusCode(StatusCodes.Status500InternalServerError, new BaseResponse
            {
                Message = SAFE_ERROR_MESSAGE
            });
        }
    }
}

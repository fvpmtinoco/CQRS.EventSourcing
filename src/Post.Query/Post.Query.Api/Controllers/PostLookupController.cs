using CQRS.Core.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Post.Query.Api.DTOs;
using Post.Query.Api.Queries;
using Post.Query.Domain.Entities;

namespace Post.Query.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PostLookupController(ILogger<PostLookupController> logger, IQueryDispatcher<PostEntity> queryDispatcher) : ControllerBase
{
    private readonly ILogger<PostLookupController> _logger = logger;
    private readonly IQueryDispatcher<PostEntity> _queryDispatcher = queryDispatcher;

    [HttpGet]
    public async Task<IActionResult> GetAllPostsAsync()
    {
        try
        {
            var posts = await _queryDispatcher.SendAsync(new FindAllPostsQuery());

            return Ok(new PostLookupResponse
            {
                Posts = posts ?? [],
                Message = $"Successfully returned {(posts?.Count).GetValueOrDefault(0)} posts."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving posts.");
            return ErrorResponse();
        }
    }

    [HttpGet("/byId/{id:guid}")]
    public async Task<IActionResult> GetPostByIdAsync(Guid id)
    {
        try
        {
            var posts = await _queryDispatcher.SendAsync(new FindPostByIdQuery
            {
                Id = id
            });

            if (posts == null || posts.Count == 0)
            {
                return NotFound(new PostLookupResponse
                {
                    Posts = null!,
                    Message = $"No post found with ID: {id}."
                });
            }
            return Ok(new PostLookupResponse
            {
                Posts = posts,
                Message = $"Successfully returned post with ID: {id}."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving post with ID: {Id}.", id);
            return ErrorResponse();
        }
    }

    [HttpGet("/byAuthor/{author}")]
    public async Task<IActionResult> GetPostsByAuthorAsync(string author)
    {
        try
        {
            var posts = await _queryDispatcher.SendAsync(new FindPostsByAuthorQuery
            {
                Author = author
            });

            return Ok(new PostLookupResponse
            {
                Posts = posts ?? [],
                Message = $"Successfully returned {(posts?.Count).GetValueOrDefault(0)} posts for author: {author}."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving posts for author: {Author}.", author);
            return ErrorResponse();
        }
    }

    [HttpGet("/withComments")]
    public async Task<IActionResult> GetPostsWithCommentsAsync()
    {
        try
        {
            var posts = await _queryDispatcher.SendAsync(new FindPostsWithCommentsQuery());
            return Ok(new PostLookupResponse
            {
                Posts = posts ?? [],
                Message = $"Successfully returned {(posts?.Count).GetValueOrDefault(0)} posts with comments."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving posts with comments.");
            return ErrorResponse();
        }
    }

    [HttpGet("/withLikes/{numberOfLikes:int}")]
    public async Task<IActionResult> GetPostsWithLikesAsync(int numberOfLikes)
    {
        try
        {
            var posts = await _queryDispatcher.SendAsync(new FindPostWithLikesQuery
            {
                NumberOfLikes = numberOfLikes
            });

            return Ok(new PostLookupResponse
            {
                Posts = posts ?? [],
                Message = $"Successfully returned {(posts?.Count).GetValueOrDefault(0)} posts with at least {numberOfLikes} likes."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving posts with at least {NumberOfLikes} likes.", numberOfLikes);
            return ErrorResponse();
        }
    }

    private ObjectResult ErrorResponse()
    {
        return StatusCode(StatusCodes.Status500InternalServerError, new PostLookupResponse
        {
            Posts = null!,
            Message = "An error occurred while retrieving the posts. Please try again later."
        });
    }
}
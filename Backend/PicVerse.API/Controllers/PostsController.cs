using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PicVerse.API.DTOs.Posts;
using PicVerse.API.Services.Interfaces;

namespace PicVerse.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly ILogger<PostsController> _logger;

        public PostsController(IPostService postService, ILogger<PostsController> logger)
        {
            _postService = postService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                var posts = await _postService.GetFeedPostsAsync(userId, page, pageSize);
                
                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting posts");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPost(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var post = await _postService.GetPostByIdAsync(id, userId);
                
                if (post == null)
                {
                    return NotFound(new { message = "Post not found" });
                }
                
                return Ok(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting post {PostId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var post = await _postService.CreatePostAsync(userId, request);
                
                return CreatedAtAction(nameof(GetPost), new { id = post.Id }, post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(int id, [FromBody] UpdatePostRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _postService.UpdatePostAsync(id, userId, request);
                
                if (result.Success)
                {
                    return Ok(result.Post);
                }
                
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating post {PostId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _postService.DeletePostAsync(id, userId);
                
                if (result.Success)
                {
                    return Ok(new { message = "Post deleted successfully" });
                }
                
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting post {PostId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("{id}/like")]
        public async Task<IActionResult> ToggleLike(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _postService.ToggleLikeAsync(id, userId);
                
                if (result.Success)
                {
                    return Ok(new
                    {
                        isLiked = result.IsLiked,
                        likesCount = result.LikesCount
                    });
                }
                
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling like for post {PostId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{id}/comments")]
        public async Task<IActionResult> GetComments(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                var comments = await _postService.GetCommentsAsync(id, userId, page, pageSize);
                
                return Ok(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comments for post {PostId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("{id}/comments")]
        public async Task<IActionResult> CreateComment(int id, [FromBody] CreateCommentRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var comment = await _postService.CreateCommentAsync(id, userId, request);
                
                return Ok(comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment for post {PostId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            return int.Parse(userIdClaim ?? "0");
        }
    }
}
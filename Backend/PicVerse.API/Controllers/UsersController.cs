using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PicVerse.API.DTOs.Users;
using PicVerse.API.Services.Interfaces;

namespace PicVerse.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                var profile = await _userService.GetUserProfileAsync(userId);
                
                if (profile == null)
                {
                    return NotFound(new { message = "User not found" });
                }
                
                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _userService.UpdateProfileAsync(userId, request);
                
                if (result.Success)
                {
                    return Ok(result.User);
                }
                
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest(new { message = "Search query is required" });
                }
                
                var users = await _userService.SearchUsersAsync(query, page, pageSize);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users with query: {Query}", query);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var user = await _userService.GetUserByIdAsync(id, currentUserId);
                
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }
                
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user {UserId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("{id}/follow")]
        public async Task<IActionResult> ToggleFollow(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                if (userId == id)
                {
                    return BadRequest(new { message = "Cannot follow yourself" });
                }
                
                var result = await _userService.ToggleFollowAsync(userId, id);
                
                if (result.Success)
                {
                    return Ok(new
                    {
                        isFollowing = result.IsFollowing,
                        followersCount = result.FollowersCount
                    });
                }
                
                return BadRequest(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling follow for user {UserId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{id}/followers")]
        public async Task<IActionResult> GetFollowers(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var followers = await _userService.GetFollowersAsync(id, page, pageSize);
                return Ok(followers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting followers for user {UserId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{id}/following")]
        public async Task<IActionResult> GetFollowing(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var following = await _userService.GetFollowingAsync(id, page, pageSize);
                return Ok(following);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting following for user {UserId}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{id}/posts")]
        public async Task<IActionResult> GetUserPosts(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var posts = await _userService.GetUserPostsAsync(id, currentUserId, page, pageSize);
                
                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting posts for user {UserId}", id);
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
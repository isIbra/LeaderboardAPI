using Leaderboard.API.Infrastructure.Models.Auth;
using Leaderboard.API.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;

namespace Leaderboard.API.Controllers.v1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth")]
    [ApiController]
    [Produces("application/json")]
    [SwaggerTag("Auth")]
    public class AuthController : CustomControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(
            IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("webhook/token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerOperation (Summary = "Generate a token", Description = "Generates a token for the user based on the provided request parameters.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Token generated successfully", typeof(TokenResponse))]
        [AllowAnonymous]
        public async Task<ActionResult<TokenResponse>> GenerateToken([FromBody] TokenRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.UserId)
                                    || string.IsNullOrEmpty(request.Signature))
                {
                    return BadRequest("Invalid request parameters");
                }

                var response = await _authService.GenerateTokenAsync(request);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Warning("Unauthorized token generation attempt: {Message}", ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error generating token");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        [HttpGet("validate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [SwaggerOperation(Summary = "Validate token", Description = "Validates if the provided token is still valid.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Token is valid", typeof(bool))]
        [Authorize]
        public ActionResult<bool> ValidateToken()
        {
            return Ok(true);
        }
    }
}

using System;
using System.Threading.Tasks;
using Leaderboard.API.Infrastructure.Models.Analytics;
using Leaderboard.API.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog.Context;
using Swashbuckle.AspNetCore.Annotations;

namespace Leaderboard.API.Controllers.v1
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Produces("application/json")]
    [Route("api/v{version:apiVersion}/analytics/retention")]
    [SwaggerTag("Retention")]
    public class RetentionController : CustomControllerBase
    {
        private readonly IRetentionService _retentionService;

        public RetentionController(IRetentionService retentionService)
        {
            _retentionService = retentionService;
        }

        [HttpGet]
        [Route("metrics")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerOperation(Summary = "Get player retention metrics",
                         Description = "Returns retention rates at different intervals (1 day, 3 days, 7 days, 30 days) and new vs returning player metrics. \n\n default date if not provided is 30 days .")]
        [SwaggerResponse(StatusCodes.Status200OK, "Retention metrics retrieved successfully", typeof(GetRetentionRatesResponse))]
        public async Task<ActionResult<GetRetentionRatesResponse>> GetRetentionMetrics([FromQuery] GetRetentionRequest request)
        {
            try
            {
                LogContext.PushProperty("UserId", UserId);
                var response = await _retentionService.GetRetentionRatesAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                LogContext.PushProperty("UserId", UserId);
                LogContext.PushProperty("Error", ex.Message);
                return StatusCode(500, "An error occurred while retrieving retention metrics");
            }
        }

        [HttpGet]
        [Route("compare")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerOperation(Summary = "Get new vs returning player metrics",
                         Description = "Returns metrics comparing new players vs returning players for the specified period. \n\n default date if not provided is 30 days .")]
        [SwaggerResponse(StatusCodes.Status200OK, "New vs returning player metrics retrieved successfully", typeof(NewVsReturningMetrics))]
        public async Task<ActionResult<NewVsReturningMetrics>> GetNewVsReturningMetrics([FromQuery] GetRetentionRequest request)
        {
            try
            {
                LogContext.PushProperty("UserId", UserId);
                var response = await _retentionService.GetNewVsReturningMetricsAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                LogContext.PushProperty("UserId", UserId);
                LogContext.PushProperty("Error", ex.Message);
                return StatusCode(500, "An error occurred while retrieving new vs returning player metrics");
            }
        }
    }
}

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using Leaderboard.API.Infrastructure.Exceptions;
using Leaderboard.API.Infrastructure.Extensions;

namespace Leaderboard.API.Infrastructure.Exceptions.Filters
{
    public class HttpExceptionFilter : ExceptionFilterAttribute
    {
        private readonly ILogger _logger;

        [ActivatorUtilitiesConstructor]
        public HttpExceptionFilter(ILogger<HttpExceptionFilter> logger)
        {
            _logger = logger;

        }

        public override void OnException(ExceptionContext context)
        {
            if (context.Exception.GetType() == typeof(ApiException))
                HandleApiException(context);
            else
                HandleAllExceptions(context);
        }

        private void HandleApiException(ExceptionContext context)
        {

            _logger.LogInformation(context.Exception, context.Exception.Message);

            context.HttpContext.Request.Headers.TryGetValue("Accept-Language", out StringValues languageHeader);
            string lang = languageHeader;

            var serviceProvider = context.HttpContext.RequestServices;
            ApiException ex = (ApiException)context.Exception;

            IActionResult resContent = new JsonResult(ex.Error.GetProblemDetails(lang.FromStringLanguage(), serviceProvider));
            int resStatusCode = (int)ex.Error.HttpStatusCode;

            context.Result = resContent;
            context.HttpContext.Response.StatusCode = resStatusCode;

            context.ExceptionHandled = true;
        }

        private void HandleAllExceptions(ExceptionContext context)
        {
            _logger.LogError(context.Exception, context.Exception.Message);

            context.HttpContext.Request.Headers.TryGetValue("Accept-Language", out StringValues languageHeader);
            string lang = languageHeader;

            string messageAr = "حدث خطأ غير متوقع أثناء معالجة طلبك. الرجاء المحاولة لاحقاً";
            string messageEn = "An unexpected error has occured while processing your request. Please try again later.";
            int statusCode = (int)HttpStatusCode.InternalServerError;
            var json = new ProblemDetails
            {
                Status = statusCode,
                Type = "about:blank",
                Title = lang == "ar" ? messageAr : messageEn,
                Detail = lang == "ar" ? messageAr : messageEn,
            };

            context.Result = new JsonResult(json);
            context.HttpContext.Response.StatusCode = statusCode;
            context.ExceptionHandled = true;
        }
    }
}

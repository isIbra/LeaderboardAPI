using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Leaderboard.API.Infrastructure.Errors;
using Leaderboard.API.Infrastructure.Exceptions;
using Leaderboard.API.Infrastructure.Extensions;
using Leaderboard.API.Models.Enums;

namespace Leaderboard.API.Controllers
{
    public class CustomControllerBase : ControllerBase
    {
        private ClaimsPrincipal _user;

        private ClaimsPrincipal ClaimsPrincipalUser
        {
            get { return _user ??= User; }
        }

        protected LanguageEnum Language => Request.Headers["Accept-Language"].ToString().FromStringLanguage();

        protected string IdempotencyKey => Request.Headers["Idempotency-Key"].ToString()?.Trim();
        protected string UserId
        {
            get
            {
                Claim claim = ClaimsPrincipalUser.FindFirst("userId");

                if (claim == null)
                {
                    throw new ApiException(new ErrorResponse(HttpStatusCode.Unauthorized, "No user Id found in token",
                        "لم يتم العثور على رقم الحساب"));
                }
                return claim.Value;
            }
        }

        protected string Email
        {
            get
            {
                Claim claim = ClaimsPrincipalUser.FindFirst("email");
                if (claim == null)
                {

                    throw new ApiException(new ErrorResponse(HttpStatusCode.Unauthorized, "No Email found in token",
                        "لم يتم العثور على البريد الإلكتروني"));
                }

                return claim.Value;
            }
        }

        protected string DeviceType
        {
            get
            {
                if (HttpContext.Request.Headers.TryGetValue("X-Device-Type", out var deviceTypeHeader))
                {
                    return deviceTypeHeader.ToString();
                }
                return "Unknown";
            }
        }

        public string Client_ID
        {
            get
            {
                Claim claim = ClaimsPrincipalUser.FindFirst("client_id");
                if (claim == null)
                {
                    throw new ApiException(new ErrorResponse(HttpStatusCode.Unauthorized, "No client id found in token",
                        ""));
                }
                return claim.Value;
            }
        }

        protected string MobileNo
        {
            get
            {
                Claim claim = ClaimsPrincipalUser.FindFirst("MobileNo");
                if (claim == null)
                {
                    throw new ApiException(new ErrorResponse(HttpStatusCode.Unauthorized, "No Mobile Number Id found in token",
                        "لم يتم العثور على رقم الجوال"));
                }

                return claim.Value;
            }
        }

        protected string DeviceID
        {
            get
            {
                Claim u = ClaimsPrincipalUser.FindFirst("deviceUUID");
                if (u == null)
                {
                    throw new ApiException(new ErrorResponse(HttpStatusCode.Unauthorized, "No Device ID found in token",
                        "لم يتم العثور على معرف الجهاز في الرمز المميز"));
                }

                return u.Value;
            }
        }

    }
}

using Microsoft.AspNetCore.Http;
using Sentia.Api.Core.Exceptions;
using Sentia.Api.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Sentia.Api.Core.Interfaces
{
    public interface ISentiaAppContext
    {
        string AuthenticationType { get; }
        bool AuthenticationTypeIsAnonymous { get; }
        bool AuthenticationTypeIsLogin { get; }
        string ApiKey { get; }
        string ClientName { get; }
        string AuthorizationToken { get; }
        string ClientIp { get; }
        string Culture { get; }
        bool HasAuthentication { get; }
        int UserId { get; }
        string UserName { get; }
        string AppBuildVersion { get; }
        int AppVersionCode { get; }
    }

    public class SentiaAppContext : ISentiaAppContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SentiaAppContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int AppVersionCode
        {
            get
            {
                var versionCode = GetRequestHeader("AppVersionCode", false);
                int.TryParse(versionCode, out var result);
                return result;
            }
        }

        public string AuthenticationType => GetClaim(ClaimTypes.Authentication);
        public bool AuthenticationTypeIsAnonymous => AuthenticationType == "Anonymous";
        public bool AuthenticationTypeIsLogin => AuthenticationType == "Login";
        public string ApiKey => GetClaim(ClaimTypes.Sid);
        public string ClientName => GetClaim(ClaimTypes.Actor);
        public string AuthorizationToken => GetRequestHeader("Authorization");
        public string ClientIp => GetClientIp();
        public string Culture => GetRequestHeader("S-Culture");
        public bool HasAuthentication => !string.IsNullOrEmpty(AuthenticationType);
        public int UserId => Convert.ToInt32(GetClaim(ClaimTypes.UserData));
        public string UserName => GetClaim(ClaimTypes.Name);

        public string AppBuildVersion => "V1";

        private string GetClaim(string claimType)
        {
            var context = _httpContextAccessor.HttpContext;
            var claim = context.User.Claims.FirstOrDefault(c => c.Type == claimType);
            if (claim == null)
            {
                throw new SentiaCustomException($"{claimType}_is_missing_in_claims");
            }

            return claim.Value;
        }

        private string GetRequestHeader(string header, bool isRequired = true)
        {
            var context = _httpContextAccessor.HttpContext;

            context.Request.Headers.TryGetValue("x-" + header, out var value);

            if (!context.Request.Headers.TryGetValue(header, out value) && isRequired)
            {
                throw new SentiaCustomException(header + "_is_missing_in_http_request_headers");
            }

            return value;
        }

        private string GetClientIp()
        {
            var context = _httpContextAccessor.HttpContext;
            return context.GetClientIp();
        }
    }
}

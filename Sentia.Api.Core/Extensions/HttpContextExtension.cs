using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Sentia.Api.Core.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sentia.Api.Core.Extensions
{
    public static class HttpContextExtension
    {
        private static readonly RouteData EmptyRouteData = new RouteData();

        private static readonly ActionDescriptor EmptyActionDescriptor = new ActionDescriptor();

        public static Task WriteResultAsync<TResult>(this HttpContext context, TResult result) where TResult : IActionResult
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var executor = (IActionResultExecutor<TResult>)context.RequestServices.GetService(typeof(IActionResultExecutor<TResult>));

            if (executor == null)
            {
                throw new InvalidOperationException($"No result executor for '{typeof(TResult).FullName}' has been registered.");
            }

            var routeData = context.GetRouteData() ?? EmptyRouteData;

            var actionContext = new ActionContext(context, routeData, EmptyActionDescriptor);

            return executor.ExecuteAsync(actionContext, result);
        }

        public static ApiErrorDto CustomBadRequest(this ActionContext actionContext)
        {
            var responseError = new List<string>();
            foreach (var keyModelStatePair in actionContext.ModelState)
            {
                var errors = keyModelStatePair.Value.Errors;
                if (errors == null || errors.Count <= 0)
                {
                    continue;
                }

                foreach (var t in errors)
                {
                    responseError.Add(GetErrorMessage(t));
                }
            }

            return new ApiErrorDto(null)
            {
                Infos = responseError
            };
        }

        private static string GetErrorMessage(ModelError error)
        {
            return string.IsNullOrEmpty(error.ErrorMessage) ? "The input was not valid." : error.ErrorMessage;
        }

        public static string GetClientIp(this HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress.ToString();

            if (context.Request.Headers.TryGetValue("x-Client-IP", out var clientIp))
            {
                var ips = clientIp.ToString();
                return ips.Contains(",") ? ips.Split(',')[0] : ips;
            }

            if (context.Request.Headers.TryGetValue("Client-IP", out clientIp))
            {
                var ips = clientIp.ToString();
                return ips.Contains(",") ? ips.Split(',')[0] : ips;
            }

            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
            {
                var ips = forwardedFor.ToString();
                return ips.Contains(",") ? ips.Split(',')[0] : ips;
            }

            return ip;
        }
    }
}

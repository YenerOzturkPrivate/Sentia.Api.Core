using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Sentia.Api.Core.Exceptions;
using Sentia.Api.Core.Extensions;
using Sentia.Api.Core.Interfaces;
using Sentia.Api.Core.Model;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Sentia.Api.Core.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private ILogger _logger = Log.ForContext<ExceptionMiddleware>();

        private static readonly HashSet<string> CorsHeaderNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            HeaderNames.AccessControlAllowCredentials,
            HeaderNames.AccessControlAllowHeaders,
            HeaderNames.AccessControlAllowMethods,
            HeaderNames.AccessControlAllowOrigin,
            HeaderNames.AccessControlExposeHeaders,
            HeaderNames.AccessControlMaxAge,
        };

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, ISentiaAppContext appContext)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException validationException)
            {
                ApiErrorDto apiErrorDto;

                var errors = new List<string>();
                if (validationException.Errors.Any())
                {
                    foreach (var error in validationException.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }

                    apiErrorDto = new ApiErrorDto(null);
                }
                else
                {
                    apiErrorDto = new ApiErrorDto("Bad Request");
                }

                apiErrorDto.Infos = errors;

                await WriteLog(context, appContext, validationException, StatusCodes.Status400BadRequest).ConfigureAwait(false);
                await ClearResponseAndBuildErrorDto(context, apiErrorDto, StatusCodes.Status400BadRequest).ConfigureAwait(false);
            }
            catch (SentiaCustomException customException)
            {
                await WriteLog(context, appContext, customException, (int)customException.StatusCode).ConfigureAwait(false);
                await ClearResponseAndBuildErrorDto(context, new ApiErrorDto(null)
                {
                    Infos = new List<string> { customException.Message }
                }, statusCode: (int)customException.StatusCode).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await WriteLog(context, appContext, ex, StatusCodes.Status500InternalServerError).ConfigureAwait(false);
                await ClearResponseAndBuildErrorDto(context, new ApiErrorDto("Server Internal Error"), StatusCodes.Status500InternalServerError)
                    .ConfigureAwait(false);
            }
        }

        private static async Task ClearResponseAndBuildErrorDto(HttpContext context, ApiErrorDto errorDto, int statusCode)
        {
            var headers = new HeaderDictionary();

            // Make sure problem responses are never cached.
            headers.Append(HeaderNames.CacheControl, "no-cache, no-store, must-revalidate");
            headers.Append(HeaderNames.Pragma, "no-cache");
            headers.Append(HeaderNames.Expires, "0");

            foreach (var header in context.Response.Headers)
            {
                // Because the CORS middleware adds all the headers early in the pipeline,
                // we want to copy over the existing Access-Control-* headers after resetting the response.
                if (CorsHeaderNames.Contains(header.Key))
                {
                    headers.Add(header);
                }
            }

            context.Response.Clear();
            context.Response.StatusCode = statusCode;

            foreach (var header in headers)
            {
                context.Response.Headers.Add(header);
            }

            await context.WriteResultAsync(new ObjectResult(errorDto));
        }

        private async Task WriteLog(HttpContext context, ISentiaAppContext appContext, Exception exception, int statusCode)
        {
            var request = context.Request;
            var apiKey = string.Empty;
            var authenticationType = string.Empty;
            var userId = string.Empty;
            var clientName = string.Empty;

            try
            {
                if (context.User.Claims.Any(c => c.Type == ClaimTypes.Authentication))
                {
                    apiKey = appContext.ApiKey;
                    authenticationType = appContext.AuthenticationType;
                    userId = appContext.UserId;
                    clientName = appContext.ClientName;
                }
            }
            catch
            {
                //ignored
            }


            var requestPathAndQuery = request.GetEncodedPathAndQuery();

            _logger = _logger.ForContext("MachineName", Environment.MachineName)
                .ForContext("RequestHost", request.Host.Host)
                .ForContext("RequestProtocol", request.Protocol)
                .ForContext("RequestMethod", request.Method)
                .ForContext("ResponseStatusCode", statusCode)
                .ForContext("RequestPath", request.Path)
                .ForContext("RequestPathAndQuery", requestPathAndQuery)
                .ForContext("Exception", exception, true)
                .ForContext("RequestHeaders", request.Headers.ToDictionary(h => h.Key, h => (object)h.Value.ToString()), true)
                .ForContext("Exception", exception, true);

            if (!string.IsNullOrEmpty(authenticationType))
            {
                _logger = _logger.ForContext("UserId", userId)
                    .ForContext("ApiKey", apiKey)
                    .ForContext("AuthenticationType", authenticationType)
                    .ForContext("ClientName", clientName);
            }

            var errorTemplate = $"HTTP {request.Method} {requestPathAndQuery} responded {statusCode}";

            _logger.Error(exception, errorTemplate);

            await Task.FromResult(true);
        }
    }
}

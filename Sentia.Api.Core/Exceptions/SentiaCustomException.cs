using System;
using System.Net;

namespace Sentia.Api.Core.Exceptions
{
    [Serializable]
    public class SentiaCustomException : Exception
    {
        public readonly HttpStatusCode StatusCode;

        public SentiaCustomException(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}

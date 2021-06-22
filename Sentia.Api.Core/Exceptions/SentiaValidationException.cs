using System;

namespace Sentia.Api.Core.Exceptions
{
    [Serializable]
    public class SentiaValidationException : Exception
    {
        public SentiaValidationException(string message) : base(message)
        {

        }
    }
}

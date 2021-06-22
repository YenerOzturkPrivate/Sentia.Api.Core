using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sentia.Api.Core.Model
{
    public class ApiErrorDto
    {
        public ApiErrorDto(string message)
        {
            Message = message;
        }
        public string Message { get; set; }

        public List<string> Infos { get; set; }
    }
}

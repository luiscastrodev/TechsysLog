using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechsysLog.Application.Common
{
    public class BusinessResult<T>
    {
        public bool IsSuccess { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public T? Data { get; private set; }

        private BusinessResult() { }

        public static BusinessResult<T> Success(T data, string message = "")
            => new BusinessResult<T> { IsSuccess = true, Data = data, Message = message };

        public static BusinessResult<T> Failure(string message)
            => new BusinessResult<T> { IsSuccess = false, Message = message };


        public static BusinessResult<T> Failure(List<string> errors)
        {
            return new BusinessResult<T> { IsSuccess = false, Message = errors.FirstOrDefault() ?? "An unexpected error occurred." };
        }
    }
}

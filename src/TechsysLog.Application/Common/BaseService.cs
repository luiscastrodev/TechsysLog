using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechsysLog.Application.Common
{
    public abstract class BaseService
    {
        public BaseService()
        {
        }

        protected BusinessResult<T> Success<T>(T data, string message = "Sucess")
        {
            return BusinessResult<T>.Success(data, message);
        }

        protected BusinessResult<T> Failure<T>(string error)
        {
            return BusinessResult<T>.Failure(error);
        }

        protected BusinessResult<T> Failure<T>(List<string> errors)
        {
            return BusinessResult<T>.Failure(errors);
        }

    }
}

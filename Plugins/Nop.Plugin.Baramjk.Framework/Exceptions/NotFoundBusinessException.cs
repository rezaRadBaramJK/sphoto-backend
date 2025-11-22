using System;

namespace Nop.Plugin.Baramjk.Framework.Exceptions
{
    public class NotFoundBusinessException : BusinessException
    {
        public NotFoundBusinessException(string message = "Not Found")
            : base(BusinessExceptionType.NotFound, message, null, 0)
        {
        }

        public NotFoundBusinessException(string message = "Not Found", Exception exception = null, int code = 0)
            : base(BusinessExceptionType.NotFound, message, exception, code)
        {
        }

        public static NotFoundBusinessException NotFound<T>()
        {
            throw new NotFoundBusinessException($"{typeof(T).Name} not found.");
        }

        public static NotFoundBusinessException NotFound(string msg)
        {
            throw new NotFoundBusinessException(msg);
        }
    }
}
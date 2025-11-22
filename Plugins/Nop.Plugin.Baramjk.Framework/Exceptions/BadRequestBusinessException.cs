using System;

namespace Nop.Plugin.Baramjk.Framework.Exceptions
{
    public class BadRequestBusinessException : BusinessException
    {
        public BadRequestBusinessException(string message)
            : base(BusinessExceptionType.BadRequest, message, null, 0)
        {
        }

        public BadRequestBusinessException(params string[] messages)
            : base(BusinessExceptionType.BadRequest, null, null, 0)
        {
            SetMessage(messages);
        }

        public BadRequestBusinessException(string message = "BadRequest", Exception exception = null, int code = 0)
            : base(BusinessExceptionType.BadRequest, message, exception, code)
        {
        }
    }
}
using System;

namespace Nop.Plugin.Baramjk.Framework.Exceptions
{
    public class InternalErrorBusinessException : BusinessException
    {
        public InternalErrorBusinessException(Exception exception)
            : base(BusinessExceptionType.InternalError, exception.Message, exception, 0)
        {
        }

        public InternalErrorBusinessException(string message)
            : base(BusinessExceptionType.InternalError, message, null, 0)
        {
        }

        public InternalErrorBusinessException(params string[] messages)
            : base(BusinessExceptionType.InternalError, null, null, 0)
        {
            SetMessage(messages);
        }

        public InternalErrorBusinessException(string message = "InternalError", Exception exception = null,
            int code = 0)
            : base(BusinessExceptionType.InternalError, message, exception, code)
        {
        }
    }
}
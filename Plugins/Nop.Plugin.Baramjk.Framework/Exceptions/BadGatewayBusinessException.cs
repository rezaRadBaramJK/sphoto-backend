using System;

namespace Nop.Plugin.Baramjk.Framework.Exceptions
{
    public class BadGatewayBusinessException : BusinessException
    {
        public BadGatewayBusinessException(string message)
            : base(BusinessExceptionType.BadGateway, message, null, 0)
        {
        }

        public BadGatewayBusinessException(params string[] messages)
            : base(BusinessExceptionType.BadGateway, null, null, 0)
        {
            SetMessage(messages);
        }

        public BadGatewayBusinessException(string message = "BadGateway", Exception exception = null, int code = 0)
            : base(BusinessExceptionType.BadGateway, message, exception, code)
        {
        }
    }
}
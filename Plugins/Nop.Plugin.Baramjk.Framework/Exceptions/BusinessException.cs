using System;

namespace Nop.Plugin.Baramjk.Framework.Exceptions
{
    public class BusinessException : BaramjkExceptionBase
    {
        public BusinessException()
        {
        }

        public BusinessException(BusinessExceptionType type, string message, Exception exception, int code)
            : base(message, exception, code)
        {
            Type = type;
        }

        public BusinessExceptionType Type { get; set; } = BusinessExceptionType.BadRequest;
    }
}
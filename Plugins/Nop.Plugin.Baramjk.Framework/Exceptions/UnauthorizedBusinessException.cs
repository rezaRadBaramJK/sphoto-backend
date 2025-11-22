namespace Nop.Plugin.Baramjk.Framework.Exceptions
{
    public class UnauthorizedBusinessException : BusinessException
    {
        public UnauthorizedBusinessException(string message = "Unauthorized")
            : base(BusinessExceptionType.Unauthorized, message, null, 0)
        {
        }
    }
}
namespace Nop.Plugin.Baramjk.Framework.Exceptions
{
    public class ValidateBusinessException : BadRequestBusinessException
    {
        public ValidateBusinessException(string message) : base(message)
        {
        }
    }
}
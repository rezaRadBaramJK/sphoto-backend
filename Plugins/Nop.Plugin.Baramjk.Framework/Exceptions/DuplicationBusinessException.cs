namespace Nop.Plugin.Baramjk.Framework.Exceptions
{
    public class DuplicationBusinessException : BadRequestBusinessException
    {
        public DuplicationBusinessException(string message) : base(message)
        {
        }
    }
}
namespace Nop.Plugin.Baramjk.Framework.Exceptions
{
    public class InvalidOperationBusinessException : BadRequestBusinessException
    {
        public InvalidOperationBusinessException(string message) : base(message)
        {
        }
    }
}
namespace Nop.Plugin.Baramjk.Framework.Exceptions
{
    public class NullBusinessException : BadRequestBusinessException
    {
        public NullBusinessException(string message) : base(message)
        {
        }

        public static void Check<T>(T t)
        {
            if (t != null)
                return;

            throw new NullBusinessException($"{typeof(T).Name} is null.");
        }
    }
}
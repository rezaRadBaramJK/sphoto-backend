using Nop.Plugin.Baramjk.Framework.Exceptions;

namespace Nop.Plugin.Baramjk.Framework.Extensions
{
    public static class BusinessExceptionEx
    {
        public static void NotFoundException<T>(this T obj, string msg = null)
        {
            if (obj != null)
                return;
            Msg<T>(ref msg);
            throw new NotFoundBusinessException(msg);
        }

        public static void NullException<T>(this T obj, string msg = null)
        {
            if (obj != null)
                return;
            Msg<T>(ref msg);
            throw new NullBusinessException(msg);
        }

        public static void IsNullEmptyException<T>(this T obj, string msg)
        {
            if (obj != null)
                return;

            throw new BadRequestBusinessException(msg);
        }

        private static string Msg<T>(ref string msg)
        {
            if (string.IsNullOrEmpty(msg))
                msg = $"{typeof(T).Name} not found";
            return msg;
        }
    }
}
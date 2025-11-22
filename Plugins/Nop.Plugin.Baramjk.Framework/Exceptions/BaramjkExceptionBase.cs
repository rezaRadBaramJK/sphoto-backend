using System;

namespace Nop.Plugin.Baramjk.Framework.Exceptions
{
    public abstract class BaramjkExceptionBase : ApplicationException
    {
        private string _message;

        protected BaramjkExceptionBase()
        {
        }

        protected BaramjkExceptionBase(string message)
            : base(message)
        {
            _message = message;
        }

        protected BaramjkExceptionBase(string message, Exception exception, int code = 0)
            : base(message, exception)
        {
            HResult = code;
            _message = message;
        }

        public override string Message => _message ?? base.Message;

        protected void SetMessage(string message)
        {
            _message = message;
        }

        protected void SetMessage(params string[] messages)
        {
            if (messages == null || messages.Length < 1)
                return;

            _message = string.Join(Environment.NewLine, messages);
        }
    }
}
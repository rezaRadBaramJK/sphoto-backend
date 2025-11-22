using System;

namespace Nop.Plugin.Baramjk.FrontendApi.Exceptions.Auth
{
    public class IncorrectInfoException: Exception
    {
        
        public IncorrectInfoException(string message) : base(message)
        {
            
        }
    }

    public class EmailAlreadyExistsException : Exception
    {
        public EmailAlreadyExistsException(string message) : base(message)
        {
            
        }
    }
}
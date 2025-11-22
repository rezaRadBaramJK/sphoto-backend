using System;

namespace Nop.Plugin.Baramjk.Framework.Exceptions
{    
    public class ThirdPartyException : BusinessException
     {
         public ThirdPartyException(string message)
             : base(BusinessExceptionType.BadRequest, message, null, 0)
         {
         }
 
         public ThirdPartyException(params string[] messages)
             : base(BusinessExceptionType.BadRequest, null, null, 0)
         {
             SetMessage(messages);
         }
 
         public ThirdPartyException(string message = "BadRequest", Exception exception = null, int code = 0)
             : base(BusinessExceptionType.BadRequest, message, exception, code)
         {
         }
     }
}
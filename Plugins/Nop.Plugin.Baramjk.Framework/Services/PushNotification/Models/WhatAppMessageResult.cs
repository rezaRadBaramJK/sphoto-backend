using System.Collections.Generic;

namespace Nop.Plugin.Baramjk.Framework.Services.PushNotification.Models
{
    public class WhatAppMessageResult
    {
        private WhatAppMessageResult()
        {
            Errors = new List<string>();
        }
        
        public bool IsSuccessful { get; set; }
        
        public List<string> Errors { get; private set; }


        public static WhatAppMessageResult GetSuccessfulResult()
        {
            return new WhatAppMessageResult
            {
                IsSuccessful = true
            };
        }

        public static WhatAppMessageResult GetFailedResult(params string[] errors)
        {
            return new WhatAppMessageResult
            {
                Errors = new List<string>(errors)
            };
        }
    }
}
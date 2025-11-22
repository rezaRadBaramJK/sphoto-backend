using Nop.Core.Configuration;

namespace Nop.Plugin.Baramjk.PushNotification.Plugins
{
    public class PushNotificationSettings : ISettings
    {
        
        public bool DisableNotification { get; set; }
        
        public int Strategy { get; set; }

        public string FireBaseConfig { get; set; }

        public string ServerKey { get; set; }

        public string PrivateKeyConfig { get; set; }
        public string SoundFileName { get; set; }
        
    }
}
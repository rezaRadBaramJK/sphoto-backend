using Newtonsoft.Json.Converters;

namespace Nop.Plugin.Baramjk.Framework.Data.Formats.Json
{
    public class DateFormatConverter : IsoDateTimeConverter
    {
        public DateFormatConverter()
        {
            DateTimeFormat = "yyyy/MM/dd HH:mm:ss";
        }

        public DateFormatConverter(string format)
        {
            DateTimeFormat = format;
        }
    }
}
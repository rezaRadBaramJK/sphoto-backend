using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Nop.Core.Domain.Media;
using Nop.Services.Media;

namespace Nop.Plugin.Baramjk.FrontendApi.Services
{
    public class PictureHelper
    {
        private readonly IPictureService _pictureService;

        public PictureHelper(IPictureService pictureService)
        {
            _pictureService = pictureService;
        }

        public async Task<Picture> GetPictureFromUrlAsync(string url, string seoName)
        {
            if (string.IsNullOrEmpty(url))
                return null;

            var client = new HttpClient();
            var stream = await client.GetStreamAsync(url);
            if (stream.CanRead == false)
                return null;

            await using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var pictureBinary = ms.ToArray();
            ms.Position = 0;

            var image = Image.FromStream(ms);
            var format = image.RawFormat;
            var codec = ImageCodecInfo.GetImageDecoders().First(c => c.FormatID == format.Guid);
            var mimeType = codec.MimeType;

            var picture = await _pictureService.InsertPictureAsync(pictureBinary, mimeType, seoName);
            return picture;
        }
    }
}
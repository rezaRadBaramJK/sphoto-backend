using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using QRCoder;

#pragma warning disable CS1998

namespace Nop.Plugin.Baramjk.Framework.Services.BarCodes
{
    public class BarCodeService : IBarCodeService
    {
        private readonly IHostEnvironment _hostEnvironment;

        public BarCodeService(IHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }

        public async Task<string> CreateCode128Async(string code)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(code, QRCodeGenerator.ECCLevel.Q);
            return Save(qrCodeData);
        }

        public async Task<string> CreateQRCodeAsync(string code)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(code, QRCodeGenerator.ECCLevel.Q);
            return Save(qrCodeData);
        }

        private string Save(QRCodeData qrCodeData)
        {
            using var qrCode = new QRCode(qrCodeData);
            var qrCodeImage = qrCode.GetGraphic(20);

            var path = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", "Barcode");
            Directory.CreateDirectory(path);
            var fileName = DateTime.Now.Ticks + ".jpg";
            var fullPath = Path.Combine(path, fileName);
            qrCodeImage.Save(fullPath);

            return $"/Barcode/{fileName}";
        }
    }
}
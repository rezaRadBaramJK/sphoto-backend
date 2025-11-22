using System;
using Nop.Plugin.Baramjk.FrontendApi.Framework.Dto;

namespace Nop.Plugin.Baramjk.FrontendApi.Dto.ReturnRequests
{
    public class UploadFileResponse : BaseDto
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public string DownloadUrl { get; set; }

        public Guid DownloadGuid { get; set; }
    }
}
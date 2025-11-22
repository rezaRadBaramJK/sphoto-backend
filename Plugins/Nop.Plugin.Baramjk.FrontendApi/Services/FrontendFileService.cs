using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Nop.Plugin.Baramjk.FrontendApi.Services.Models.Files;

namespace Nop.Plugin.Baramjk.FrontendApi.Services
{
    public class FrontendFileService
    {
        private readonly FrontendApiSettings _frontendApiSettings;

        private readonly string[] _supportedFileTypes;

        public FrontendFileService(FrontendApiSettings frontendApiSettings)
        {
            _frontendApiSettings = frontendApiSettings;
            
            _supportedFileTypes = 
                string.IsNullOrEmpty(_frontendApiSettings.UploadFileSupportedTypes) 
                    ? Array.Empty<string>() 
                    : _frontendApiSettings.UploadFileSupportedTypes.Split(',');
        }
        
        public FileValidationResult ValidateFile(IFormFile file)
        {
            var type = file.FileName.Split('.').Last();
            
            if (string.IsNullOrEmpty(type) ||
                _supportedFileTypes.Any(t => t.Equals(type, StringComparison.OrdinalIgnoreCase)) == false)
                return new FileValidationResult
                {
                    Message = "Invalid file type"
                };

            var fileMbSize = file.Length / 1048576.0;
            if (fileMbSize > _frontendApiSettings.UploadFileMaxSize)
                return new FileValidationResult
                {
                    Message = $"Invalid file size. Max file size is {_frontendApiSettings.UploadFileMaxSize}."
                };

            return new FileValidationResult
            {
                IsValid = true
            };
        }
        
    }
}
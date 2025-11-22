namespace Nop.Plugin.Baramjk.FrontendApi.Services.Models.Files
{
    public class FileValidationResult
    {
        public bool IsValid { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
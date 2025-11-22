namespace Nop.Plugin.Baramjk.Framework.Services.License
{
    public interface ILicenseService
    {
        bool IsLicensed(string pluginName);
    }
}
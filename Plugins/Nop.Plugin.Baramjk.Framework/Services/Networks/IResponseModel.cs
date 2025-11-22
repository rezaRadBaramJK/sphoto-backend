namespace Nop.Plugin.Baramjk.Framework.Services.Networks
{
    public interface IResponseModel
    {
        public bool IsSuccess { get; }
        public string Message { get; }
    }

    public interface IGenericResponseModel : IResponseModel
    {
        public bool HasData { get; }
    }
}
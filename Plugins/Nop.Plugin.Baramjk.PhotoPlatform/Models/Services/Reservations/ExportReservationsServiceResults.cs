using Nop.Core.Domain.Catalog;

namespace Nop.Plugin.Baramjk.PhotoPlatform.Models.Services.Reservations
{
    public class ExportReservationsServiceResults
    {
        public string Message { get; set; }
        
        public bool Success { get; set; }
        
        public Product Product { get; set; }
    }
}
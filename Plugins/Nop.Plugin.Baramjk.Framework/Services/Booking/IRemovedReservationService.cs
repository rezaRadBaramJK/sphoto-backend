using System.Threading.Tasks;
using Nop.Plugin.Baramjk.Framework.Services.Booking.Domains;

namespace Nop.Plugin.Baramjk.Framework.Services.Booking
{
    public interface IRemovedReservationService
    {
        Task InsertAsync(Reservation reservation);
        Task<RemovedReservation> GetByOrderIdAndProductIdAsync(int orderId, int productId);
        Task<RemovedReservation> GetByReservationIdAsync(int reservationId);
    }
}
using System.Threading.Tasks;

namespace Nop.Plugin.Baramjk.Framework.Services.Technicians.Abstractions
{
    public interface ITechnicianRejectedReservationService
    {
        Task RemoveAsync(int reservationId, int technicianId)// no need technician id, but for more care we got it
            ;

        Task ClearRejectedReservationAsync(int reservationId);

        Task<bool> IsRejectedReservationExistsAsync(int reservationId, int technicianId)// no need technician id, but for more care we got it
            ;
    }
}
using System.Linq;
using System.Threading.Tasks;
using Nop.Data;
using Nop.Plugin.Baramjk.PhotoPlatform.Domains;


namespace Nop.Plugin.Baramjk.PhotoPlatform.Services
{
    public class ReservationHistoryService
    {
        private readonly IRepository<ReservationHistory> _reservationHistoryRepository;


        public ReservationHistoryService(IRepository<ReservationHistory> reservationHistoryRepository)
        {
            _reservationHistoryRepository = reservationHistoryRepository;
        }

        public Task UpdateAsync(ReservationHistory reservationHistory)
        {
            return _reservationHistoryRepository.UpdateAsync(reservationHistory);
        }

        public Task InsertAsync(ReservationHistory reservationHistory)
        {
            return _reservationHistoryRepository.InsertAsync(reservationHistory);
        }


        public Task<ReservationHistory> GetReservationHistoryAsync(int reservationId, int customerId)
        {
            return _reservationHistoryRepository.Table.Where(rh => rh.ReservationId == reservationId && rh.LastChangedBy == customerId)
                .FirstOrDefaultAsync();
        }
    }
}
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Services.Technicians.Domains;
using Nop.Plugin.Baramjk.Framework.Services.Technicians.Models.Services;

namespace Nop.Plugin.Baramjk.Framework.Services.Technicians.Abstractions
{
    public interface ITechnicianAvailableReservationService
    {
        Task AddAsync(TechnicianAvailableReservation technicianAvailableReservationToAdd);
        Task AddAsync(int reservationId);
        Task<IPagedList<TechnicianReservationServiceResult>> GetAsync(int pageIndex = 0, int pageSize = int.MaxValue);
        Task<TechnicianAvailableReservation> GetByReservationIdAsync(int reservationId);

        /// <exception cref="NopException"></exception>
        Task ApprovalAsync(int reservationId);

        Task RejectAsync(int reservationId);

        Task DeleteAsync(TechnicianAvailableReservation technicianAvailableReservation);
    }
}
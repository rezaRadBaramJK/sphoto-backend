using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Services.Booking.Domains;
using Nop.Plugin.Baramjk.Framework.Services.Technicians.Domains;
using Nop.Plugin.Baramjk.Framework.Services.Technicians.Models;
using Nop.Plugin.Baramjk.Framework.Services.Technicians.Models.Services;
using Nop.Plugin.Baramjk.Framework.Services.Technicians.Models.Types;

namespace Nop.Plugin.Baramjk.Framework.Services.Technicians.Abstractions
{
    public interface ITechnicianReservationService
    {
        IPagedList<TechnicianRelReservation> GetAll(int pageIndex = 0, int pageSize = int.MaxValue);
        
        Task<TechnicianRelReservation> GetById(int technicianOrderRecordId);
        
        TechnicianRelReservation GetByReservationId(int reservationId);
        
        Task<TechnicianRelReservation>  GetByReservationIdAsync(int reservationId);
        
        Task<TechnicianRelReservation>  GetByReservationIdAsync(int reservationId, int technicianId);
        
        Task InsertAsync(Reservation reservation, int technicianId);
        
        Task UpdateTechnicianOrderRecord(TechnicianRelReservation technicianRelReservationRecord);
        
        Task DeleteAsync(TechnicianRelReservation technicianRelReservationRecord);

        Task DeleteAsync(int reservationId, int technicianId);

        /// <exception cref="NopException"></exception>
        Task SetTechnicianReservation(SetTechnicianReservationRequest request);
        
        Task SetReservationReservationProcessAsync(int reservationId, TechnicianReservationProcessType technicianReservationProcess);
        Task SetReservationReservationResultAsync(int reservationId, TechnicianReservationResultType reservationResult);

        Task SetReservationCompletedAsync(int reservationId, TechnicianReservationResultType result, string note, List<int> pictureIds);

        /// <exception cref="NopException"></exception>
        Task<List<ReservationServiceResult>> GetReservationsAsync();

        Task<Technician> GetByCustomerIdAsync(int customerId);

        Task<IPagedList<TechnicianReservationServiceResult>> GetCompletedReservationsAsync(int pageNumber = 1, int pageSize = int.MaxValue);

        Task<CompletedReservationServiceResults> GetCompletedReservationAsync(int reservationId);

        Task<IList<TechnicianReservationBetween>> GetTechnicianReservationBetweenAsync(DateTime fromDate, DateTime toDate);

        Task<IList<Reservation>> GetReservationsByOrderIdAsync(int orderId);

        Task<bool> IsTechnicianAvailableAtTimeAsync(int technicianId, DateTime dateTime);

        Task OnReservationUpdateAsync(Reservation reservation, int storeId, TimeSlot oldTimeSlot, DateTime oldDate);
        
        Task HandelReservationTechnicianAsync(Reservation reservation, int storeId);
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Baramjk.Framework.Services.Booking.Domains;
using Nop.Plugin.Baramjk.Framework.Services.Technicians.Domains;

namespace Nop.Plugin.Baramjk.Framework.Services.Technicians.Abstractions
{
    public interface ITechnicianService
    {
        /// <summary>
        ///     Get all technician records
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of the technician record</returns>
        Task<IPagedList<Technician>> GetAllAsync(int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        ///     Get a technician record by identifier
        /// </summary>
        /// <param name="technicianRecordId">Record identifier</param>
        /// <returns>technician record</returns>
        Task<Technician> GetByIdAsync(int technicianRecordId);

        /// <summary>
        ///     Insert the technician record
        /// </summary>
        /// <param name="technicianRecord">technician record</param>
        Task InsertAsync(Technician technicianRecord);

        /// <summary>
        ///     Update the technician record
        /// </summary>
        /// <param name="technicianRecord">technician record</param>
        Task UpdateAsync(Technician technicianRecord);

        /// <summary>
        ///     Delete the technician record
        /// </summary>
        /// <param name="technicianRecord">technician record</param>
        Task DeleteAsync(Technician technicianRecord);
        
        Task<List<Technician>> GetAvailableAsync(Reservation selectedReservation);
        Task RegisterAsync(Technician technicianToRegister);
        Task<Technician> GetByCustomerIdAsync(int customerId);
        Task OnRemoveVendorAsync(int vendorId);

        Task<IList<Technician>> GetTechniciansByVendorIdAsync(int vendorId);

        Task<int> GetAvailableTechnicianCountByDateAsync(DateTime dateTime);

        Task<bool> IsTechnicianAvailableAsync(Technician technician, DateTime dateTime);

    }
}
using Nop.Plugin.Baramjk.Framework.Services.Technicians.Domains;

namespace Nop.Plugin.Baramjk.Framework.Services.Technicians.Models.Services
{
    public class TimeSlotTechnicianResult
    {
        public int TimeSlotId { get; set; }
        
        public Technician Technician { get; set; }
    }
}
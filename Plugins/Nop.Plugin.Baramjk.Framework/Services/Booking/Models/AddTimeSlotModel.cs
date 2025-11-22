using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Baramjk.Framework.Services.Booking.Models
{
    public class AddTimeSlotModel 
    {

        public AddTimeSlotModel()
        {
            DependTimeSlotIds = new List<int>();
        }
        public int Id { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        [DisplayName("Product")]
        public int ProductId { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        [DisplayName("Day")]
        public DayOfWeek DayOfWeek { get; set; }

        [Required]
        [MinLength(5)]
        public string StartTime { get; set; }

        [Required]
        [MinLength(5)]
        public string EndTime { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public double Cost { get; set; }

        [Required]
        public string Color { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public string TypeName { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Booking.Admin.TimeSlots.DependTimeSlotIds")]
        public IList<int> DependTimeSlotIds { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Baramjk.Booking.Admin.TimeSlots.IsDepend")]
        public bool IsDepend { get; set; }
        
    }
}
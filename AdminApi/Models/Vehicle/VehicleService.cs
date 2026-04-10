using System;
using System.ComponentModel.DataAnnotations;

namespace AdminApi.Models.Vehicle
{
    public class VehicleService
    {
        [Key]
        public int VehicleServiceId { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [StringLength(200)]
        public string? ServiceCompany { get; set; }

        [StringLength(20)]
        public string? ServiceDate { get; set; } // dd.MM.yyyy

        public decimal? Cost { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public int? StaffUserId { get; set; }

        public int? DailyReportEntryId { get; set; }

        [Required]
        public int AddedBy { get; set; }

        [Required]
        public DateTime DateAdded { get; set; }
    }
}

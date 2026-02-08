using System;
using System.ComponentModel.DataAnnotations;

namespace AdminApi.Models.Vehicle
{
    /// <summary>
    /// Placeholder model for future Vehicle Services (Serviset e Automjeteve).
    /// Fields and business logic to be defined.
    /// </summary>
    public class VehicleService
    {
        [Key]
        public int VehicleServiceId { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [StringLength(20)]
        public string? ServiceDate { get; set; } // dd.MM.yyyy

        [StringLength(500)]
        public string? Description { get; set; }

        public decimal? Cost { get; set; }

        [Required]
        public int AddedBy { get; set; }

        [Required]
        public DateTime DateAdded { get; set; }
    }
}

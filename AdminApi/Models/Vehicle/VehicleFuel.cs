using System;
using System.ComponentModel.DataAnnotations;

namespace AdminApi.Models.Vehicle
{
    public class VehicleFuel
    {
        [Key]
        public int VehicleFuelId { get; set; }

        [Required]
        [StringLength(20)]
        public string FillDate { get; set; } = string.Empty; // Data e mbushjes – dd.MM.yyyy

        [Required]
        public int VehicleId { get; set; } // FK to Vehicles

        [Required]
        public decimal FuelAmount { get; set; } // Shuma e derivates

        [Required]
        [StringLength(20)]
        public string FuelType { get; set; } = string.Empty; // Diesel or Benzin

        [Required]
        public int StaffUserId { get; set; } // FK to Users

        [Required]
        public int AddedBy { get; set; }

        [Required]
        public DateTime DateAdded { get; set; }
    }
}

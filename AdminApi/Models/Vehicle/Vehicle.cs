using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AdminApi.Models.Vehicle
{
    public class Vehicle
    {
        [Key]
        public int VehicleId { get; set; }

        [Required]
        [StringLength(20)]
        public string PlateNumber { get; set; } = string.Empty; // Nr. i Tabelave

        [StringLength(50)]
        public string? ChassisNumber { get; set; } // Nr. i Shasis

        [StringLength(30)]
        public string? Color { get; set; } // Ngjyra

        [StringLength(50)]
        public string? Type { get; set; } // Tipi

        [StringLength(50)]
        public string? Brand { get; set; } // Marka

        [StringLength(20)]
        public string? RegistrationDate { get; set; } // Data e regjistrimit – dd.MM.yyyy

        [StringLength(20)]
        public string? ExpiryDate { get; set; } // Data e skadimit – dd.MM.yyyy

        [StringLength(50)]
        public string? CertificateNumber { get; set; } // Nr. i Atestit

        [Required]
        public int AddedBy { get; set; }

        [Required]
        public DateTime DateAdded { get; set; }

        [DefaultValue(true)]
        public bool IsActive { get; set; } = true;

        [DefaultValue(false)]
        public bool IsMigrationData { get; set; }

        public DateTime? LastUpdatedDate { get; set; }
        public int? LastUpdatedBy { get; set; }
    }
}

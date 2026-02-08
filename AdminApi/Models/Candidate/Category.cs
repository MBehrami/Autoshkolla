using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AdminApi.Models.Candidate
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        [Required]
        [StringLength(10)]
        public string? CategoryName { get; set; } // A1, A2, A, B, B+E, C1, C
        [StringLength(500)]
        public string? Description { get; set; }
        [Required]
        public bool IsActive { get; set; }
        [Required]
        public int AddedBy { get; set; }
        [Required]
        public DateTime DateAdded { get; set; }
        [DefaultValue(false)]
        public bool IsMigrationData { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public int? LastUpdatedBy { get; set; }
    }
}

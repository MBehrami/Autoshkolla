using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AdminApi.Models.User
{
    public class Users
    {
		public int UserId { get; set; }
		[Required]
		public int UserRoleId { get; set; }
		[Required]
		[StringLength(100)]
		public string? FullName { get; set; }
		[Required]
		[StringLength(100)]
		public required string Email { get; set; }
		[Required]
		[StringLength(100)]
		public required string Password { get; set; }
		public string? PasswordSalt { get; set; }
		[StringLength(100)]
		public string? Mobile { get; set; }
		public string? DateOfBirth { get; set; }
		[StringLength(500)]
		public string? ImagePath { get; set; }
		[StringLength(200)]
		public string? ForgetPasswordRef { get; set; }
		[Required]
		public bool IsActive { get; set; }
		[Required]
		public int AddedBy { get; set; }
		[DefaultValue(false)]
		public bool IsMigrationData { get; set; }
		[Required]
		public DateTime DateAdded { get; set; }
		public DateTime? LastPasswordChangeDate { get; set; }
		public int? PasswordChangedBy { get; set; }
		[Required]
		public bool IsPasswordChange { get; set; }
		public DateTime? LastUpdatedDate { get; set; }
		public int? LastUpdatedBy { get; set; }
	}
}

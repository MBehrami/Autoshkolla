using System;
using System.ComponentModel.DataAnnotations;

namespace AdminApi.Models.Others
{
    public class Contacts
    {
        [Key]
        public int ContactId{get;set;}
        [Required]
		[StringLength(200)]
        public required string Name{get;set;}
        [Required]
		[StringLength(200)]
        public required string Email{get;set;}
        [Required]
        public required string Message{get;set;}
		[Required]
		public DateTime DateAdded { get; set; }
    }
}
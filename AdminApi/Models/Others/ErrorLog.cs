using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AdminApi.Models.Others
{
    public class ErrorLog
    {
        public int ErrorLogId{get;set;}
        public int? Status{get;set;}
        [Required]
		[StringLength(20)]
        public string? StatusText{get;set;}
        [Required]
		[StringLength(1000)]
        public string? Url{get;set;}
        [StringLength(2000)]
        public string? Message{get;set;}
		public int? AddedBy { get; set; }
		[Required]
		public DateTime DateAdded { get; set; }
    }
}
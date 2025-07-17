using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdminApi.Models.User
{
    public class Browse
    {	
		public int UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
		public string? LogInTime { get; set; }
		public string? LogOutTime { get; set; }
		public string? Ip { get; set; }
		public string? Browser { get; set; }
		public string? BrowserVersion { get; set; }
		public string? Platform { get; set; }
	}
}

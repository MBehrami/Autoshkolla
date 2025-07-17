using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminApi.Models.Helper
{
    public class Confirmation
    {
        public string? Status { get; set; }
        public string? ResponseMsg { get; set; }
    }

    public class Response
    {
        public required string token { get; set; }
        public object? Obj{get;set;}
    }
    public class DropDownHelper
    {
        public string id { get; set; }
        public string? text { get; set; }   
    }
}

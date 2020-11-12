using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NeproWebApi.Models
{
    public class EndPick
    {
        public string UserId { get; set; }
        public string LPID { get; set; }
        public int EndPickedReason { get; set; }
    }
    public class EndPickResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }


    }
}
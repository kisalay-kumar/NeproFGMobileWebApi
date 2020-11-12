using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NeproWebApi.Models
{
    public class CancelLoading
    {
        public string UserId { get; set; }
        public string LPID { get; set; }
    }
    public class CancelLoadingkResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }


    }
}
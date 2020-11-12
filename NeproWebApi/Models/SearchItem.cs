using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NeproWebApi.Models
{
    public class SearchItemRequest
    {
            public string ItemName { get; set; }
            public string WorkOrderNo { get; set; }
            public string UserId { get; set; }
    }
        public class SearchItemResponse
    {
            public string ItemName { get; set; }
            public string WorkOrderNo { get; set; }
            public string ItemId { get; set; }  
            public string Status { get; set; }
            public string Message { get; set; }


        }
    }

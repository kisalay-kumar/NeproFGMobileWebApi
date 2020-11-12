using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NeproWebApi.Models
{
    public class GetWorkOrder
    {
        public class GetOrderRequest
        {
            public string UserId { get; set; }
            public string WorkOrderNo { get; set; }
        }
        public class GetOrderRespose
        {
           
            public string WorkOrderNo { get; set; }
            public string WorkOrderId { get; set; }
            public string Status { get; set; }
            public string Message { get; set; }
        }
    }
}
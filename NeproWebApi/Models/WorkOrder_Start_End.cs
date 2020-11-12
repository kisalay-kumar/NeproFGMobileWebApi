using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NeproWebApi.Models
{
    public class WorkOrder_Start_End
    {
        public class WorkOrder_Start_EndREQ
        {
            public string WorkOrderNo { get; set; }
            public string UserId { get; set; }
            public string AutoPick { get; set; }
            public string AutoRoute { get; set; }
            public string StartQty { get; set; }
        }
        public class WorkOrder_Start_EndRES
        {
            
                 public byte IsFinancialEnd { get; set; }
            public string WorkOrderNo { get; set; }
            public string ItemId { get; set; }
            public string ItemName { get; set; }
            public string ItemDescription { get; set; }
            public string ProductionLine { get; set; }
            public string WareHouse { get; set; }
            public string Quantity { get; set; }
            public string Site { get; set; }
            public string WOStatus { get; set; }
            public string StatusId { get; set; }
            public string BalanceQuantity { get; set; }
            public string RafQuantity { get; set; }
            public string StartedQty { get; set; }
            public string WareHouseId { get; set; }
            public string SiteID { get; set; }
            public string Status { get; set; }
            public string Message { get; set; }



        }
        public class WorkOrderStartEndRES
        {
            public string Status { get; set; }
            public string Message { get; set; }

        }
    }
}
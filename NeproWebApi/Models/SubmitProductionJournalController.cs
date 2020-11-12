using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NeproWebApi.Models
{
    public class ProductionJournalRequest
    {
            public string UserId { get; set; }
            public string WorkOrderNo { get; set; }       
    }
    public class ProductionJournalResponse
    {
        public List<PickingList> PickingListData { get; set; }
        public List<RoutingCardList> RoutingListData { get; set; }
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public string Quantity { get; set; }
        public string WorkOrderNo { get; set; }
        public string WareHouseId { get; set; }
        public string SiteID { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }
    public class PickingList
    {
        //public string Shift { get; set; }
        //public Decimal QTY { get; set; }
        public string ItemId { get; set; }
        //public string Date { get; set; }
        public string ItemName { get; set; }
        public string Unit { get; set; }

    }
    public class RoutingCardList
    {
        public string OperationName { get; set; }
        public string Resource { get; set; }
        public string ResourceType { get; set; }
        public string OperationId { get; set; }
        public string Priority { get; set; }

    }

    public class SubmitProductionJournalRequest
    {
        public List<PickingListData> PickingList { get; set; }
        public List<RoutingCardListData> RoutingCardList { get; set; }
        public string ItemId { get; set; }
        public string UserId { get; set; }
        public string WorkOrderNo { get; set; }
        
    }
    public class SubmitProductionJournalResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
    }

    public class PickingListData
    {
        public string Shift { get; set; }
        public Decimal Quantity { get; set; }
        public string ItemId { get; set; }
        public string Date { get; set; }
        public string ItemName { get; set; }
        public string Unit { get; set; }

    }
    public class RoutingCardListData
    {
        public string OperationName { get; set; }
        public string Resource { get; set; }
        public string ResourceType { get; set; }
        public int OperationId { get; set; }
        public string Shift { get; set; }
        public Decimal Quantity { get; set; }
        public int Hours { get; set; }
        public Decimal Date { get; set; }
        public string Priority { get; set; }

    }



}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NeproWebApi.Models
{
    public class PickAndLoadReq
    {
        public string StickerNo { get; set; }
        public string ItemId { get; set; }
        public string Reason { get; set; }
        public string UserId { get; set; }
        public Decimal LoadQuantity { get; set; }
        public string LoadingId { get; set; }
        public string Iscompleted { get; set; }        public string ActivityID { get; set; }        public string EndPickedReason { get; set; }

    }
    public class STDetailResponce
    {
        public string Status { get; set; }
        public string Message { get; set; }


    }
}
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Web.Http;
using NeproWebApi.Models;
using Newtonsoft.Json;

namespace FRD_InventoryWebApi.Controllers
{
    public class CountController : ApiController
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Conn"].ConnectionString);
        string query = ""; SqlCommand dbcommand;
        //    //    ServiceReference1.CallContext Cct = new ServiceReference1.CallContext();
        //    //    ServiceReference1.AMY_FRDServiceClient obj = new ServiceReference1.AMY_FRDServiceClient();

        [Route("api/Nepro/UpdateRAF")]
        [HttpPost]
        public GetrequestNo UpdateRAF(RAFDetailsReq RAFD)
        {

            GetrequestNo res = new GetrequestNo();
            //        List<GetrequestNoRes> ListView = new List<GetrequestNoRes>();
            try
            {
                if (RAFD.StickerNo == "" || RAFD.StickerNo == null )
                {
                    res.Status = "Failure";
                    res.Message = "Enter Sticker ID";
                }
                if (RAFD.Shift == "" || RAFD.Shift ==null )
                {
                    res.Status = "Failure";
                    res.Message = "Enter Shift";
                }
                //if (RAFD.WorkOrderNo == "" || RAFD.WorkOrderNo == null)
                //{
                //    res.Status = "Failure";
                //    res.Message = "Invalid WorkOrderNo";
                //}


                bool AutoPicked = true;
                bool AutoRoute = true;
                if (RAFD.AutoPick == 1)
                {
                    AutoPicked = true;
                }
                else
                {
                    AutoPicked = false;
                }
                if (RAFD.AutoRoute == 1)
                {
                    AutoRoute = true;
                }
                else
                {
                    AutoRoute = false;
                }
                RAFD.ActivityID = 6;
                query = "Sp_Counting";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.CommandTimeout = 0;
                dbcommand.Parameters.AddWithValue("@QueryType", "FetchRafData");
                dbcommand.Parameters.AddWithValue("@StillageID", RAFD.StickerNo);
                dbcommand.Parameters.AddWithValue("@UserId", RAFD.UserId);
                dbcommand.Parameters.AddWithValue("@ActivityID", RAFD.ActivityID);
                SqlDataAdapter daGetData = new SqlDataAdapter(dbcommand);
                DataSet dsGetData = new DataSet();
                daGetData.Fill(dsGetData);
                SqlDataAdapter da = new SqlDataAdapter(dbcommand);

                var Hold = "";
                if (dsGetData.Tables[0].Rows[0]["isHold"].ToString() == "True")
                {
                    Hold = "Qc Hold";
                }
                else
                {
                    Hold = "Qc Release";
                }

                if (Convert.ToString(dsGetData.Tables[1].Rows[0]["WorkOrderNo"]) == "NA")
                {
                    res.Status = "Failure";
                    res.Message = "This operation doesn't process this stillage sticker ";
                    return res;
                }

                if (dsGetData.Tables[0].Rows[0]["value"].ToString()=="3")
                {
                    res.Status = "Failure";
                    res.Message = "This stillage is already counted";
                    return res;
                }
                else if (dsGetData.Tables[0].Rows[0]["value"].ToString() != "2")
                {

                    query = "Sp_AxWebserviceIntegration";
                    dbcommand = new SqlCommand(query, conn);
                    //dbcommand.Connection.Open();
                    dbcommand.CommandType = CommandType.StoredProcedure;
                    dbcommand.CommandTimeout = 0;
                    da = new SqlDataAdapter(dbcommand);
                    DataSet ds = new DataSet();
                    da.Fill(ds);

                    //  DataSet ds = CommonManger.FillDatasetWithParam("Sp_AxWebserviceIntegration");
                    NeproWebApi.AXWebServiceRef1.Iace_FinishedGoodServiceClient obj = new NeproWebApi.AXWebServiceRef1.Iace_FinishedGoodServiceClient();
                    obj.ClientCredentials.Windows.ClientCredential.Domain = Convert.ToString(ds.Tables[0].Rows[0]["Domain"]);
                    obj.ClientCredentials.Windows.ClientCredential.UserName = Convert.ToString(ds.Tables[0].Rows[0]["Username"]);
                    obj.ClientCredentials.Windows.ClientCredential.Password = Convert.ToString(ds.Tables[0].Rows[0]["Password"]);

                    NeproWebApi.AXWebServiceRef1.CallContext Cct = new NeproWebApi.AXWebServiceRef1.CallContext();
                    Cct.Company = Convert.ToString(ds.Tables[0].Rows[0]["Company"]);
                    Cct.Language = Convert.ToString(ds.Tables[0].Rows[0]["Language"]);

                    string value2 = obj.ReportAsFinished(Cct, Convert.ToString(dsGetData.Tables[1].Rows[0]["WorkOrderNo"]), AutoPicked, AutoRoute, RAFD.Shift, RAFD.StickerNo, "HHD", false,false,false);
                    //string value2 = obj.UpdateQty(Cct, Convert.ToString(dsGetData.Tables[1].Rows[0]["WorkOrderNo"]), AutoPicked, AutoRoute, RAFD.Shift, RAFD.StickerNo, "HDD",150);
                    string value = obj.InsertHistoryHeaderData(Cct, RAFD.StickerNo, Convert.ToString(dsGetData.Tables[1].Rows[0]["SiteID"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["WorkOrderNo"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["ItemId"]), Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["WorkOrderQty"]));
                    obj.InsertHistoryDetailData(Cct, RAFD.StickerNo, "", Convert.ToString(dsGetData.Tables[0].Rows[0]["ActivityName"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ActivityDesc"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["StillageLocation"]), "", "", "", "", "", "", "Yes", 0, "", 0, Convert.ToDecimal(dsGetData.Tables[0].Rows[0]["StillageQty"]), Convert.ToDecimal(dsGetData.Tables[0].Rows[0]["StillageQty"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["UserName"]), Hold, 0, "","","", Convert.ToString(dsGetData.Tables[0].Rows[0]["WareHouseID"]),0);

                    // DataTable dtItems = (DataTable)JsonConvert.DeserializeObject(Xmlitems, (typeof(DataTable)));
                }
                else
                {
                    res.Status = "Failure";
                    res.Message = "This stillage does not exist";
                    return res;
                }

                query = "Sp_MovingWebApi";
                dbcommand = new SqlCommand(query, conn);
                //dbcommand.Connection.Open();
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.Parameters.AddWithValue("@QueryType", "UpdateRAF"); 
                //dbcommand.Parameters.AddWithValue("@WorkOrderNo", RAFD.WorkOrderNo);
                dbcommand.Parameters.AddWithValue("@StickerId", RAFD.StickerNo);
                dbcommand.Parameters.AddWithValue("@UserId", RAFD.UserId);
                dbcommand.Parameters.AddWithValue("@Shift", RAFD.Shift);
                dbcommand.Parameters.AddWithValue("@Quantity", RAFD.Quantity);
                dbcommand.Parameters.AddWithValue("@AutoPick", RAFD.AutoPick);
                dbcommand.Parameters.AddWithValue("@AutoRoute", RAFD.AutoRoute);
                dbcommand.CommandTimeout = 0;
                da = new SqlDataAdapter(dbcommand);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows[0]["value"].ToString() == "1")
                {
                    res.Status = "Success"; 
                    res.Message = "RAF Posted successfully";
                    return res;
                }
                else
                {
                    res.Status = "Failure";
                    res.Message = "RAF Not Posted successfully";
                    return res;
                }
            }
            catch (Exception Ex)
            {
                res.Status = "Failure";
                res.Message = Ex.Message;
            }
            finally
            {
                dbcommand.Connection.Close();
            }
            return res;
        }


      
    }
        
        public class RAFDetailsReq
        {
        public string StickerNo { get; set; }
            public Int64 UserId { get; set; }
            public string Shift { get; set; }
            public decimal Quantity { get; set; }
        public int AutoPick { get; set; }
        public int AutoRoute { get; set; }
        public int ActivityID { get; set; }
    }
    //public class GetDataSeqWiseResPonce
    //{
    //    public string Status { get; set; }
    //    public string Message { get; set; }
    //    public string VendorId { get; set; }
    //    public string PONumber { get; set; }
    //    public string ItemId { get; set; }
    //    public string ItemName { get; set; }
    //    public string ItemnameArabic { get; set; }
    //    public string Expdate { get; set; }
    //    public string BatchId { get; set; }
    //    public string Config { get; set; }
    //    public string VendorName { get; set; }
    //    public string StickerQty { get; set; }
    //}

    public class GetrequestNo
    {
        public string Status { get; set; }
        public string Message { get; set; }

    }
    public class GetrequestNoRes
    {
        public string RequisitionNo { get; set; }
        public string LocationId { get; set; }
        public string WareHouseName { get; set; }


    }
    public class ShipingRequest
    {
        public string RequisitionNo { get; set; }
        public string FromLocation { get; set; }
        public string ToLocation { get; set; }
        public string WebUser { get; set; }
        public List<ShipingRequestItem> ItemList
        { get; set; }


    }
    public class ShipingRequestItem
    {
       
        public string ItemId { get; set; }
        public string PickedQty { get; set; }
        public string Reason { get; set; }
        public string RemainingQty { get; set; }
        public string Config { get; set; }
        public string BatchId { get; set; }

    }
    public class ShipingRequestItemList
    {

        public string ItemId { get; set; }
        public string PickedQty { get; set; }
        public string Reason { get; set; }
        public string RemainingQty { get; set; }
        public string BatchNo { get; set; }
      

    }


    public class ShipingResponse
    {

        public string Status { get; set; }
        public string Message { get; set; }
        public string TONumber { get; set; }
       
    }


    public class requestControl
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public List<RequestControlReq> dataList
        { get; set; }

    }

    public class RequestControlReq
    {
        public string RequisitionNo { get; set; }
        public string ReqestDate { get; set; }
        public string CreatedBy { get; set; }
        public string Branch { get; set; }
        public string ItemId { get; set; }
        public string ItemNameArabic { get; set; }
        public string ItemName { get; set; }
        public string ApprovedQty { get; set; }
        
      

    }
    public class ReqUserId
    {
        public string UserId { get; set; }
        public string ReqNo { get; set; }
    }

    public class RequestControlResponse
    {
       
        public string RequisitionNo { get; set; }
        public string Location { get; set; }
        public string ReqestDate { get; set; }
        public string CreatedByWH { get; set; }
        public string Branch { get; set; }
        public string ItemNameArabic { get; set; }
        public string ItemNameEnglish { get; set; }
        public string ApprovedQty { get; set; }
        public string PickedQty { get; set; }
        public string RemainingQty { get; set; }
    }
}

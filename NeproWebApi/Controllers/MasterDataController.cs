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
//using FRD_InventoryWebApi.ServiceReference1;
using NeproWebApi.Models;
using System.Text.RegularExpressions;

namespace FRD_InventoryWebApi.Controllers
{
    public class MasterDataController : ApiController
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Conn"].ConnectionString);
        string query = ""; SqlCommand dbcommand;

        [Route("api/Nepro/MasterData")]
        [HttpPost]
        public AllMasterData GetMasterData()
        {

            AllMasterData res = new AllMasterData();
            try
            {


                query = "Sp_MasterDataWebApi";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.Connection.Open();
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.Parameters.AddWithValue("@QueryType", "GetMasterData");

                SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                DataSet dt = new DataSet();
                da.Fill(dt);

                List<SiteList> SiteList = new List<SiteList>();
                foreach (DataRow row in dt.Tables[7].Rows)
                {
                    SiteList A = new SiteList();
                    A.id = row["id"].ToString();
                    A.name = row["name"].ToString();
                    SiteList.Add(A);
                }
                res.SiteListData = SiteList;

                List<Aisle> AisleList = new List<Aisle>();
      
                foreach (DataRow row in dt.Tables[0].Rows)
                {
                    Aisle A = new Aisle();
                    A.id = Convert.ToInt64(row["id"]);
                    A.name = row["name"].ToString();
                    AisleList.Add(A);
                }

                List<Rack> RackList = new List<Rack>();
      
                foreach (DataRow row in dt.Tables[1].Rows)
                {
                    Rack R = new Rack();
                    R.id = Convert.ToInt64(row["id"]);
                    R.name = row["name"].ToString();
                    RackList.Add(R);
                }


               List<Bin> BinList = new List<Bin>();
                foreach (DataRow row in dt.Tables[2].Rows)
                {
                    Bin A = new Bin();
                    A.id = Convert.ToInt64(row["id"]);
                    A.name = row["name"].ToString();
                    BinList.Add(A);
                }



                 List<Reason> ReasonList = new List<Reason>();
                foreach (DataRow row in dt.Tables[3].Rows)
                {
                    Reason A = new Reason();
                    A.id = Convert.ToInt64(row["id"]);
                    A.name = row["name"].ToString();
                    ReasonList.Add(A);
                }

                 List<FLTList> FLTList = new List<FLTList>();
                foreach (DataRow row in dt.Tables[4].Rows)
                {
                    FLTList A = new FLTList();
                    A.id = Convert.ToInt64(row["id"]);
                    A.name = row["name"].ToString();
                    FLTList.Add(A);
                }
                List<WareHouseList> WareHouseList = new List<WareHouseList>();
                foreach (DataRow row in dt.Tables[5].Rows)
                {
                    WareHouseList A = new WareHouseList();
                    A.id = row["id"].ToString(); ;
                    A.name = row["name"].ToString();
                    WareHouseList.Add(A);
                }

                res.AisleList = AisleList;
                res.RackList = RackList;
                res.BinList = BinList;
                res.ReasonList = ReasonList;
                res.FLTList = FLTList;
                res.WareHouseList = WareHouseList;
                res.Status = "Success";
                res.Message = "Data retrived successfully";



            }
            catch (Exception ex)
            {
                res.Status = "Failure";
                res.Message = ex.Message;

            }
            finally
            {
                dbcommand.Connection.Close();
            }
            return res;
        }

        [Route("api/Nepro/ScanStillageCount")]
        [HttpPost]
        public StillageSticker ScanStillageCount(StickerReq StillReq)
        {
            StillageSticker SM = new StillageSticker();
            try
            {
                
                    query = "Sp_MasterDataWebApi";
                    dbcommand = new SqlCommand(query, conn);
                    dbcommand.Connection.Open();
                    dbcommand.CommandType = CommandType.StoredProcedure;
                    dbcommand.Parameters.AddWithValue("@QueryType", "GetScanStillageCount");
                    dbcommand.Parameters.AddWithValue("@StickerId", StillReq.StickerNo);
                    dbcommand.Parameters.AddWithValue("@UserId", StillReq.UserId);
                    SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                if (dt.Rows[0]["value"].ToString() == "3")
                    {
                    SM.Status = "Failure";
                    SM.Message = "This Stillage Already RAF";
                    return SM;

                }

                if (dt.Rows[0]["value"].ToString() == "4")
                {
                    SM.Status = "Failure";
                    SM.Message = "This Stillage is in QC Hold..";
                    return SM;

                }

                if (dt.Rows[0]["value"].ToString() == "5")
                {
                    SM.Status = "Failure";
                    SM.Message = "This is rejected stillage..";
                    return SM;

                }

                if (dt.Rows[0]["value"].ToString() != "2")
                {

                    if(dt != null && dt.Rows.Count > 0)
                    {
                        SM.StickerID = dt.Rows[0]["StickerID"].ToString();
                        SM.StandardQty = Convert.ToDecimal(dt.Rows[0]["StandardQty"]);
                        SM.ItemId = dt.Rows[0]["ItemId"].ToString();
                        SM.WorkOrderNo = dt.Rows[0]["WorkOrderNo"].ToString();
                        SM.Description = dt.Rows[0]["Description"].ToString();
                        SM.ItemStdQty = Convert.ToDecimal(dt.Rows[0]["ItemStdQty"]);
                        SM.SiteID = dt.Rows[0]["SiteID"].ToString();
                        SM.WareHouseID= dt.Rows[0]["WareHouseID"].ToString();
                        SM.Status = "Success";
                        SM.Message = "Data retrived successfully";
                    }
                    
                    else
                    {
                        SM.Status = "Failure";
                        SM.Message = "Stillage Id Invalid";
                    }
                }
                else
                {
                    SM.Status = "Failure";
                    SM.Message = "This stillage does not exist";
                }
            }
            catch(Exception Ex)
            {
                SM.Status = "Failure";
                SM.Message = Ex.Message;
            }
            finally
            {
                dbcommand.Connection.Close();
            }
            return SM;
        }

        [Route("api/Nepro/GetLocation")]
        [HttpPost]
        public LocationResponse GetLocationData(LocationReq Location)
        {

            LocationResponse res = new LocationResponse();
            try
            {
                query = "Sp_MasterDataWebApi";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.Connection.Open();
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.Parameters.AddWithValue("@QueryType", "GetLocationData");
                dbcommand.Parameters.AddWithValue("@LocationId", Location.LocationId);
                SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt != null && dt.Rows.Count > 0)
                {
                    res.SiteId = dt.Rows[0]["SiteId"].ToString();
                    res.SiteName = dt.Rows[0]["SiteName"].ToString();
                    res.WarehouseId = dt.Rows[0]["WarehouseId"].ToString();
                    res.WarehouseName = dt.Rows[0]["WarehouseName"].ToString();
                    res.ZoneId = dt.Rows[0]["ZoneId"].ToString();
                    res.ZoneName = dt.Rows[0]["ZoneName"].ToString();
                    res.Status = "Success";
                    res.Message = "Data retrived successfully";
                }
                else
                {
                    res.Status = "Failure";
                    res.Message = "Data not found";
                }
             
            }
            catch (Exception ex)
            {
                res.Status = "Failure";
                res.Message = ex.Message;
            }
                return res;

        }
    }


    public class StickerMaster
    {
        public List<StillageSticker> StillageList { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }

    }
    public class StillageSticker
    {
        public string StickerID { get; set; }
        public decimal StandardQty { get; set; }
        public string ItemId { get; set; }
        public string WorkOrderNo { get; set; }
        public string Description { get; set; }
        public decimal ItemStdQty { get; set; }
        public byte isHold { get; set; }
        public string SiteID { get; set; }
        public string WareHouseID { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }
    public class LocationReq
    {
        public string LocationId { get; set; }
    }
    public class StickerReq
    {
        public string StickerNo { get; set; }
        public Int64 UserId { get; set; }
    }
    public class Aisle
    {
        public Int64 id { get; set; }
        public string name { get; set; }
    }
    public class Rack
    {
        public Int64 id { get; set; }
        public string name { get; set; }
    }
    public class Bin
    {
        public Int64 id { get; set; }
        public string name { get; set; }
    }
    public class Reason
    {
        public Int64 id { get; set; }
        public string name { get; set; }
    }
    public class FLTList
    {
        public Int64 id { get; set; }
        public string name { get; set; }
    }
    public class WareHouseList
    {
        public String id { get; set; }
        public string name { get; set; }
    }
    public class SiteList
    {
        public string id { get; set; }
        public string name { get; set; }
    }
    public class AllMasterData
    {
        public List<Aisle> AisleList { get; set; }
        public List<Rack> RackList { get; set; }
        public List<Bin> BinList { get; set; }
        public List<Reason> ReasonList { get; set; }
        public List<FLTList> FLTList { get; set; }
        public List<WareHouseList> WareHouseList { get; set; }
        public List<SiteList> SiteListData { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }

    public class LocationResponse
    {
        public string SiteId { get; set; }
        public string SiteName { get; set; }
        public string WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public string ZoneId { get; set; }
        public string ZoneName { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }
}

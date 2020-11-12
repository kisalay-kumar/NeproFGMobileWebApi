using FRD_InventoryWebApi.Controllers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NeproWebApi.Controllers
{
    public class LoadingPlanController : ApiController
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Conn"].ConnectionString);
        string query = ""; SqlCommand dbcommand;


        [Route("api/Nepro/GetLoadingPlan")]
        [HttpPost]
        public LoadingPlan ScanLookUpStillage(StickerReq SR)
        {
            LoadingPlan SS = new LoadingPlan();
            try
            {
                if (SR.StickerNo == "")
                {
                    SS.Status = "Failure";
                    SS.Message = "Enter Sticker ID";
                }
                if (SR.UserId == 0)
                {
                    SS.Status = "Failure";
                    SS.Message = "Invalid UserId";
                }
                query = "Sp_LadingPlanWebApi";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.Connection.Open();
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.Parameters.AddWithValue("@QueryType", "GetLoadingPlan");
                dbcommand.Parameters.AddWithValue("@StickerId", SR.StickerNo);
                dbcommand.Parameters.AddWithValue("@AssignedUser", SR.UserId);
                    
                dbcommand.CommandTimeout = 0;
                SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                DataSet dt = new DataSet();
                da.Fill(dt);
                if (dt != null && dt.Tables[0].Rows.Count > 0)
                {
                    List<ScanLoadingPlan> LoadingPlanList = new List<ScanLoadingPlan>();

                    foreach (DataRow row in dt.Tables[0].Rows)
                    {
                        ScanLoadingPlan A = new ScanLoadingPlan();
                        A.TLPHID = Convert.ToInt64(row["TLPHID"]);
                        A.LoadingPlanNo = row["LoadingPlanNo"].ToString();
                        A.CustomerId = row["CustomerId"].ToString();
                        LoadingPlanList.Add(A);
                    }
                    SS.ScanLoadingPlanList = LoadingPlanList;
                    SS.Status = "Success";
                    SS.Message = "Data retrived successfully";
                }
                else
                {
                    SS.Status = "Failure";
                    SS.Message = "Invalid Stillage";
                }
            }
            catch (Exception Ex)
            {
                SS.Status = "Failure";
                SS.Message = Ex.Message;
            }
            finally
            {
                dbcommand.Connection.Close();
            }
            return SS;
        }

        [Route("api/Nepro/GetLoadingPlanDetails")]
        [HttpPost]
        public LoadingPlanDetails LoadingPlanDetails(LoadingPlanReq LPR)
        {
            LoadingPlanDetails SS = new LoadingPlanDetails();
            try
            {
                if (LPR.LPID == "0")
                {
                    SS.Status = "Failure";
                    SS.Message = "Invalid ID";
                }
                if (LPR.UserId == "0")
                {
                    SS.Status = "Failure";
                    SS.Message = "Invalid UserId";
                }
                query = "Sp_LadingPlanWebApi";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.Connection.Open();
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.Parameters.AddWithValue("@QueryType", "LoadingPlanDetails");
                dbcommand.Parameters.AddWithValue("@TLPHID", LPR.LPID);
                dbcommand.Parameters.AddWithValue("@UserId", LPR.UserId);

                dbcommand.CommandTimeout = 0;
                SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                DataSet dt = new DataSet();
                da.Fill(dt);
                if (dt != null && dt.Tables[0].Rows.Count > 0)
                {
                    SS.TruckID = dt.Tables[0].Rows[0]["TruckID"].ToString();
                    SS.DriverName = dt.Tables[0].Rows[0]["DriverName"].ToString();
                    SS.GateNo = Convert.ToInt16(dt.Tables[0].Rows[0]["GateNo"]);
                    SS.LoadingPlanNo = dt.Tables[0].Rows[0]["LoadingPlanNo"].ToString();
                    SS.DriverID = dt.Tables[0].Rows[0]["DriverId"].ToString();
                    SS.TruckNo = dt.Tables[0].Rows[0]["TruckNo"].ToString();
                    

                    List<LoadingPlanDetailsList> LoadingPlanList = new List<LoadingPlanDetailsList>();
                    foreach (DataRow row in dt.Tables[0].Rows)
                    {
                        LoadingPlanDetailsList A = new LoadingPlanDetailsList();
                        A.WorkOrderQty = Convert.ToDecimal(row["WorkOrderQty"]);
                        A.ItemName = row["ItemName"].ToString();
                        A.ItemId = row["ItemId"].ToString();
                        A.SiteName = row["SiteName"].ToString();
                        A.Aisle = row["Aisle"].ToString();
                        A.Rack = row["Rack"].ToString();
                        A.Bin = row["Bin"].ToString();
                        A.StillageQty = Convert.ToDecimal(row["StillageQty"]);
                        A.StillageStdQty = Convert.ToDecimal(row["StillageStdQty"]);
                        A.WareHouseID = Convert.ToInt64(row["WareHouseID"]);
                        A.StickerID = row["StickerID"].ToString();
                        A.PickingQty = Convert.ToDecimal(row["PickingQty"]);
                        A.Zone = row["Zone"].ToString();
                        LoadingPlanList.Add(A);
                    }
                    SS.LoadingPlanList1 = LoadingPlanList;



                    List<ReasonList> ReasonList = new List<ReasonList>();
                    foreach (DataRow row in dt.Tables[1].Rows)
                    {
                        ReasonList A = new ReasonList();
                        A.id = Convert.ToString(row["id"]);
                        A.name = row["name"].ToString();

                        ReasonList.Add(A);
                    }
                    SS.ReasonList = ReasonList;






                    SS.Status = "Success";
                    SS.Message = "Data retrived successfully";
                }
                else
                {
                    SS.Status = "Failure";
                    SS.Message = "There is no more stillage in this loading plan.";
                }
            }
            catch (Exception Ex)
            {
                SS.Status = "Failure";
                SS.Message = Ex.Message;
            }
            finally
            {
                dbcommand.Connection.Close();
            }
            return SS;
        }
    }
}

public class LoadingPlanReq
{
    public string UserId { get; set; }
    public string LPID { get; set; }

}


public class LoadingPlan
{

    public List<ScanLoadingPlan> ScanLoadingPlanList { get; set; }
    public string Status { get; set; }
    public string Message { get; set; }
}
public class ScanLoadingPlan
{
    public Int64 TLPHID { get; set; }
    public string LoadingPlanNo { get; set; }
    public string CustomerId { get; set; }
}


public class LoadingPlanDetailsList
{
    public decimal WorkOrderQty { get; set; }
    public string ItemName { get; set; }
    public string ItemId { get; set; }
    public string SiteName { get; set; }
    public string Aisle { get; set; }
    public string Rack { get; set; }
    public string Bin { get; set; }
    public decimal StillageQty { get; set; }
    public decimal StillageStdQty { get; set; }
    public Int64 WareHouseID { get; set; }
    public string StickerID { get; set; }
    public decimal PickingQty { get; set; }
    public string Zone { get; set; }

}

public class LoadingPlanDetails
{
    public List<LoadingPlanDetailsList> LoadingPlanList1 { get; set; }
    public List<ReasonList> ReasonList { get; set; }
    public string TruckID { get; set; }
    public string DriverName { get; set; }
    public int GateNo { get; set; }
    public string LoadingPlanNo { get; set; }
    public string DriverID { get; set; }
    public string TruckNo { get; set; }
    public string Status { get; set; }
    public string Message { get; set; }
}

public class ReasonList
{
    public string id { get; set; }
    public string name { get; set; }
}
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
                //Isrent=0 then normal otherwise 1 = discarded or complete

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
                dbcommand.Parameters.AddWithValue("@Isrecent", SR.IsRecent);
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
                        A.LoadingWareHouseId = row["WareHouseID"].ToString();
                        A.RecentType = Convert.ToInt64(row["RecentType"]);
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


        [Route("api/Nepro/ReopenLoadingPlan")]
        [HttpPost]
        public LoadingPlan ReopenLoadingPlan(REopen SR)
        {
            LoadingPlan SS = new LoadingPlan();
            try
            {
             
                if (SR.TLPHID == 0)
                {
                    SS.Status = "Failure";
                    SS.Message = "Enter Sticker ID";
                }
                if (SR.UserId == "")
                {
                    SS.Status = "Failure";
                    SS.Message = "Invalid UserId";
                }
                query = "Sp_NewLoadingPlan";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.Connection.Open();
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.Parameters.AddWithValue("@QueryType", "ReopenLoadingPlan");
                dbcommand.Parameters.AddWithValue("@TLPHID", SR.TLPHID);
                dbcommand.Parameters.AddWithValue("@UserID", SR.UserId);
                dbcommand.CommandTimeout = 0;
                SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                DataSet dt = new DataSet();
                da.Fill(dt);
                if (dt != null && dt.Tables[0].Rows.Count > 0)
                {
                    SS.Status = "Success";
                    SS.Message = "Loading plan reopened successfully";
                }
                else
                {
                    SS.Status = "Failure";
                    SS.Message = "Invalid loading plan no";
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




        [Route("api/Nepro/GetNewLoadingPlanDetails")]
        [HttpPost]
        public NewLoadingPlanDetails LoadingPlanDetails(NewLoadingPlanReq LPR)
        {
            NewLoadingPlanDetails SS = new NewLoadingPlanDetails();
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
                query = "Sp_NewLoadingPlanWebApi";
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
                    if (dt.Tables[2].Rows[0]["result"].ToString() == "2")
                    {
                        SS.Status = "close";
                        SS.Message = "This loading plan is discarded";

                    }
                    else if (dt.Tables[2].Rows[0]["result"].ToString() == "3")
                    {
                        SS.Status = "close";
                        SS.Message = "This loading plan is finished";

                    }
                    else
                    {

                        SS.TruckID = dt.Tables[0].Rows[0]["TruckID"].ToString();
                        SS.DriverName = dt.Tables[0].Rows[0]["DriverName"].ToString();
                        SS.GateNo = Convert.ToInt16(dt.Tables[0].Rows[0]["GateNo"]);
                        SS.LoadingPlanNo = dt.Tables[0].Rows[0]["LoadingPlanNo"].ToString();
                        SS.DriverID = dt.Tables[0].Rows[0]["DriverId"].ToString();
                        SS.TruckNo = dt.Tables[0].Rows[0]["TruckNo"].ToString();


                        List<NewLoadingPlanDetailsList> LoadingPlanList = new List<NewLoadingPlanDetailsList>();
                        foreach (DataRow row in dt.Tables[0].Rows)
                        {
                            NewLoadingPlanDetailsList A = new NewLoadingPlanDetailsList();
                            A.ItemName = row["ItemName"].ToString();
                            A.ItemId = row["ItemId"].ToString();
                            A.ToBeLoadedQty = Convert.ToDecimal(row["PickingQty"]);
                            A.DeliveryReminderQty = Convert.ToDecimal(row["PDSCWDELIVERYREMINDER"]);
                            A.LoadedQty = Convert.ToDecimal(row["LoadedQuantity"]);
                            A.PickingPlanNo = row["PlanningID"].ToString();
                            A.SalesOrder = row["SalesOrder"].ToString();

                            LoadingPlanList.Add(A);
                        }
                        SS.ItemList = LoadingPlanList;



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

        [Route("api/Nepro/UpdateLoadingplanscanstatus")]
        [HttpPost]
        public LoadingPlan UpdateLoadingplanscanstatus(NewLoadingPlanReq LPR)
        {
            LoadingPlan SS1 = new LoadingPlan();
            try
            {
                if (LPR.LPID == "0")
                {
                    SS1.Status = "Failure";
                    SS1.Message = "Invalid ID";
                }
                if (LPR.UserId == "0")
                {
                    SS1.Status = "Failure";
                    SS1.Message = "Invalid UserId";
                }
                query = "Sp_NewLoadingPlanWebApi";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.Connection.Open();
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.Parameters.AddWithValue("@QueryType", "UpdatePendingFlag");
                dbcommand.Parameters.AddWithValue("@TLPHID", LPR.LPID);
                dbcommand.Parameters.AddWithValue("@UserId", LPR.UserId);

                dbcommand.CommandTimeout = 0;
                SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                DataSet dt = new DataSet();
                da.Fill(dt);
                if (dt != null && dt.Tables[0].Rows.Count > 0)
                {
                    if (dt.Tables[0].Rows[0]["result"].ToString() == "1")
                    {
                        SS1.Status = "Success";
                        SS1.Message = "Successfull move to ongoing to pending";

                    }
                   
                    else
                    {
                        SS1.Status = "Failure";
                        SS1.Message = "Sorry Invalid loading plan no.";

                    }


                }
                else
                {
                    SS1.Status = "Failure";
                    SS1.Message = "Sorry Invalid loading plan no.";
                }
            }
            catch (Exception Ex)
            {
                SS1.Status = "Failure";
                SS1.Message = Ex.Message;
            }
            finally
            {
                dbcommand.Connection.Close();
            }
            return SS1;
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
    public string LoadingWareHouseId { get; set; }
    public Int64 RecentType { get; set; }
}

public class REopen
{
    public Int64 TLPHID { get; set; }
    public string UserId { get; set; }
}


public class NewLoadingPlanReq
{
    public string UserId { get; set; }
    public string LPID { get; set; }

}

public class NewLoadingPlanDetailsList
{
    public string ItemName { get; set; }
    public string ItemId { get; set; }
    public string PickingPlanNo { get; set; }
    public decimal ToBeLoadedQty { get; set; }
    public decimal DeliveryReminderQty { get; set; }
    public decimal LoadedQty { get; set; }
    public string SalesOrder { get; set; }


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
    public string SalesOrder { get; set; }

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

public class NewLoadingPlanDetails
{
    public List<NewLoadingPlanDetailsList> ItemList { get; set; }
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
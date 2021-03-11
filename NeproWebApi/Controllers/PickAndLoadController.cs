using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NeproWebApi.Models;
using Newtonsoft.Json;

namespace NeproWebApi.Controllers
{
    public class PickAndLoadController : ApiController
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Conn"].ConnectionString);
        string query = ""; SqlCommand dbcommand;

        [Route("api/Nepro/UpdatePickAndLoading")]
        [HttpPost]
        public STDetailResponce STDetailUpdate(PickAndLoadReq PAL)
        {
            STDetailResponce SM = new STDetailResponce();
            try
            {
                if (PAL.StickerNo == "" || PAL.StickerNo == null)
                {
                    SM.Status = "Failure";
                    SM.Message = "Enter StickerNo";
                    return SM;
                }
                if (PAL.UserId == "" || PAL.UserId == null)
                {
                    SM.Status = "Failure";
                    SM.Message = "Invalid UserId";
                    return SM;
                }
                if (PAL.LoadingId == "" || PAL.LoadingId == null)
                {
                    SM.Status = "Failure";
                    SM.Message = "Invalid LoadingId";
                    return SM;
                }

                query = "Sp_PickAndLoad";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.CommandTimeout = 0;
                dbcommand.Parameters.AddWithValue("@QueryType", "FetchDataPick&Load");
                dbcommand.Parameters.AddWithValue("@StillageID", PAL.StickerNo);
                dbcommand.Parameters.AddWithValue("@UserId", PAL.UserId);
                dbcommand.Parameters.AddWithValue("@ActivityID", 4);
                dbcommand.Parameters.AddWithValue("@Reason", PAL.Reason);
                dbcommand.Parameters.AddWithValue("@LoadingId", PAL.LoadingId);
                SqlDataAdapter daGetData = new SqlDataAdapter(dbcommand);
                DataSet dsGetData = new DataSet();
                daGetData.Fill(dsGetData);

                SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                if (dsGetData.Tables[0].Rows[0]["value"].ToString() != "2")
                {

                    query = "Sp_AxWebserviceIntegration";
                    dbcommand = new SqlCommand(query, conn);
                    //  dbcommand.Connection.Open();
                    dbcommand.CommandType = CommandType.StoredProcedure;
                    dbcommand.CommandTimeout = 0;
                     da = new SqlDataAdapter(dbcommand);
                    DataSet ds = new DataSet();
                    da.Fill(ds);

                    //  DataSet ds = CommonManger.FillDatasetWithParam("Sp_AxWebserviceIntegration");
                    AXWebServiceRef1.Iace_FinishedGoodServiceClient obj = new AXWebServiceRef1.Iace_FinishedGoodServiceClient();
                    obj.ClientCredentials.Windows.ClientCredential.Domain = Convert.ToString(ds.Tables[0].Rows[0]["Domain"]);
                    obj.ClientCredentials.Windows.ClientCredential.UserName = Convert.ToString(ds.Tables[0].Rows[0]["Username"]);
                    obj.ClientCredentials.Windows.ClientCredential.Password = Convert.ToString(ds.Tables[0].Rows[0]["Password"]);

                    AXWebServiceRef1.CallContext Cct = new AXWebServiceRef1.CallContext();
                    Cct.Company = Convert.ToString(ds.Tables[0].Rows[0]["Company"]);
                    Cct.Language = Convert.ToString(ds.Tables[0].Rows[0]["Language"]);

                    string value = obj.InsertHistoryHeaderData(Cct, PAL.StickerNo, Convert.ToString(dsGetData.Tables[0].Rows[0]["SiteID"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["SiteID"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ItemId"]), Convert.ToDecimal(dsGetData.Tables[0].Rows[0]["WorkOrderQty"]));
                   // obj.InsertHistoryDetailData(Cct, PAL.StickerNo, Convert.ToString(dsGetData.Tables[1].Rows[0]["PlanningId"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityName"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityDesc"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), "", "", "", "", Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonName"]), Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonDes"]), "No", Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["MergeQty"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["FromMerge"]), PAL.LoadQuantity, 0, Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["StillageQty"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]), "Reserved", 0, "","","", Convert.ToString(dsGetData.Tables[1].Rows[0]["WareHouseID"]),0);
                    obj.InsertHistoryDetailData(Cct, PAL.StickerNo, Convert.ToString(dsGetData.Tables[1].Rows[0]["PlanningId"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityName"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityDesc"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), "", "", "", "", Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonName"]), Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonDes"]), "No", Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["MergeQty"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["FromMerge"]), PAL.LoadQuantity, 0, Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["StillageQty"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]), "Reserved", 0, "", "", "", Convert.ToString(dsGetData.Tables[1].Rows[0]["WareHouseID"]));

                }
                else
                {
                    SM.Status = "Failure";
                    SM.Message = "This Stillage Does Not Exist";
                    return SM;
                }

                query = "Sp_AssignWebApi";
                dbcommand = new SqlCommand(query, conn);
               // dbcommand.Connection.Open();
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.Parameters.AddWithValue("@QueryType", "UpdatePickAndLoading");
                dbcommand.Parameters.AddWithValue("@StickerId", PAL.StickerNo);
                dbcommand.Parameters.AddWithValue("@Reason", PAL.Reason);
                dbcommand.Parameters.AddWithValue("@LoadingId", PAL.LoadingId);
                dbcommand.Parameters.AddWithValue("@UserId", PAL.UserId);
                dbcommand.Parameters.AddWithValue("@ItemId", PAL.ItemId);
                dbcommand.Parameters.AddWithValue("@LoadedQuantity", PAL.LoadQuantity);
                dbcommand.Parameters.AddWithValue("@Iscompleted", PAL.Iscompleted);
                dbcommand.CommandTimeout = 0;
                da = new SqlDataAdapter(dbcommand);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows[0]["value"].ToString() == "1")
                {
                    SM.Status = "Success";
                    SM.Message = "Loaded Successfully";
                }
                else if (dt.Rows[0]["value"].ToString() == "2")
                {
                    SM.Status = "Failure";
                    SM.Message = "Stillage already loaded.!";
                }
                else
                {
                    SM.Status = "Failure";
                    SM.Message = "Loading Failure";
                }

            }
            catch (Exception Ex)
            {
                SM.Status = "Failure";
                SM.Message = Ex.Message;
            }
            //finally
            //{
            //    //dbcommand.Connection.Close();
            //}
            return SM;
        }




        [Route("api/Nepro/UpdateNewLoadingQty")]
        [HttpPost]
        public STDetailResponce ItemUpdatedStillageWise(LoadReqForNew PAL)
        {
            STDetailResponce SM = new STDetailResponce();
            try
            {
                if (PAL.StickerNo == "" || PAL.StickerNo == null)
                {
                    SM.Status = "Failure";
                    SM.Message = "Enter StickerNo";
                    return SM;
                }
                if (PAL.UserId == "" || PAL.UserId == null)
                {
                    SM.Status = "Failure";
                    SM.Message = "Invalid UserId";
                    return SM;
                }
                if (PAL.LPID == "" || PAL.LPID == null)
                {
                    SM.Status = "Failure";
                    SM.Message = "Invalid LoadingId";
                    return SM;
                }

                query = "[Sp_NewLoadingPlan]";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.CommandTimeout = 0;
                dbcommand.Parameters.AddWithValue("@QueryType", "FetchNewDataPick&Load");
                dbcommand.Parameters.AddWithValue("@StillageID", PAL.StickerNo);
                dbcommand.Parameters.AddWithValue("@UserId", PAL.UserId);
                dbcommand.Parameters.AddWithValue("@ActivityID", 4);
                dbcommand.Parameters.AddWithValue("@Reason", "");
                dbcommand.Parameters.AddWithValue("@LoadingId", PAL.LPID);
                dbcommand.Parameters.AddWithValue("@planningid", PAL.PickingPlanNo); 
                dbcommand.Parameters.AddWithValue("@LoadedQty", PAL.LoadQty);
                dbcommand.Parameters.AddWithValue("@IsUnLoad", PAL.IsUnload);

                SqlDataAdapter daGetData = new SqlDataAdapter(dbcommand);
                DataSet dsGetData = new DataSet();
                daGetData.Fill(dsGetData);

                SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                if (dsGetData.Tables[0].Rows[0]["value"].ToString() == "3")
                {
                    SM.Status = "close";
                    SM.Message = "This loading plan is discarded";
                }
                else if (dsGetData.Tables[0].Rows[0]["value"].ToString() == "4")
                {
                    SM.Status = "close";
                    SM.Message = "This loading plan is finished";
                }

                else
                {
                    
                    if (dsGetData.Tables[0].Rows[0]["value"].ToString() != "2")
                    {




                        query = "Sp_AxWebserviceIntegration";
                        dbcommand = new SqlCommand(query, conn);
                        //  dbcommand.Connection.Open();
                        dbcommand.CommandType = CommandType.StoredProcedure;
                        dbcommand.CommandTimeout = 0;
                        da = new SqlDataAdapter(dbcommand);
                        DataSet ds = new DataSet();
                        da.Fill(ds);

                        //  DataSet ds = CommonManger.FillDatasetWithParam("Sp_AxWebserviceIntegration");



                        ////     need to be un comment on live
                        ///
                        AXWebServiceRef1.Iace_FinishedGoodServiceClient obj = new AXWebServiceRef1.Iace_FinishedGoodServiceClient();
                        obj.ClientCredentials.Windows.ClientCredential.Domain = Convert.ToString(ds.Tables[0].Rows[0]["Domain"]);
                        obj.ClientCredentials.Windows.ClientCredential.UserName = Convert.ToString(ds.Tables[0].Rows[0]["Username"]);
                        obj.ClientCredentials.Windows.ClientCredential.Password = Convert.ToString(ds.Tables[0].Rows[0]["Password"]);

                        AXWebServiceRef1.CallContext Cct = new AXWebServiceRef1.CallContext();
                        Cct.Company = Convert.ToString(ds.Tables[0].Rows[0]["Company"]);
                        Cct.Language = Convert.ToString(ds.Tables[0].Rows[0]["Language"]);

                        string value = obj.InsertHistoryHeaderData(Cct, PAL.StickerNo, Convert.ToString(dsGetData.Tables[0].Rows[0]["SiteID"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["SiteID"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ItemId"]), Convert.ToDecimal(dsGetData.Tables[0].Rows[0]["WorkOrderQty"]));
                       // obj.InsertHistoryDetailData(Cct, PAL.StickerNo, Convert.ToString(dsGetData.Tables[1].Rows[0]["PlanningId"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityName"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityDesc"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), "", "", "", "", Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonName"]), Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonDes"]), "No", Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["MergeQty"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["FromMerge"]), PAL.LoadQty, 0, Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["StillageQty"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]), "Reserved", 0, "", "", "", Convert.ToString(dsGetData.Tables[1].Rows[0]["WareHouseID"]), 0);
                        obj.InsertHistoryDetailData(Cct, PAL.StickerNo, Convert.ToString(dsGetData.Tables[1].Rows[0]["PlanningId"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityName"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityDesc"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), "", "", "", "", Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonName"]), Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonDes"]), "No", Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["MergeQty"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["FromMerge"]), PAL.LoadQty, 0, Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["StillageQty"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]), "Reserved", 0, "", "", "", Convert.ToString(dsGetData.Tables[1].Rows[0]["WareHouseID"]));

                    }
                    else
                    {
                        SM.Status = "Failure";
                        SM.Message = "This Stillage Does Not Exist";
                        return SM;
                    }

                    query = "Sp_NewLoadingPlan";
                    dbcommand = new SqlCommand(query, conn);
                    // dbcommand.Connection.Open();
                    dbcommand.CommandType = CommandType.StoredProcedure;
                    dbcommand.Parameters.AddWithValue("@QueryType", "UpdateNewPickAndLoading");
                    dbcommand.Parameters.AddWithValue("@StickerId", PAL.StickerNo);
                    dbcommand.Parameters.AddWithValue("@Reason", "");
                    dbcommand.Parameters.AddWithValue("@LoadingId", PAL.LPID);
                    dbcommand.Parameters.AddWithValue("@UserId", PAL.UserId);
                    dbcommand.Parameters.AddWithValue("@ItemId", PAL.ItemId);
                    dbcommand.Parameters.AddWithValue("@LoadedQty", PAL.LoadQty);
                    dbcommand.Parameters.AddWithValue("@planningid", PAL.PickingPlanNo);
                    dbcommand.Parameters.AddWithValue("@IsUnLoad", PAL.IsUnload);
                    dbcommand.Parameters.AddWithValue("@SalesOrder", PAL.SalesOrder);


                    dbcommand.CommandTimeout = 0;
                    da = new SqlDataAdapter(dbcommand);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows[0]["value"].ToString() == "1")
                    {
                        SM.Status = "Success";
                        SM.Message = "Loaded Successfully";
                    }
                    else if (dt.Rows[0]["value"].ToString() == "2")
                    {
                        SM.Status = "Failure";
                        SM.Message = "Stillage already loaded.!";
                    }
                    else if (dt.Rows[0]["value"].ToString() == "3")
                    {
                        SM.Status = "Success";
                        SM.Message = "Unloaded Successfully.!";
                    }
                    else
                    {
                        SM.Status = "Failure";
                        SM.Message = "Loading Failure";
                    }
                }

            }
            catch (Exception Ex)
            {
                SM.Status = "Failure";
                SM.Message = Ex.Message;
            }
            
            return SM;
        }

        [Route("api/Nepro/ScanStillageForLoadingPlan")]
        [HttpPost]
        public ScanStillageResponse ScanLookUpStillage(StickerReq SR)
        {
            ScanStillageResponse SS = new ScanStillageResponse();
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


                query = "Sp_NewLoadingPlan";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.Connection.Open();
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.Parameters.AddWithValue("@QueryType", "GetScanStillage");
                dbcommand.Parameters.AddWithValue("@StickerId", SR.StickerNo);
                dbcommand.Parameters.AddWithValue("@UserId", SR.UserId);


                dbcommand.CommandTimeout = 0;
                SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                DataSet dt = new DataSet();
                da.Fill(dt);
                if (dt != null && dt.Tables[0].Rows.Count > 0)
                {
                    if (dt.Tables[6].Rows[0]["result"].ToString() == "2")
                    {
                        SS.Status = "close";
                        SS.Message = "This loading plan is discarded";

                    }
                    else if (dt.Tables[6].Rows[0]["result"].ToString() == "3")
                    {
                        SS.Status = "close";
                        SS.Message = "This loading plan is finished";

                    }
                    else
                    {

                        SS.StickerID = dt.Tables[0].Rows[0]["StickerID"].ToString();
                        SS.StandardQty = Convert.ToDecimal(dt.Tables[0].Rows[0]["StandardQty"]);
                        SS.ItemId = dt.Tables[0].Rows[0]["ItemId"].ToString();
                        SS.Description = dt.Tables[0].Rows[0]["Description"].ToString();
                        SS.ItemStdQty = Convert.ToDecimal(dt.Tables[0].Rows[0]["ItemStdQty"]);
                        SS.Location = dt.Tables[0].Rows[0]["Location"].ToString();
                        SS.IsTransfered = Convert.ToByte(dt.Tables[0].Rows[0]["IsTransfered"]);
                        SS.WareHouseID = dt.Tables[3].Rows[0]["WareHouseID"].ToString();
                        SS.WareHouseName = dt.Tables[0].Rows[0]["WareHouseName"].ToString();
                        SS.TransferId = dt.Tables[0].Rows[0]["TransferId"].ToString();
                        SS.IsShiped = dt.Tables[0].Rows[0]["IsShiped"].ToString();
                        SS.isHold = Convert.ToByte(dt.Tables[0].Rows[0]["isHold"]);
                        SS.IsCounted = Convert.ToByte(dt.Tables[0].Rows[0]["IsCounted"]);
                        SS.ToBeTransferWHID = dt.Tables[0].Rows[0]["ToBeTransferWHID"].ToString();

                        SS.STRP = Convert.ToInt16(dt.Tables[0].Rows[0]["STRP"]);
                        SS.UOM = Convert.ToString(dt.Tables[0].Rows[0]["UOM"]);
                        if (DBNull.Value.Equals(dt.Tables[2].Rows[0]["Prodstatus"]))
                        {
                            SS.Prodstatus = 0;
                            SS.WoStatus = "N/A";
                        }
                        else
                        {
                            SS.Prodstatus = Convert.ToInt16(dt.Tables[2].Rows[0]["Prodstatus"]);
                            SS.WoStatus = dt.Tables[2].Rows[0]["WorkorderStatus"].ToString();
                        }


                        List<SiteList> SiteList = new List<SiteList>();
                        foreach (DataRow row in dt.Tables[1].Rows)
                        {
                            SiteList A = new SiteList();
                            A.id = row["id"].ToString();
                            A.name = row["name"].ToString();
                            SiteList.Add(A);
                        }



                        List<ReasonList> ReasonList = new List<ReasonList>();
                        foreach (DataRow row in dt.Tables[4].Rows)
                        {
                            ReasonList A = new ReasonList();
                            A.id = Convert.ToString(row["id"]);
                            A.name = row["name"].ToString();

                            ReasonList.Add(A);
                        }
                        SS.ReasonList = ReasonList;
                        if (dt.Tables[5].Rows.Count > 0)
                        {
                            SS.isLoaded = 1;
                            List<StillageLoadedResponse> StillageLoadedResponse = new List<StillageLoadedResponse>();
                            foreach (DataRow row in dt.Tables[5].Rows)
                            {
                                StillageLoadedResponse A = new StillageLoadedResponse();
                                A.PickingPlanNo = Convert.ToString(row["PickingPlanNo"]);
                                A.LoadedQty = Convert.ToDecimal(row["LoadQty"]);

                                StillageLoadedResponse.Add(A);
                            }
                            SS.LoadedPickingPlanList = StillageLoadedResponse;
                        }
                        else
                        {
                            SS.isLoaded = 0;
                        }


                        SS.SiteListData = SiteList;
                        SS.Status = "Success";
                        SS.Message = "Data retrived successfully";
                    }
                }
                else
                {
                    SS.Status = "Failure";
                    SS.Message = "This stillage does not exist";
                    return SS;
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

        [Route("api/Nepro/UpdateLoadingPlan")]
        [HttpPost]
        public STDetailResponce UpdateLoadingPlan(LoadReqForNew PAL)
        {
            STDetailResponce SM = new STDetailResponce();
            try
            {
                if (PAL.UserId == "" || PAL.UserId == null)
                {
                    SM.Status = "Failure";
                    SM.Message = "Invalid UserId";
                    return SM;
                }
                if (PAL.LPID == "" || PAL.LPID == null)
                {
                    SM.Status = "Failure";
                    SM.Message = "Invalid LoadingId";
                    return SM;
                }

                query = "[Sp_NewLoadingPlan]";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.CommandTimeout = 0;
                dbcommand.Parameters.AddWithValue("@QueryType", "UpdateLoadingPlanStatus");
                dbcommand.Parameters.AddWithValue("@UserId", PAL.UserId);
                dbcommand.Parameters.AddWithValue("@LoadingId", PAL.LPID);
                dbcommand.Parameters.AddWithValue("@Isdiscard", PAL.Isdiscard);
                SqlDataAdapter daGetData = new SqlDataAdapter(dbcommand);
                DataSet dsGetData = new DataSet();
                daGetData.Fill(dsGetData);


                if (dsGetData.Tables[0].Rows[0]["value"].ToString() == "1")
                {
                    SM.Status = "Success";
                    SM.Message = "Loading Plan finished Successfully";
                }
                else if (dsGetData.Tables[0].Rows[0]["value"].ToString() == "3")
                {
                    SM.Status = "Success";
                    SM.Message = "Loading Plan Discarded Successfully";
                }
                else if (dsGetData.Tables[0].Rows[0]["value"].ToString() == "2")
                {
                    SM.Status = "Failure";
                    SM.Message = "Loading Failure";
                }
                else
                {
                    SM.Status = "Failure";
                    SM.Message = "Loading Failure";
                }

            }
            catch (Exception Ex)
            {
                SM.Status = "Failure";
                SM.Message = Ex.Message;
            }

            return SM;
        }
    }
}

public class LoadReqForNew
{
    public string StickerNo { get; set; }
    public string UserId { get; set; }
    public string ItemId { get; set; }
    public string PickingPlanNo { get; set; }
    public Decimal LoadQty { get; set; }
    public string LPID { get; set; }
    public string IsUnload { get; set; }
    public string Isdiscard { get; set; }
    public string SalesOrder { get; set; }

}

public class StillageLoadedResponse
{
    public string PickingPlanNo { get; set; }
    public decimal LoadedQty { get; set; }
}

    public class ScanStillageResponse
{
    public List<SiteList> SiteListData { get; set; }
    public List<StillageLoadedResponse> LoadedPickingPlanList { get; set; }
    public List<ReasonList> ReasonList { get; set; }
    public string StickerID { get; set; }
    public decimal StandardQty { get; set; }
    public string ItemId { get; set; }
    public string Description { get; set; }
    public decimal ItemStdQty { get; set; }
    public string WareHouseName { get; set; }
    public string Location { get; set; }
    public byte IsTransfered { get; set; }
    public string WareHouseID { get; set; }
    public string SiteID { get; set; }
    public string Status { get; set; }
    public string Message { get; set; }
    public string TransferId { get; set; }
    public string IsShiped { get; set; }
    public int STRP { get; set; }
    public string UOM { get; set; }
    public byte isHold { get; set; }
    public byte IsCounted { get; set; }
    public string ToBeTransferWHID { get; set; }
    public string WoStatus { get; set; }
    public int Prodstatus { get; set; }
    public byte IsAssignTransfer { get; set; }
    public int isLoaded { get; set; }
}

public class StickerReq
{
    public string StickerNo { get; set; }
    public Int64 UserId { get; set; }
    public string IsRecent { get; set; }
}
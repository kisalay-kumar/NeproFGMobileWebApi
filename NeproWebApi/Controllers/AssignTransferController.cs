using Json.Net;
using Newtonsoft.Json;
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
    public class AssignTransferController : ApiController
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Conn"].ConnectionString);
        string query = ""; SqlCommand dbcommand;
        [Route("api/Nepro/AssignTransfer")]
        [HttpPost]
        public AssignTransferRes AssignTransferProcess(AssignTransferReq ATReq)
        {
            AssignTransferRes ATRes = new AssignTransferRes();
            try
            {
                if (ATReq.UserId == 0)
                {
                    ATRes.Status = "Failure";
                    ATRes.Message = "Invalid UserId";
                }
                var json = JsonConvert.SerializeObject(ATReq.StillageList);

                if (ATReq.IsTj == 0)
                {
                    query = "Sp_AssignTransfer";
                    dbcommand = new SqlCommand(query, conn);
                    dbcommand.Connection.Open();
                    dbcommand.CommandType = CommandType.StoredProcedure;
                    dbcommand.Parameters.AddWithValue("@QueryType", "SaveTempData");
                    dbcommand.Parameters.AddWithValue("@IsTj", ATReq.IsTj);
                    dbcommand.Parameters.AddWithValue("@SiteId", ATReq.SiteId);
                    dbcommand.Parameters.AddWithValue("@WareHouseId", ATReq.WareHouseId);
                    dbcommand.Parameters.AddWithValue("@FLT", ATReq.FLT);
                    dbcommand.Parameters.AddWithValue("@UserId", ATReq.UserId);
                    dbcommand.Parameters.AddWithValue("@FromWareHouseId", ATReq.FromWareHouseId);
                    dbcommand.Parameters.AddWithValue("@json", json);
                    dbcommand.CommandTimeout = 0;
                    SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                    DataSet dt = new DataSet();
                    da.Fill(dt);


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


                    var Value = obj.InsertTransferHeader(Cct, ATReq.StillageList[0].StillageID, ATReq.WareHouseId, Convert.ToString(dt.Tables[0].Rows[0]["UserName"]), Convert.ToString(dt.Tables[0].Rows[0]["ActivityId"]));

                    int count = 0;
                    // foreach (var Data in ATReq.StillageList)
                    for (var i = 0; i < ATReq.StillageList.Count; i++)

                    {
                        //string a = ATReq.StillageList[i].StillageID;
                        obj.InsertTransferOrderLines(Cct, Value, ATReq.StillageList[i].StillageID);

                        if(count==0)
                        { 
                        query = "Sp_AssignTransfer";
                        dbcommand = new SqlCommand(query, conn);
                        //dbcommand.Connection.Open();
                        dbcommand.CommandType = CommandType.StoredProcedure;
                        dbcommand.Parameters.AddWithValue("@QueryType", "SaveDataApi");
                        dbcommand.Parameters.AddWithValue("@IsTj", ATReq.IsTj);
                        dbcommand.Parameters.AddWithValue("@SiteId", ATReq.SiteId);
                        dbcommand.Parameters.AddWithValue("@WareHouseId", ATReq.WareHouseId);
                        dbcommand.Parameters.AddWithValue("@FLT", ATReq.FLT);
                        dbcommand.Parameters.AddWithValue("@UserId", ATReq.UserId);
                        dbcommand.Parameters.AddWithValue("@FromWareHouseId", ATReq.FromWareHouseId);
                        dbcommand.Parameters.AddWithValue("@json", json);
                            dbcommand.Parameters.AddWithValue("@ActivityId", dt.Tables[0].Rows[0]["ActivityId"]);
                            dbcommand.Parameters.AddWithValue("@LocationId", ATReq.LocationId);
                            dbcommand.CommandTimeout = 0;
                            SqlDataAdapter da5 = new SqlDataAdapter(dbcommand);
                            DataSet dt5 = new DataSet();
                            da5.Fill(dt5);
                        }

                        query = "Sp_AssignTransfer";
                        dbcommand = new SqlCommand(query, conn);
                        dbcommand.CommandType = CommandType.StoredProcedure;
                        dbcommand.CommandTimeout = 0;
                        dbcommand.Parameters.AddWithValue("@QueryType", "FetchAssignTransferData");
                        dbcommand.Parameters.AddWithValue("@StickerId", ATReq.StillageList[i].StillageID);
                        dbcommand.Parameters.AddWithValue("@UserId", ATReq.UserId);
                        dbcommand.Parameters.AddWithValue("@journalId", Value);
                        SqlDataAdapter daGetData = new SqlDataAdapter(dbcommand);
                        DataSet dsGetData = new DataSet();
                        daGetData.Fill(dsGetData);
                        da = new SqlDataAdapter(dbcommand);
                        //var Hold = "";
                        //if (dsGetData.Tables[0].Rows[0]["isHold"].ToString() == "True")
                        //{
                        //    Hold = "Qc Hold";
                        //}
                        //else
                        //{
                        //    Hold = "Qc Release";
                        //}

                        obj.InsertHistoryHeaderData(Cct, ATReq.StillageList[i].StillageID, Convert.ToString(dsGetData.Tables[0].Rows[0]["SiteID"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ItemId"]), Convert.ToDecimal(dsGetData.Tables[0].Rows[0]["WorkOrderQty"]));
                        obj.InsertHistoryDetailData(Cct, ATReq.StillageList[i].StillageID, "", "Planned for Transfer Order", "Planned for Transfer Order", Convert.ToString(dsGetData.Tables[0].Rows[0]["StillageLocation"]), "", Convert.ToString(ATReq.FLT), "", "", "", "", "Yes", 0, "", 0, 0, Convert.ToDecimal(dsGetData.Tables[0].Rows[0]["StillageQty"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["UserName"]), "Reserved", 0, "", Convert.ToString(dsGetData.Tables[0].Rows[0]["FromWareHouseId"]), ATReq.WareHouseId, Convert.ToString(dsGetData.Tables[0].Rows[0]["FromWareHouseId"]), 0);

                        count++;
                    }

                    query = "Sp_AssignTransfer";
                    dbcommand = new SqlCommand(query, conn);
                    //dbcommand.Connection.Open();
                    dbcommand.CommandType = CommandType.StoredProcedure;
                    dbcommand.Parameters.AddWithValue("@QueryType", "UpdateIsAssignTransfer");
                    dbcommand.Parameters.AddWithValue("@json", json);
                    dbcommand.CommandTimeout = 0;
                    da = new SqlDataAdapter(dbcommand);
                    dt = new DataSet();
                    da.Fill(dt);


                    ATRes.Status = "Success";
                    ATRes.Message = "Transfer assigned successfully";

                }
                else
                {
                    query = "Sp_AssignTransfer";
                    dbcommand = new SqlCommand(query, conn);
                    dbcommand.Connection.Open();
                    dbcommand.CommandType = CommandType.StoredProcedure;
                    dbcommand.Parameters.AddWithValue("@QueryType", "SaveTempData");
                    dbcommand.Parameters.AddWithValue("@IsTj", ATReq.IsTj);
                    dbcommand.Parameters.AddWithValue("@SiteId", ATReq.SiteId);
                    dbcommand.Parameters.AddWithValue("@WareHouseId", ATReq.WareHouseId);
                    dbcommand.Parameters.AddWithValue("@FLT", ATReq.FLT);
                    dbcommand.Parameters.AddWithValue("@UserId", ATReq.UserId);
                    dbcommand.Parameters.AddWithValue("@FromWareHouseId", ATReq.FromWareHouseId);
                    dbcommand.Parameters.AddWithValue("@json", json);

                    dbcommand.CommandTimeout = 0;
                    SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                    DataSet dt = new DataSet();
                    da.Fill(dt);

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

                    string journalId = obj.CreateTransferJournalHeader(Cct, Convert.ToString(dt.Tables[0].Rows[0]["ActivityId"]), Convert.ToString(dt.Tables[0].Rows[0]["UserName"]), ATReq.StillageList[0].StillageID);

                    int count = 0;
                    for (var i = 0; i < ATReq.StillageList.Count; i++)

                    {
                        //string a = ATReq.StillageList[i].StillageID;
                        if (count == 0)
                        {
                            query = "Sp_AssignTransfer";
                            dbcommand = new SqlCommand(query, conn);
                            //dbcommand.Connection.Open();
                            dbcommand.CommandType = CommandType.StoredProcedure;
                            dbcommand.Parameters.AddWithValue("@QueryType", "SaveDataApi");
                            dbcommand.Parameters.AddWithValue("@IsTj", ATReq.IsTj);
                            dbcommand.Parameters.AddWithValue("@SiteId", ATReq.SiteId);
                            dbcommand.Parameters.AddWithValue("@WareHouseId", ATReq.WareHouseId);
                            dbcommand.Parameters.AddWithValue("@FLT", ATReq.FLT);
                            dbcommand.Parameters.AddWithValue("@UserId", ATReq.UserId);
                            dbcommand.Parameters.AddWithValue("@FromWareHouseId", ATReq.FromWareHouseId);
                            dbcommand.Parameters.AddWithValue("@json", json);
                            dbcommand.Parameters.AddWithValue("@ActivityId", dt.Tables[0].Rows[0]["ActivityId"]);
                            dbcommand.Parameters.AddWithValue("@LocationId", ATReq.LocationId);
                            dbcommand.CommandTimeout = 0;
                            dbcommand.CommandTimeout = 0;
                            SqlDataAdapter da2 = new SqlDataAdapter(dbcommand);
                            DataSet dt2 = new DataSet();
                            da2.Fill(dt2);
                        }

                        query = "Sp_AssignTransfer";
                        dbcommand = new SqlCommand(query, conn);
                        dbcommand.CommandType = CommandType.StoredProcedure;
                        dbcommand.CommandTimeout = 0;
                        dbcommand.Parameters.AddWithValue("@QueryType", "FetchAssignTransferData");
                        dbcommand.Parameters.AddWithValue("@StickerId", ATReq.StillageList[i].StillageID);
                        dbcommand.Parameters.AddWithValue("@UserId", ATReq.UserId);
                        dbcommand.Parameters.AddWithValue("@journalId", journalId);
                        SqlDataAdapter daGetData = new SqlDataAdapter(dbcommand);
                        DataSet dsGetData = new DataSet();
                        daGetData.Fill(dsGetData);
                        da = new SqlDataAdapter(dbcommand);
                        //var Hold = "";
                        //if (dsGetData.Tables[0].Rows[0]["isHold"].ToString() == "True")
                        //{
                        //    Hold = "Qc Hold";
                        //}
                        //else
                        //{
                        //    Hold = "Qc Release"; 
                        //}

                        string result1 = obj.CreateTransferJournalLines(Cct, journalId, Convert.ToString(dsGetData.Tables[0].Rows[0]["ItemId"]), Convert.ToDecimal(dsGetData.Tables[0].Rows[0]["StillageQty"]), Convert.ToDateTime(DateTime.Now), Convert.ToString(dsGetData.Tables[0].Rows[0]["fromSiteID"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["FromWareHouseId"]), ATReq.SiteId, ATReq.WareHouseId, ATReq.StillageList[i].StillageID);


                        obj.InsertHistoryHeaderData(Cct, ATReq.StillageList[i].StillageID, Convert.ToString(dsGetData.Tables[0].Rows[0]["SiteID"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ItemId"]), Convert.ToDecimal(dsGetData.Tables[0].Rows[0]["WorkOrderQty"]));
                        obj.InsertHistoryDetailData(Cct, ATReq.StillageList[i].StillageID, "", "Planned for Transfer Journal", "Planned for Transfer Journal", Convert.ToString(dsGetData.Tables[0].Rows[0]["StillageLocation"]), "", Convert.ToString(ATReq.FLT), "", "", "", "", "Yes", 0, "", 0, 0, Convert.ToDecimal(dsGetData.Tables[0].Rows[0]["StillageQty"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["UserName"]), "Reserved", 0, "", Convert.ToString(dsGetData.Tables[0].Rows[0]["FromWareHouseId"]), ATReq.WareHouseId, Convert.ToString(dsGetData.Tables[0].Rows[0]["FromWareHouseId"]), 0);

                        count++;
                    }

                    query = "Sp_AssignTransfer";
                    dbcommand = new SqlCommand(query, conn);
                    //dbcommand.Connection.Open();
                    dbcommand.CommandType = CommandType.StoredProcedure;
                    dbcommand.Parameters.AddWithValue("@QueryType", "UpdateIsAssignTransfer");
                    dbcommand.Parameters.AddWithValue("@json", json);
                    dbcommand.CommandTimeout = 0;
                    da = new SqlDataAdapter(dbcommand);
                    dt = new DataSet();
                    da.Fill(dt);



                    ATRes.Status = "Success";
                    ATRes.Message = "Transfer assigned successfully";


                }
            }
            catch (Exception Ex)
            {
                ATRes.Status = "Failure";
                ATRes.Message = Ex.Message;
            }
            finally
            {
                dbcommand.Connection.Close();
            }
            return ATRes;
        }


        [Route("api/Nepro/GetAssignTransferHeader")]
        [HttpPost]
        public GetAssignTransferDataRes GetAssignTransferHeader(GetAssignTransferDataReq GATDReq)
        {
            GetAssignTransferDataRes GATDRes = new GetAssignTransferDataRes();
            try
            {

                if (GATDReq.UserId == 0)
                {
                    GATDRes.Status = "Failure";
                    GATDRes.Message = "Invalid UserId";
                    return GATDRes;
                }

                query = "Sp_AssignTransfer";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.CommandTimeout = 0;
                dbcommand.Parameters.AddWithValue("@QueryType", "GetAssignTransferData");
                dbcommand.Parameters.AddWithValue("@UserId", GATDReq.UserId);
                SqlDataAdapter daGetData = new SqlDataAdapter(dbcommand);
                DataSet dsGetData = new DataSet();
                daGetData.Fill(dsGetData);

                if (Convert.ToString(dsGetData.Tables[0].Rows[0]["value"]) == "0")
                {
                    List<TransferList> TransferList = new List<TransferList>();
                    foreach (DataRow row in dsGetData.Tables[0].Rows)
                    {
                        TransferList A = new TransferList();
                        A.ActivityId = row["ActivityId"].ToString();
                        A.IsTj = Convert.ToInt16(row["IsTj"]);
                        A.NoOfStillages = row["NoOfStillage"].ToString();
                        A.SiteId = row["SiteId"].ToString();
                        A.WareHouseId = row["WareHouseId"].ToString();
                        A.FromWareHouseId = row["FromWareHouseId"].ToString();
                        A.TATHID = row["TATHID"].ToString();
                        TransferList.Add(A);
                    }
                    GATDRes.TransferList = TransferList;



                    GATDRes.Status = "Success";
                    GATDRes.Message = "Data Send successfully";
                }
                else
                {
                    GATDRes.Status = "Failure";
                    GATDRes.Message = "No assignments found";
                }
            }
            catch (Exception Ex)
            {
                GATDRes.Status = "Failure";
                GATDRes.Message = Ex.Message;
            }
            finally
            {
                dbcommand.Connection.Close();
            }
            return GATDRes;
        }

        [Route("api/Nepro/GetAssignTransferDetails")]
        [HttpPost]
        public GetAssignTransferDataRes GetAssignTransferDetails(GetAssignTransferDataReq GATDReq)
        {
            GetAssignTransferDataRes GATDRes = new GetAssignTransferDataRes();
            try
            {

                if (GATDReq.UserId == 0)
                {
                    GATDRes.Status = "Failure";
                    GATDRes.Message = "Invalid UserId";
                    return GATDRes;
                }

                query = "Sp_AssignTransfer";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.CommandTimeout = 0;
                dbcommand.Parameters.AddWithValue("@QueryType", "GetAssignTransferDetails");
                dbcommand.Parameters.AddWithValue("@UserId", GATDReq.UserId);
                dbcommand.Parameters.AddWithValue("@TATHID1", GATDReq.TATHID);
                dbcommand.Parameters.AddWithValue("@ActivityId", GATDReq.ActivityId);
                SqlDataAdapter daGetData = new SqlDataAdapter(dbcommand);
                DataSet dsGetData = new DataSet();
                daGetData.Fill(dsGetData);

                if (Convert.ToString(dsGetData.Tables[0].Rows[0]["value"]) == "0")
                {
                    List<StillageList> StillageList = new List<StillageList>();
                    foreach (DataRow row in dsGetData.Tables[0].Rows)
                    {
                        StillageList A = new StillageList();
                        A.StillageID = row["StickerId"].ToString();
                        A.StillageQty = row["StillageQty"].ToString();
                        A.ItemID = row["ItemID"].ToString();
                        A.ItemDesc = row["ItemDesc"].ToString();
                        StillageList.Add(A);
                    }
                    GATDRes.StillageList = StillageList;




                    GATDRes.Status = "Success";
                    GATDRes.Message = "Data Send successfully";
                }
                else
                {
                    GATDRes.Status = "Failure";
                    GATDRes.Message = "Stillages not found";
                }
            }
            catch (Exception Ex)
            {
                GATDRes.Status = "Failure";
                GATDRes.Message = Ex.Message;
            }
            finally
            {
                dbcommand.Connection.Close();
            }
            return GATDRes;
        }


        [Route("api/Nepro/PostAssignTransfer")]
        [HttpPost]
        public AssignTransferRes PostAssignTransfer(AssignTransferReq ATReq)
        {
            AssignTransferRes ATR = new AssignTransferRes();
            try
            {
                if (ATReq.UserId == 0)
                {
                    ATR.Status = "Failure";
                    ATR.Message = "Invalid UserId";
                    return ATR;
                }

                query = "Sp_AssignTransfer";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.CommandTimeout = 0;
                dbcommand.Parameters.AddWithValue("@QueryType", "FetchAssignTransferDetails");
                dbcommand.Parameters.AddWithValue("@UserId", ATReq.UserId);
                dbcommand.Parameters.AddWithValue("@TATHID1", ATReq.TATHID);
                SqlDataAdapter daGetData = new SqlDataAdapter(dbcommand);
                DataSet dsGetData = new DataSet();
                daGetData.Fill(dsGetData);
                int i = 0;
                if (Convert.ToString(dsGetData.Tables[0].Rows[0]["value"]) == "0")
                {
                    query = "Sp_AxWebserviceIntegration";
                    dbcommand = new SqlCommand(query, conn);
                    //  dbcommand.Connection.Open();
                    dbcommand.CommandType = CommandType.StoredProcedure;
                    dbcommand.CommandTimeout = 0;
                    SqlDataAdapter da = new SqlDataAdapter(dbcommand);
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

                    if (ATReq.IsTj == 1)
                    {
                        obj.PostTransferJournal(Cct, Convert.ToString(dsGetData.Tables[0].Rows[i]["TransferID"]));

                        for (i = 0; i < dsGetData.Tables[0].Rows.Count; i++)
                        {
                            string value = obj.InsertHistoryHeaderData(Cct, dsGetData.Tables[0].Rows[i]["StillageID"].ToString(), Convert.ToString(dsGetData.Tables[0].Rows[i]["SiteID"]), Convert.ToString(dsGetData.Tables[0].Rows[i]["WorkOrderNo"]), Convert.ToString(dsGetData.Tables[0].Rows[i]["ItemId"]), Convert.ToDecimal(dsGetData.Tables[0].Rows[i]["WorkOrderQty"]));

                            obj.InsertHistoryDetailData(Cct, dsGetData.Tables[0].Rows[i]["StillageID"].ToString(), "", "Transfered with TJ", "Transfered with TJ", Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), "", "", "", "", "", "", "No", 0, "", 0, 0, Convert.ToDecimal(dsGetData.Tables[0].Rows[i]["StillageQty"]), Convert.ToString(dsGetData.Tables[0].Rows[i]["UserName"]), "QC Release", 0, "", Convert.ToString(dsGetData.Tables[0].Rows[i]["FromWareHouseId"]), Convert.ToString(dsGetData.Tables[0].Rows[i]["ToWareHouseId"]), Convert.ToString(dsGetData.Tables[0].Rows[i]["ToWareHouseId"]), 0);



                            //i++;
                        }
                    }
                    else
                    {
                        obj.TransferOrderPickingList(Cct, Convert.ToString(dsGetData.Tables[0].Rows[i]["TransferID"]));

                        obj.shipTransferOrder(Cct, Convert.ToString(dsGetData.Tables[0].Rows[i]["TransferID"]));
                        for (i = 0; i < dsGetData.Tables[0].Rows.Count; i++)
                        {
                            string value = obj.InsertHistoryHeaderData(Cct, dsGetData.Tables[0].Rows[i]["StillageID"].ToString(), Convert.ToString(dsGetData.Tables[0].Rows[i]["SiteID"]), Convert.ToString(dsGetData.Tables[0].Rows[i]["WorkOrderNo"]), Convert.ToString(dsGetData.Tables[0].Rows[i]["ItemId"]), Convert.ToDecimal(dsGetData.Tables[0].Rows[i]["WorkOrderQty"]));

                            obj.InsertHistoryDetailData(Cct, dsGetData.Tables[0].Rows[i]["StillageID"].ToString(), "", "Transfered with TO", "Transfered with TO", Convert.ToString(dsGetData.Tables[0].Rows[i]["StillageLocation"]), "", "", "", "", "", "", "Yes", 0, "", 0, 0, Convert.ToDecimal(dsGetData.Tables[0].Rows[i]["StillageQty"]), Convert.ToString(dsGetData.Tables[0].Rows[i]["UserName"]), "QC Release", 0, "", Convert.ToString(dsGetData.Tables[0].Rows[i]["FromWareHouseId"]), Convert.ToString(dsGetData.Tables[0].Rows[i]["ToWareHouseId"]), Convert.ToString(dsGetData.Tables[0].Rows[i]["ToWareHouseId"]), 0);
                            // i++;
                            query = "Sp_AssignTransfer";
                            dbcommand = new SqlCommand(query, conn);
                            dbcommand.CommandType = CommandType.StoredProcedure;
                            dbcommand.CommandTimeout = 0;
                            dbcommand.Parameters.AddWithValue("@QueryType", "UpdateAssignedSticker");
                            dbcommand.Parameters.AddWithValue("@UserId", ATReq.UserId);
                            dbcommand.Parameters.AddWithValue("@TATHID1", ATReq.TATHID);
                            dbcommand.Parameters.AddWithValue("@IsTj", ATReq.IsTj);
                            dbcommand.Parameters.AddWithValue("@StickerId", dsGetData.Tables[0].Rows[i]["StillageID"].ToString());


                            SqlDataAdapter daGetData1 = new SqlDataAdapter(dbcommand);
                            DataSet dsGetData1 = new DataSet();
                            daGetData1.Fill(dsGetData1);
                            if (dsGetData1.Tables[0].Rows[0]["value"].ToString() == "2")
                            {
                                ATR.Status = "Success";
                                ATR.Message = "Transfered successfully with TO Process.";

                            }

                        }
                    }
                    if (ATReq.IsTj == 1)
                    {

                        query = "Sp_AssignTransfer";
                        dbcommand = new SqlCommand(query, conn);
                        dbcommand.CommandType = CommandType.StoredProcedure;
                        dbcommand.CommandTimeout = 0;
                        dbcommand.Parameters.AddWithValue("@QueryType", "UpdateAssignedSticker");
                        dbcommand.Parameters.AddWithValue("@UserId", ATReq.UserId);
                        dbcommand.Parameters.AddWithValue("@TATHID1", ATReq.TATHID);
                        dbcommand.Parameters.AddWithValue("@IsTj", ATReq.IsTj);
                        dbcommand.Parameters.AddWithValue("@StickerId", "");


                        SqlDataAdapter daGetData1 = new SqlDataAdapter(dbcommand);
                        DataSet dsGetData1 = new DataSet();
                        daGetData1.Fill(dsGetData1);
                        if (dsGetData1.Tables[0].Rows[0]["value"].ToString() == "1")
                        {


                            query = "Sp_TransferWebApi";
                            dbcommand = new SqlCommand(query, conn);
                            //dbcommand.Connection.Open();
                            dbcommand.CommandType = CommandType.StoredProcedure;
                            dbcommand.Parameters.AddWithValue("@QueryType", "ResetDataAfterTransferAssign");
                            dbcommand.Parameters.AddWithValue("@StickerId", ATReq.TATHID);

                            dbcommand.CommandTimeout = 0;
                            da = new SqlDataAdapter(dbcommand);
                            DataTable dt3 = new DataTable();
                            da.Fill(dt3);


                            ATR.Status = "Success";
                            ATR.Message = "Transfered successfully with TJ Process.";

                        }

                    }

                }
            }
            catch (Exception Ex)
            {
                ATR.Status = "Failure";
                ATR.Message = Ex.Message;
            }
            return ATR;
        }


        [Route("api/Nepro/GetLocationFromWarehouse")]
        [HttpPost]
        public SendLocationFromWarehouse GetLocationFromWarehouse(GetWarehouseReq GWHReq)
        {
            SendLocationFromWarehouse SLWHRes = new SendLocationFromWarehouse();
            try
            {

                if (GWHReq.WareHouseId == "")
                {
                    SLWHRes.Status = "Failure";
                    SLWHRes.Message = "Invalid WareHouse";
                    return SLWHRes;
                }

                query = "Sp_AssignTransfer";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.CommandTimeout = 0;
                dbcommand.Parameters.AddWithValue("@QueryType", "GetLocationFromWarehouse");
                dbcommand.Parameters.AddWithValue("@UserId", GWHReq.UserId);
                dbcommand.Parameters.AddWithValue("@WareHouseId", GWHReq.WareHouseId);
                SqlDataAdapter daGetData = new SqlDataAdapter(dbcommand);
                DataSet dsGetData = new DataSet();
                daGetData.Fill(dsGetData);

                List<LocationList> LocationList = new List<LocationList>();
                foreach (DataRow row in dsGetData.Tables[0].Rows)
                {
                    LocationList A = new LocationList();
                    A.id = row["id"].ToString();
                    A.name = row["name"].ToString();
                    LocationList.Add(A);
                }
                SLWHRes.LocationData = LocationList;




                SLWHRes.Status = "Success";
                SLWHRes.Message = "Location send successfully";
            }
            catch (Exception Ex)
            {
                SLWHRes.Status = "Failure";
                SLWHRes.Message = Ex.Message;
            }
            finally
            {
                dbcommand.Connection.Close();
            }
            return SLWHRes;
        }







    }
}


public class SendLocationFromWarehouse
{

    public string Status { get; set; }
    public string Message { get; set; }
    public List<LocationList> LocationData { get; set; }
}

public class GetWarehouseReq
{
    public string WareHouseId { get; set; }
    public Int64 UserId { get; set; }

}
public class LocationList
{

    public string id { get; set; }
    public string name { get; set; }
}


public class AssignTransferRes
{
    public string Status { get; set; }
    public string Message { get; set; }
}

public class AssignTransferReq
{
    public Int64 UserId { get; set; }
    public Int64 IsTj { get; set; }
    public Int64 TATHID { get; set; }
    public Int64 FLT { get; set; }
    public string SiteId { get; set; }
    public string WareHouseId { get; set; }
    public string FromWareHouseId { get; set; }
    public List<StillageList> StillageList { get; set; }
    public string LocationId { get; set; }
}
public class StillageList
{
    public string StillageID { get; set; }
    public string ItemID { get; set; }
    public string ItemDesc { get; set; }
    public string StillageQty { get; set; }
}

public class GetAssignTransferDataRes
{
    public string Status { get; set; }
    public string Message { get; set; }
    public Int64 UserId { get; set; }
    public List<TransferList> TransferList { get; set; }
    public List<StillageList> StillageList { get; set; }
}
public class GetAssignTransferDataReq
{
    public Int64 UserId { get; set; }
    public Int64 TATHID { get; set; }
    public Int64 ActivityId { get; set; }

}
public class TransferList
{
    public Int64 IsTj { get; set; }
    public string SiteId { get; set; }
    public string ActivityId { get; set; }
    public string TATHID { get; set; }
    public string NoOfStillages { get; set; }
    public string WareHouseId { get; set; }
    public string FromWareHouseId { get; set; }

}


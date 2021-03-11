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
    public class TransferController : ApiController
    {
        //string Value = "";
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Conn"].ConnectionString);
        string query = ""; SqlCommand dbcommand;


        [Route("api/Nepro/ScanTransferStillage")]
        [HttpPost]
        public ScanTransferStillage ScanTransferStillage(StickerReq SR)
        {
            ScanTransferStillage STS = new ScanTransferStillage();
            try
            {
                if (SR.StickerNo == "")
                {
                    STS.Status = "Failure";
                    STS.Message = "Enter Sticker ID";
                }
                if (SR.UserId == 0)
                {
                    STS.Status = "Failure";
                    STS.Message = "Invalid UserId";
                }
                query = "Sp_TransferWebApi";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.Connection.Open();
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.Parameters.AddWithValue("@QueryType", "ScanTrasnferStillage");
                dbcommand.Parameters.AddWithValue("@StickerId", SR.StickerNo);
                dbcommand.Parameters.AddWithValue("@UserId", SR.UserId);

                dbcommand.CommandTimeout = 0;
                SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                DataSet dt = new DataSet();
                da.Fill(dt);
                if (dt != null && dt.Tables[0].Rows.Count > 0)
                {
                    if(Convert.ToBoolean(dt.Tables[0].Rows[0]["IsAssignTransfer"])==true)
                    {
                         STS.Status = "Failure";
                        STS.Message = "This stillage is already asssigned..!";
                    }
                    else
                    { 
                    STS.StickerID = dt.Tables[0].Rows[0]["StickerID"].ToString();
                    STS.StandardQty = Convert.ToDecimal(dt.Tables[0].Rows[0]["StandardQty"]);
                    STS.ItemId = dt.Tables[0].Rows[0]["ItemId"].ToString();
                    STS.Description = dt.Tables[0].Rows[0]["Description"].ToString();
                    STS.ItemStdQty = Convert.ToDecimal(dt.Tables[0].Rows[0]["ItemStdQty"]);
                    STS.Location = dt.Tables[0].Rows[0]["Location"].ToString();
                    STS.IsTransfered = Convert.ToByte(dt.Tables[0].Rows[0]["IsTransfered"]);
                    STS.WareHouseID = dt.Tables[0].Rows[0]["WareHouseID"].ToString();
                    STS.WareHouseName = dt.Tables[0].Rows[0]["WareHouseName"].ToString();
                    STS.TransferId = dt.Tables[0].Rows[0]["TransferId"].ToString();
                    STS.IsShiped = dt.Tables[0].Rows[0]["IsShiped"].ToString();
                    STS.isHold = Convert.ToByte(dt.Tables[0].Rows[0]["isHold"]);
                    STS.IsCounted = Convert.ToByte(dt.Tables[0].Rows[0]["IsCounted"]);
                    STS.ToBeTransferWHID = dt.Tables[0].Rows[0]["ToBeTransferWHID"].ToString();
                    STS.WoStatus = dt.Tables[2].Rows[0]["WorkorderStatus"].ToString();
                    STS.IsTJ = Convert.ToByte(dt.Tables[0].Rows[0]["IsTJ"]);
                    List<SiteList> SiteList = new List<SiteList>();
                    foreach (DataRow row in dt.Tables[1].Rows)
                    {
                        SiteList A = new SiteList();
                        A.id = row["id"].ToString();
                        A.name = row["name"].ToString();
                        SiteList.Add(A);
                    }

                    STS.SiteListData = SiteList;
                    List<FLTList> FLTList = new List<FLTList>();
                    foreach (DataRow row in dt.Tables[3].Rows)
                    {
                        FLTList A = new FLTList();
                        A.id = row["id"].ToString();
                        A.name = row["name"].ToString();
                        FLTList.Add(A);
                    }
                    STS.FLTList = FLTList;
                    STS.SiteListData = SiteList;
                    STS.Status = "Success";
                    STS.Message = "Data retrived successfully";
                }
                }
                else
                {
                    STS.Status = "Failure";
                    STS.Message = "This stillage does not exist";
                    return STS;
                }
            }
            catch (Exception ex)
            {
                STS.Status = "Failure";
                STS.Message = ex.Message;
            }
            finally
            {
                dbcommand.Connection.Close();
            }
            return STS;
        }



        [Route("api/Nepro/UpdateTransferStillage")]
        [HttpPost]
        public UpdateTrasnfer UpdateTrasnferStillage(TrasnferReq SR)
        {
            UpdateTrasnfer UT = new UpdateTrasnfer();
            var TraId = "";
            try
            {
                if (SR.StickerNo == "")
                {
                    UT.Status = "Failure";
                    UT.Message = "Enter Sticker ID";
                }
                if (SR.UserId == 0)
                {
                    UT.Status = "Failure";
                    UT.Message = "Invalid UserId";
                }

                query = "Sp_TransferWebApi";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.CommandTimeout = 0;
                dbcommand.Parameters.AddWithValue("@QueryType", "DataTransferHeader");
                //dbcommand.Parameters.AddWithValue("@StillageID", SR.StickerNo);
                dbcommand.Parameters.AddWithValue("@UserId", SR.UserId);
                SqlDataAdapter daGetData = new SqlDataAdapter(dbcommand);
                DataSet dsGetData = new DataSet();
                daGetData.Fill(dsGetData);

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
                var Value = obj.InsertTransferHeader(Cct, SR.StickerNo, SR.WareHouseID, Convert.ToString(dsGetData.Tables[0].Rows[0]["UserName"]), SR.StickerNo);
                TraId = Value;
                obj.InsertTransferOrderLines(Cct, Value, SR.StickerNo);

                query = "Sp_TransferWebApi";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.CommandTimeout = 0;
                dbcommand.Parameters.AddWithValue("@QueryType", "FetchDataTransfer");
                dbcommand.Parameters.AddWithValue("@StickerId", SR.StickerNo);
                dbcommand.Parameters.AddWithValue("@UserId", SR.UserId);
                daGetData = new SqlDataAdapter(dbcommand);
                dsGetData = new DataSet();
                daGetData.Fill(dsGetData);
                da = new SqlDataAdapter(dbcommand);
                var Hold = "";
                if (dsGetData.Tables[0].Rows[0]["isHold"].ToString() == "True")
                {
                    Hold = "Qc Hold";
                }
                else
                {
                    Hold = "Qc Release";
                }


                string value = obj.InsertHistoryHeaderData(Cct, SR.StickerNo, Convert.ToString(dsGetData.Tables[0].Rows[0]["SiteID"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ItemId"]), Convert.ToDecimal(dsGetData.Tables[0].Rows[0]["WorkOrderQty"]));
                //obj.InsertHistoryDetailData(Cct, SR.StickerNo, "", Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityName"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityDesc"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), "", "", "", "", "", "", "No", 0, "", 0, 0, Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["StillageQty"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]), Hold, 0, "", Convert.ToString(dsGetData.Tables[1].Rows[0]["FromWareHouseId"]), SR.WareHouseID, Convert.ToString(dsGetData.Tables[1].Rows[0]["FromWareHouseId"]),0);
                obj.InsertHistoryDetailData(Cct, SR.StickerNo, "", Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityName"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityDesc"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), "", "", "", "", "", "", "No", 0, "", 0, 0, Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["StillageQty"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]), Hold, 0, "", Convert.ToString(dsGetData.Tables[1].Rows[0]["FromWareHouseId"]), SR.WareHouseID, Convert.ToString(dsGetData.Tables[1].Rows[0]["FromWareHouseId"]));



                query = "Sp_TransferWebApi";
                dbcommand = new SqlCommand(query, conn);
                //dbcommand.Connection.Open();
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.Parameters.AddWithValue("@QueryType", "UpdateTransfer");
                dbcommand.Parameters.AddWithValue("@StickerId", SR.StickerNo);
                dbcommand.Parameters.AddWithValue("@UserId", SR.UserId);
                dbcommand.Parameters.AddWithValue("@WareHouseID", SR.WareHouseID);
                dbcommand.Parameters.AddWithValue("@TransferID", Value);
                dbcommand.CommandTimeout = 0;
                da = new SqlDataAdapter(dbcommand);
                DataTable dt = new DataTable();
                da.Fill(dt);
                UT.TransferId= "";
                if (dt.Rows[0]["value"].ToString() == "2")
                {
                    UT.Status = "Success";
                    UT.Message = "Stillage already transfered";
                    return UT;
                }
                obj.TransferOrderPickingList(Cct, Value);
                obj.shipTransferOrder(Cct, Value);

                string result = UpdateShipping(Value, SR.StickerNo);
                if (result == "1")
                {
                    UT.Status = "Success";
                    UT.Message = "Stillage transfered successfully";
                }
                
                else
                {
                    UT.Status = "Failure";
                    UT.Message = "Stillage transfered Failure";
                }


            }
            catch (Exception Ex)
            {
                UT.Status = "Failure";
                UT.Message = Ex.Message;
                UT.TransferId = TraId;
            }
            finally
            {
                dbcommand.Connection.Close();
            }
            return UT;
        }


        [Route("api/Nepro/ScanRecievedTransfer")]
        [HttpPost]
        public RecievedTransferDetails GetRecievedTransferStillage(StickerReq SR)
        {
            RecievedTransferDetails RR = new RecievedTransferDetails();
            try
            {
                if (SR.StickerNo == "" || SR.StickerNo == null)
                {
                    RR.Status = "Failure";
                    RR.Message = "Enter Sticker ID";
                }
                if (SR.UserId == 0)
                {
                    RR.Status = "Failure";
                    RR.Message = "Invalid UserId";
                }
                query = "Sp_TransferWebApi";
                dbcommand = new SqlCommand(query, conn);
                //dbcommand.Connection.Open();
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.Parameters.AddWithValue("@QueryType", "GetTranseredDetails");
                dbcommand.Parameters.AddWithValue("@StickerId", SR.StickerNo);
                dbcommand.Parameters.AddWithValue("@UserId", SR.UserId);

                dbcommand.CommandTimeout = 0;
                SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt != null && dt.Rows.Count > 0)
                {
                    RR.StickerID = dt.Rows[0]["StickerID"].ToString();
                    RR.StandardQty = Convert.ToDecimal(dt.Rows[0]["StandardQty"]);
                    RR.ItemId = dt.Rows[0]["ItemId"].ToString();
                    RR.Description = dt.Rows[0]["Description"].ToString();
                    RR.ItemStdQty = Convert.ToDecimal(dt.Rows[0]["ItemStdQty"]);
                    RR.IsRecieved = Convert.ToByte(dt.Rows[0]["IsRecieved"]);
                    RR.WareHouseID= dt.Rows[0]["WareHouseID"].ToString();
                    RR.SiteID= dt.Rows[0]["SiteID"].ToString();
                    RR.Status = "Success";
                    RR.Message = "Data retrived successfully";
                }
                else
                {
                    RR.Status = "Failure";
                    RR.Message = "This stillage does not exist";
                    return RR;
                }
            }
            catch (Exception Ex)
            {
                RR.Status = "Failure";
                RR.Message = Ex.Message;
            }
          
            finally
            {
                dbcommand.Connection.Close();
            }
            return RR;
        }

        [Route("api/Nepro/UpdateRecievedTransfer")]
        [HttpPost]
        public RecievedTransferDetails UpdateRecievedTransferStillage(StickerReq SR)
        {
            RecievedTransferDetails RR = new RecievedTransferDetails();
            try
            {
                if (SR.StickerNo == "")
                {
                    RR.Status = "Failure";
                    RR.Message = "Enter Sticker ID";
                }
                if (SR.UserId == 0)
                {
                    RR.Status = "Failure";
                    RR.Message = "Invalid UserId";
                }

                query = "Sp_TransferWebApi";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.CommandTimeout = 0;
                dbcommand.Parameters.AddWithValue("@QueryType", "CheckStillageIsAssignedOrNot");
                dbcommand.Parameters.AddWithValue("@StickerId", SR.StickerNo);
                dbcommand.Parameters.AddWithValue("@UserId", SR.UserId);
                SqlDataAdapter da1 = new SqlDataAdapter(dbcommand);
                DataSet ds1 = new DataSet();
                da1.Fill(ds1);
                if (ds1.Tables[0].Rows[0]["value"].ToString() == "2")
                {
                    query = "Sp_TransferWebApi";
                    dbcommand = new SqlCommand(query, conn);
                    dbcommand.CommandType = CommandType.StoredProcedure;
                    dbcommand.CommandTimeout = 0;
                    dbcommand.Parameters.AddWithValue("@QueryType", "FetchDataRecieve");
                    dbcommand.Parameters.AddWithValue("@StickerId", SR.StickerNo);
                    dbcommand.Parameters.AddWithValue("@UserId", SR.UserId);
                    SqlDataAdapter daGetData1 = new SqlDataAdapter(dbcommand);
                    DataSet dsGetData1 = new DataSet();
                    daGetData1.Fill(dsGetData1);
                    SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                    var Hold = "";
                    if (dsGetData1.Tables[0].Rows[0]["isHold"].ToString() == "True")
                    {
                        Hold = "Qc Hold";
                    }
                    else
                    {
                        Hold = "Qc Release";
                    }

                    query = "Sp_TransferWebApi";
                    dbcommand = new SqlCommand(query, conn);
                    dbcommand.CommandType = CommandType.StoredProcedure;
                    dbcommand.CommandTimeout = 0;
                    dbcommand.Parameters.AddWithValue("@QueryType", "DataReceive");
                    dbcommand.Parameters.AddWithValue("@StickerId", SR.StickerNo);
                    dbcommand.Parameters.AddWithValue("@UserId", SR.UserId);
                    SqlDataAdapter daGetData = new SqlDataAdapter(dbcommand);
                    DataSet dsGetData = new DataSet();
                    daGetData.Fill(dsGetData);
                    da = new SqlDataAdapter(dbcommand);

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

                        obj.RegisterTransferOrder(Cct, Convert.ToString(dsGetData.Tables[0].Rows[0]["TransferID"]), SR.StickerNo);
                        obj.RecieveTransferOrder(Cct, Convert.ToString(dsGetData.Tables[0].Rows[0]["TransferID"]), SR.StickerNo);


                        string value = obj.InsertHistoryHeaderData(Cct, SR.StickerNo, Convert.ToString(dsGetData1.Tables[0].Rows[0]["SiteID"]), Convert.ToString(dsGetData1.Tables[0].Rows[0]["WorkOrderNo"]), Convert.ToString(dsGetData1.Tables[0].Rows[0]["ItemId"]), Convert.ToDecimal(dsGetData1.Tables[0].Rows[0]["WorkOrderQty"]));
                        //obj.InsertHistoryDetailData(Cct, SR.StickerNo, "", "Recieve", "Recieve", Convert.ToString(dsGetData1.Tables[2].Rows[0]["ZONEID"]), "", "", "", "", "", "", "No", 0, "", 0, 0, Convert.ToDecimal(dsGetData1.Tables[1].Rows[0]["StillageQty"]), Convert.ToString(dsGetData1.Tables[1].Rows[0]["UserName"]), Hold, 0, "", "", "", Convert.ToString(dsGetData1.Tables[1].Rows[0]["FromWareHouseID"]), 0);
                        obj.InsertHistoryDetailData(Cct, SR.StickerNo, "", "Recieve", "Recieve", Convert.ToString(dsGetData1.Tables[2].Rows[0]["ZONEID"]), "", "", "", "", "", "", "No", 0, "", 0, 0, Convert.ToDecimal(dsGetData1.Tables[1].Rows[0]["StillageQty"]), Convert.ToString(dsGetData1.Tables[1].Rows[0]["UserName"]), Hold, 0, "", "", "", Convert.ToString(dsGetData1.Tables[1].Rows[0]["FromWareHouseID"]));



                    }
                    else
                    {
                        RR.Status = "Failure";
                        RR.Message = "This Stillage Does Not Exist";
                        return RR;
                    }

                    query = "Sp_TransferWebApi";
                    dbcommand = new SqlCommand(query, conn);
                    //dbcommand.Connection.Open();
                    dbcommand.CommandType = CommandType.StoredProcedure;
                    dbcommand.Parameters.AddWithValue("@QueryType", "UpdateRecievedStillage");
                    dbcommand.Parameters.AddWithValue("@StickerId", SR.StickerNo);
                    dbcommand.Parameters.AddWithValue("@UserId", SR.UserId);
                    dbcommand.Parameters.AddWithValue("@ZoneId", Convert.ToString(dsGetData1.Tables[2].Rows[0]["ZONEID"]));

                    dbcommand.CommandTimeout = 0;
                    da = new SqlDataAdapter(dbcommand);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    if (dt.Rows[0]["value"].ToString() == "1")
                    {

                        query = "Sp_TransferWebApi";
                        dbcommand = new SqlCommand(query, conn);
                        //dbcommand.Connection.Open();
                        dbcommand.CommandType = CommandType.StoredProcedure;
                        dbcommand.Parameters.AddWithValue("@QueryType", "ResetDataAfterTransfer");
                        dbcommand.Parameters.AddWithValue("@StickerId", SR.StickerNo);

                        dbcommand.CommandTimeout = 0;
                        da = new SqlDataAdapter(dbcommand);
                        DataTable dt3 = new DataTable();
                        da.Fill(dt3);

                        RR.Status = "Success";
                        RR.Message = "Stillage recieved successfully";
                    }
                    else if (dt.Rows[0]["value"].ToString() == "2")
                    {
                        RR.Status = "Failure";
                        RR.Message = "Stillage already recieved";
                    }
                    else
                    {
                        RR.Status = "Failure";
                        RR.Message = "Invalid Sticker ID.!";
                    }
                }
                else
                {
                    if (ds1.Tables[1].Rows[0]["Count"].ToString() != "1")
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
                        obj.UpdateTransferStatus(Cct, SR.StickerNo, ds1.Tables[2].Rows[0]["TransferID"].ToString());
                        query = "Sp_TransferWebApi";
                        dbcommand = new SqlCommand(query, conn);
                        dbcommand.CommandType = CommandType.StoredProcedure;
                        dbcommand.CommandTimeout = 0;
                        dbcommand.Parameters.AddWithValue("@QueryType", "UpdateAssignStillage");
                        dbcommand.Parameters.AddWithValue("@StickerId", SR.StickerNo);
                        dbcommand.Parameters.AddWithValue("@UserId", SR.UserId);
                        SqlDataAdapter da4 = new SqlDataAdapter(dbcommand);
                        DataSet ds4 = new DataSet();
                        da4.Fill(ds4);
                        RR.Status = "Success";
                        RR.Message = "Stillage Recieved Successfully..!";
                    }
                    else
                    {
                        int l = 0;
                        string ZoneID = "";
                        for (l = 0; l < ds1.Tables[2].Rows.Count; l++)
                        {
                            query = "Sp_TransferWebApi";
                            dbcommand = new SqlCommand(query, conn);
                            dbcommand.CommandType = CommandType.StoredProcedure;
                            dbcommand.CommandTimeout = 0;
                            dbcommand.Parameters.AddWithValue("@QueryType", "FetchDataRecieve");
                            dbcommand.Parameters.AddWithValue("@StickerId", ds1.Tables[2].Rows[l]["StickerId"].ToString());
                            dbcommand.Parameters.AddWithValue("@UserId", SR.UserId);
                            SqlDataAdapter daGetData1 = new SqlDataAdapter(dbcommand);
                            DataSet dsGetData1 = new DataSet();
                            daGetData1.Fill(dsGetData1);
                            SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                            var Hold = "";
                            if (dsGetData1.Tables[0].Rows[0]["isHold"].ToString() == "True")
                            {
                                Hold = "Qc Hold";
                            }
                            else
                            {
                                Hold = "Qc Release";
                            }

                            query = "Sp_TransferWebApi";
                            dbcommand = new SqlCommand(query, conn);
                            dbcommand.CommandType = CommandType.StoredProcedure;
                            dbcommand.CommandTimeout = 0;
                            dbcommand.Parameters.AddWithValue("@QueryType", "DataReceive");
                            dbcommand.Parameters.AddWithValue("@StickerId", ds1.Tables[2].Rows[l]["StickerId"].ToString());
                            dbcommand.Parameters.AddWithValue("@UserId", SR.UserId);
                            SqlDataAdapter daGetData = new SqlDataAdapter(dbcommand);
                            DataSet dsGetData = new DataSet();
                            daGetData.Fill(dsGetData);
                            da = new SqlDataAdapter(dbcommand);

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
                                obj.UpdateTransferStatus(Cct, ds1.Tables[2].Rows[l]["StickerId"].ToString(), dsGetData.Tables[0].Rows[0]["TransferID"].ToString());
                                if (l == ds1.Tables[2].Rows.Count-1)
                                {
                                    obj.RegisterTransferOrder(Cct, Convert.ToString(dsGetData.Tables[0].Rows[0]["TransferID"]), ds1.Tables[2].Rows[l]["StickerId"].ToString());
                                    obj.RecieveTransferOrder(Cct, Convert.ToString(dsGetData.Tables[0].Rows[0]["TransferID"]), ds1.Tables[2].Rows[l]["StickerId"].ToString());
                                }
                                string value = obj.InsertHistoryHeaderData(Cct, ds1.Tables[2].Rows[l]["StickerId"].ToString(), Convert.ToString(dsGetData1.Tables[0].Rows[0]["SiteID"]), Convert.ToString(dsGetData1.Tables[0].Rows[0]["WorkOrderNo"]), Convert.ToString(dsGetData1.Tables[0].Rows[0]["ItemId"]), Convert.ToDecimal(dsGetData1.Tables[0].Rows[0]["WorkOrderQty"]));
                               // obj.InsertHistoryDetailData(Cct, ds1.Tables[2].Rows[l]["StickerId"].ToString(), "", "Recieve", "Recieve", Convert.ToString(dsGetData1.Tables[2].Rows[0]["ZONEID"]), "", "", "", "", "", "", "No", 0, "", 0, 0, Convert.ToDecimal(dsGetData1.Tables[1].Rows[0]["StillageQty"]), Convert.ToString(dsGetData1.Tables[1].Rows[0]["UserName"]), Hold, 0, "", "", "", Convert.ToString(dsGetData1.Tables[4].Rows[0]["ToWareHouseId"]), 0);
                                obj.InsertHistoryDetailData(Cct, ds1.Tables[2].Rows[l]["StickerId"].ToString(), "", "Recieve", "Recieve", Convert.ToString(dsGetData1.Tables[2].Rows[0]["ZONEID"]), "", "", "", "", "", "", "No", 0, "", 0, 0, Convert.ToDecimal(dsGetData1.Tables[1].Rows[0]["StillageQty"]), Convert.ToString(dsGetData1.Tables[1].Rows[0]["UserName"]), Hold, 0, "", "", "", Convert.ToString(dsGetData1.Tables[4].Rows[0]["ToWareHouseId"]));

                                ZoneID = Convert.ToString(dsGetData1.Tables[2].Rows[0]["ZONEID"]);

                            }
                            else
                            {
                                RR.Status = "Failure";
                                RR.Message = "This Stillage Does Not Exist";
                                return RR;
                            }

                            query = "Sp_TransferWebApi";
                            dbcommand = new SqlCommand(query, conn);
                            //dbcommand.Connection.Open();
                            dbcommand.CommandType = CommandType.StoredProcedure;
                            dbcommand.Parameters.AddWithValue("@QueryType", "UpdateRecievedStillage");
                            dbcommand.Parameters.AddWithValue("@StickerId", ds1.Tables[2].Rows[l]["StickerId"].ToString());
                            dbcommand.Parameters.AddWithValue("@UserId", SR.UserId);
                            //dbcommand.Parameters.AddWithValue("@TransID", dsGetData.Tables[0].Rows[0]["TransferID"]);
                            dbcommand.Parameters.AddWithValue("@ZoneId", ZoneID);

                            dbcommand.CommandTimeout = 0;
                            SqlDataAdapter da3 = new SqlDataAdapter(dbcommand);
                            DataTable dt = new DataTable();
                            da3.Fill(dt);
                            if (dt.Rows[0]["value"].ToString() == "1")
                            {
                                RR.Status = "Success";
                                RR.Message = "Stillage recieved successfully";
                            }
                            else if (dt.Rows[0]["value"].ToString() == "2")
                            {
                                RR.Status = "Failure";
                                RR.Message = "Stillage already recieved";
                            }
                            else
                            {
                                RR.Status = "Failure";
                                RR.Message = "Invalid Sticker ID.!";
                            }

                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                RR.Status = "Failure";
                RR.Message = Ex.Message;
            }

            //finally
            //{
            //    dbcommand.Connection.Close();
            //}
            return RR;
        }



        [Route("api/Nepro/ShipTransfer")]
        [HttpPost]
        public ShipRes ShipTransfer(ShipReq SR)
        {
            ShipRes RR = new ShipRes();
            //int Ship = 0;
            try
            {
                if (SR.StickerNo == "" || SR.StickerNo == null)
                {
                    RR.Status = "Failure";
                    RR.Message = "Enter Sticker ID";
                }
                if (SR.UserId == "" || SR.UserId == null)
                {
                    RR.Status = "Failure";
                    RR.Message = "Invalid UserId";
                }


                query = "Sp_TransferWebApi";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.CommandTimeout = 0;
                dbcommand.Parameters.AddWithValue("@QueryType", "FetchDataTransfer");
                dbcommand.Parameters.AddWithValue("@StickerId", SR.StickerNo);
                dbcommand.Parameters.AddWithValue("@UserId", SR.UserId);
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


                query = "Sp_AxWebserviceIntegration";
                    dbcommand = new SqlCommand(query, conn);
                    //dbcommand.Connection.Open();
                    dbcommand.CommandType = CommandType.StoredProcedure;
                    dbcommand.CommandTimeout = 0;
                    da = new SqlDataAdapter(dbcommand);
                    DataSet ds = new DataSet();
                    da.Fill(ds);

                    //  ds = CommonManger.FillDatasetWithParam("Sp_AxWebserviceIntegration");
                    AXWebServiceRef1.Iace_FinishedGoodServiceClient obj = new AXWebServiceRef1.Iace_FinishedGoodServiceClient();
                    obj.ClientCredentials.Windows.ClientCredential.Domain = Convert.ToString(ds.Tables[0].Rows[0]["Domain"]);
                    obj.ClientCredentials.Windows.ClientCredential.UserName = Convert.ToString(ds.Tables[0].Rows[0]["Username"]);
                    obj.ClientCredentials.Windows.ClientCredential.Password = Convert.ToString(ds.Tables[0].Rows[0]["Password"]);

                    AXWebServiceRef1.CallContext Cct = new AXWebServiceRef1.CallContext();
                    Cct.Company = Convert.ToString(ds.Tables[0].Rows[0]["Company"]);
                    Cct.Language = Convert.ToString(ds.Tables[0].Rows[0]["Language"]);

                    obj.TransferOrderPickingList(Cct, SR.TransferId);
                    obj.shipTransferOrder(Cct, SR.TransferId);



                string value = obj.InsertHistoryHeaderData(Cct, SR.StickerNo, Convert.ToString(dsGetData.Tables[0].Rows[0]["SiteID"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ItemId"]), Convert.ToDecimal(dsGetData.Tables[0].Rows[0]["WorkOrderQty"]));
               // obj.InsertHistoryDetailData(Cct, SR.StickerNo, "", Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityName"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityDesc"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), "", "", "", "", "", "", "No", 0, "", 0, 0, Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["StillageQty"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]), Hold, 0, "", Convert.ToString(dsGetData.Tables[2].Rows[0]["WareHouseID"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["FromWareHouseId"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["WareHouseID"]),0);
                obj.InsertHistoryDetailData(Cct, SR.StickerNo, "", Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityName"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityDesc"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), "", "", "", "", "", "", "No", 0, "", 0, 0, Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["StillageQty"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]), Hold, 0, "", Convert.ToString(dsGetData.Tables[2].Rows[0]["WareHouseID"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["FromWareHouseId"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["WareHouseID"]));



                string result = UpdateShipping( SR.TransferId, SR.StickerNo);

                if (result == "1")
                {
                    RR.Status = "Success";
                    RR.Message = "Shipped successfully";
                }
                else
                {
                    RR.Status = "Failure";
                    RR.Message = "Shipped Failure";
                }
            }
            catch (Exception Ex)
            {
                RR.Status = "Failure";
                RR.Message = Ex.Message;
            }

            //finally
            //{
            //    dbcommand.Connection.Close();
            //}
            return RR;
        }


        public string UpdateShipping(string TransId,string StickerNo)
        {
            query = "Sp_TransferWebApi";
            dbcommand = new SqlCommand(query, conn);
            //dbcommand.Connection.Open();
            dbcommand.CommandType = CommandType.StoredProcedure;
            dbcommand.Parameters.AddWithValue("@QueryType", "UpdateShip");
            dbcommand.Parameters.AddWithValue("@StickerId", StickerNo);
            dbcommand.Parameters.AddWithValue("@TransferID", TransId);
            dbcommand.CommandTimeout = 0;
            SqlDataAdapter da = new SqlDataAdapter(dbcommand);
            DataTable dt = new DataTable();
            da.Fill(dt);
            string result = dt.Rows[0]["value"].ToString();
            return result;
        }



        [Route("api/Nepro/WareHouseData")]
        [HttpPost]
        public WareHouseDataRes WareHouseDta(WareHouseDataReq WHDR)
        {
            WareHouseDataRes RR = new WareHouseDataRes();
            try
            {
                if (WHDR.SiteId == "" || WHDR.SiteId == null)
                {
                    RR.Status = "Failure";
                    RR.Message = "Enter SiteId";
                }
                
                query = "Sp_TransferWebApi";
                dbcommand = new SqlCommand(query, conn);
                //dbcommand.Connection.Open();
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.Parameters.AddWithValue("@QueryType", "GetWareHouseData");
                dbcommand.Parameters.AddWithValue("@SiteId", WHDR.SiteId);
                dbcommand.CommandTimeout = 0;
                SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt != null && dt.Rows.Count > 0)
                {

                    List<WareHouseList> WareHouseList = new List<WareHouseList>();
                    foreach (DataRow row in dt.Rows)
                    {
                        WareHouseList A = new WareHouseList();
                        A.id = row["id"].ToString();
                        A.name = row["name"].ToString();
                        WareHouseList.Add(A);
                    }
                    RR.WareHouseListData = WareHouseList;

                    RR.Status = "Success";
                    RR.Message = "Stillage retrived successfully";
                }
                else
                {
                    RR.Status = "Failure";
                    RR.Message = "Record Not Found";
                    return RR;
                }
            }
            catch (Exception Ex)
            {
                RR.Status = "Failure";
                RR.Message = Ex.Message;
            }

            finally
            {
                dbcommand.Connection.Close();
            }
            return RR;
        }

        [Route("api/Nepro/NewTranferStillage")]
        [HttpPost]
        public UpdateTrasnfer NewTranferStillage(TransferRequest SR)
        {
            int i = 0;
                string TraId = "";
            UpdateTrasnfer UT = new UpdateTrasnfer();

            int count = SR.StillageDetailsData.Count();
           string StillageNotSH ="";
            string StillageID = "";
            int flag = 0;

            if (SR.TJ == "0") {

                for (i = 0; i < count; i++)
                {
                    flag = 0;
                    try
                    {
                        StillageID = SR.StillageDetailsData[i].StillageID.ToString();
                        query = "Sp_TransferWebApi";
                        dbcommand = new SqlCommand(query, conn);
                        dbcommand.CommandType = CommandType.StoredProcedure;
                        dbcommand.CommandTimeout = 0;
                        dbcommand.Parameters.AddWithValue("@QueryType", "DataTransferHeader");
                        //dbcommand.Parameters.AddWithValue("@StillageID", SR.StickerNo);
                        dbcommand.Parameters.AddWithValue("@UserId", SR.UserId);
                        SqlDataAdapter daGetData = new SqlDataAdapter(dbcommand);
                        DataSet dsGetData = new DataSet();
                        daGetData.Fill(dsGetData);

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
                        if (SR.StillageDetailsData[i].TransID.ToString() == "")
                        {
                            var Value = obj.InsertTransferHeader(Cct, StillageID, SR.WareHouseID, Convert.ToString(dsGetData.Tables[0].Rows[0]["UserName"]), StillageID);
                            TraId = Value;
                           obj.InsertTransferOrderLines(Cct, Value, StillageID);
                          
                            flag = 1;


                            query = "Sp_TransferWebApi";
                            dbcommand = new SqlCommand(query, conn);
                            //dbcommand.Connection.Open();
                            dbcommand.CommandType = CommandType.StoredProcedure;
                            dbcommand.Parameters.AddWithValue("@QueryType", "UpdateTransfer");
                            dbcommand.Parameters.AddWithValue("@StickerId", StillageID);
                            dbcommand.Parameters.AddWithValue("@UserId", SR.UserId);
                            dbcommand.Parameters.AddWithValue("@WareHouseID", SR.WareHouseID);
                            dbcommand.Parameters.AddWithValue("@ZoneId", SR.LocationId);
                            dbcommand.Parameters.AddWithValue("@TransferID", Value);
                            dbcommand.CommandTimeout = 0;
                            da = new SqlDataAdapter(dbcommand);
                            DataTable dt = new DataTable();
                            da.Fill(dt);
                            UT.TransferId = "";
                            //if (dt.Rows[0]["value"].ToString() == "2")
                            //{
                            //    UT.Status = "Success";
                            //    UT.Message = "Stillage already transfered";
                            //    UT.StillageNotSH = StillageNotSH;
                            //    return UT;
                            //}
                            obj.TransferOrderPickingList(Cct, Value);
                            obj.shipTransferOrder(Cct, Value);

                            query = "Sp_TransferWebApi";
                            dbcommand = new SqlCommand(query, conn);
                            dbcommand.CommandType = CommandType.StoredProcedure;
                            dbcommand.CommandTimeout = 0;
                            dbcommand.Parameters.AddWithValue("@QueryType", "FetchDataTransfer");
                            dbcommand.Parameters.AddWithValue("@StickerId", StillageID);
                            dbcommand.Parameters.AddWithValue("@UserId", SR.UserId);
                            daGetData = new SqlDataAdapter(dbcommand);
                            dsGetData = new DataSet();
                            daGetData.Fill(dsGetData);
                            da = new SqlDataAdapter(dbcommand);
                            var Hold = "";
                            if (dsGetData.Tables[0].Rows[0]["isHold"].ToString() == "True")
                            {
                                Hold = "Qc Hold";
                            }
                            else
                            {
                                Hold = "Qc Release";
                            }

                            string result = UpdateShipping(Value, StillageID);
                            string value = obj.InsertHistoryHeaderData(Cct, StillageID, Convert.ToString(dsGetData.Tables[0].Rows[0]["SiteID"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ItemId"]), Convert.ToDecimal(dsGetData.Tables[0].Rows[0]["WorkOrderQty"]));
                           // obj.InsertHistoryDetailData(Cct, StillageID, "", Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityName"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityDesc"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), "", "", "", "", "", "", "Yes", 0, "", 0, 0, Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["StillageQty"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]), Hold, 0, "", Convert.ToString(dsGetData.Tables[1].Rows[0]["FromWareHouseId"]), SR.WareHouseID, Convert.ToString(dsGetData.Tables[1].Rows[0]["FromWareHouseId"]),0);
                            obj.InsertHistoryDetailData(Cct, StillageID, "", Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityName"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityDesc"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), "", "", "", "", "", "", "Yes", 0, "", 0, 0, Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["StillageQty"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]), Hold, 0, "", Convert.ToString(dsGetData.Tables[1].Rows[0]["FromWareHouseId"]), SR.WareHouseID, Convert.ToString(dsGetData.Tables[1].Rows[0]["FromWareHouseId"]));


                            if (result == "1")
                            {
                                UT.Status = "Success";
                                UT.Message = "Stillage transfered successfully";
                                UT.StillageNotSH = StillageNotSH;
                            }

                            else
                            {
                                UT.Status = "Failure";
                                UT.Message = "Stillage transfered Failure";
                                UT.StillageNotSH = StillageNotSH;
                            }
                        }

                        else
                        {
                            flag = 3;
                            obj.TransferOrderPickingList(Cct, SR.StillageDetailsData[i].TransID.ToString());

                            obj.shipTransferOrder(Cct, SR.StillageDetailsData[i].TransID.ToString());

                            query = "Sp_TransferWebApi";
                            dbcommand = new SqlCommand(query, conn);
                            dbcommand.CommandType = CommandType.StoredProcedure;
                            dbcommand.CommandTimeout = 0;
                            dbcommand.Parameters.AddWithValue("@QueryType", "FetchDataTransfer");
                            dbcommand.Parameters.AddWithValue("@StickerId", StillageID);
                            dbcommand.Parameters.AddWithValue("@UserId", SR.UserId);
                            daGetData = new SqlDataAdapter(dbcommand);
                            dsGetData = new DataSet();
                            daGetData.Fill(dsGetData);
                            da = new SqlDataAdapter(dbcommand);
                            var Hold = "";
                            if (dsGetData.Tables[0].Rows[0]["isHold"].ToString() == "True")
                            {
                                Hold = "Qc Hold";
                            }
                            else
                            {
                                Hold = "Qc Release";
                            }

                            string result = UpdateShipping(SR.StillageDetailsData[i].TransID.ToString(), StillageID);
                            string value = obj.InsertHistoryHeaderData(Cct, StillageID, Convert.ToString(dsGetData.Tables[0].Rows[0]["SiteID"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ItemId"]), Convert.ToDecimal(dsGetData.Tables[0].Rows[0]["WorkOrderQty"]));
                           // obj.InsertHistoryDetailData(Cct, StillageID, "", Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityName"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityDesc"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), "", "", "", "", "", "", "No", 0, "", 0, 0, Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["StillageQty"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]), Hold, 0, "", Convert.ToString(dsGetData.Tables[1].Rows[0]["FromWareHouseId"]), SR.WareHouseID, Convert.ToString(dsGetData.Tables[1].Rows[0]["FromWareHouseId"]),0);
                            obj.InsertHistoryDetailData(Cct, StillageID, "", Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityName"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityDesc"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), "", "", "", "", "", "", "No", 0, "", 0, 0, Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["StillageQty"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]), Hold, 0, "", Convert.ToString(dsGetData.Tables[1].Rows[0]["FromWareHouseId"]), SR.WareHouseID, Convert.ToString(dsGetData.Tables[1].Rows[0]["FromWareHouseId"]));


                            if (result == "1")
                            {
                                UT.Status = "Success";
                                UT.Message = "Stillage transfered successfully";
                                UT.StillageNotSH = StillageNotSH;
                            }

                            else
                            {
                                UT.Status = "Failure";
                                UT.Message = "Stillage transfered Failure";
                                UT.StillageNotSH = StillageNotSH;
                            }

                        }

                    }
                    catch (Exception ex)
                    {
                        if (flag == 1 || flag == 3) {
                            StillageNotSH = StillageNotSH + StillageID;
                            StillageNotSH = StillageNotSH + ",";
                        }
                        query = "Sp_TransferWebApi";
                        dbcommand = new SqlCommand(query, conn);
                        dbcommand.CommandType = CommandType.StoredProcedure;
                        dbcommand.CommandTimeout = 0;
                        dbcommand.Parameters.AddWithValue("@QueryType", "FetchDataTransfer");
                        dbcommand.Parameters.AddWithValue("@StickerId", StillageID);
                        dbcommand.Parameters.AddWithValue("@UserId", SR.UserId);
                       SqlDataAdapter daGetData = new SqlDataAdapter(dbcommand);
                      DataSet  dsGetData = new DataSet();
                        daGetData.Fill(dsGetData);
                      SqlDataAdapter  da = new SqlDataAdapter(dbcommand);
                        var Hold = "";
                        if (dsGetData.Tables[0].Rows[0]["isHold"].ToString() == "True")
                        {
                            Hold = "Qc Hold";
                        }
                        else
                        {
                            Hold = "Qc Release";
                        }
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
                        string value = obj.InsertHistoryHeaderData(Cct, StillageID, Convert.ToString(dsGetData.Tables[0].Rows[0]["SiteID"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ItemId"]), Convert.ToDecimal(dsGetData.Tables[0].Rows[0]["WorkOrderQty"]));
                        //obj.InsertHistoryDetailData(Cct, StillageID, "","Transfered", "Transfered", Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), "", "", "", "", "", "", "Yes", 0, "", 0, 0, Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["StillageQty"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]), Hold, 0, "", Convert.ToString(dsGetData.Tables[1].Rows[0]["FromWareHouseId"]), SR.WareHouseID, Convert.ToString(dsGetData.Tables[1].Rows[0]["FromWareHouseId"]),0);
                        obj.InsertHistoryDetailData(Cct, StillageID, "", "Transfered", "Transfered", Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), "", "", "", "", "", "", "Yes", 0, "", 0, 0, Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["StillageQty"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]), Hold, 0, "", Convert.ToString(dsGetData.Tables[1].Rows[0]["FromWareHouseId"]), SR.WareHouseID, Convert.ToString(dsGetData.Tables[1].Rows[0]["FromWareHouseId"]));

                    }

                }

            }
            else
            {

                for (i = 0; i < count; i++)
                {
                    try
                    {
                        StillageID = SR.StillageDetailsData[i].StillageID.ToString();
                        query = "Sp_TransferWebApi";
                        dbcommand = new SqlCommand(query, conn);
                        dbcommand.CommandType = CommandType.StoredProcedure;
                        dbcommand.CommandTimeout = 0;
                        dbcommand.Parameters.AddWithValue("@QueryType", "FetchDataTransfer");
                        dbcommand.Parameters.AddWithValue("@StickerId", StillageID);
                        dbcommand.Parameters.AddWithValue("@UserId", SR.UserId);
                        SqlDataAdapter daGetData = new SqlDataAdapter(dbcommand);
                        DataSet dsGetData = new DataSet();
                        daGetData.Fill(dsGetData);
                        var Hold = "";
                        if (dsGetData.Tables[0].Rows[0]["isHold"].ToString() == "True")
                        {
                            Hold = "Qc Hold";
                        }
                        else
                        {
                            Hold = "Qc Release";
                        }

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

                        //var Value = obj.InsertTransferHeader(Cct, StillageID, SR.WareHouseID, Convert.ToString(dsGetData.Tables[0].Rows[0]["UserName"]));
                        //TraId = Value;
                        //obj.InsertTransferOrderLines(Cct, Value, StillageID);

                        string journalId = obj.CreateTransferJournalHeader(Cct,"TJProcess", Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]), StillageID);
                        string result1=obj.CreateTransferJournalLines(Cct, journalId, Convert.ToString(dsGetData.Tables[1].Rows[0]["ItemId"]), Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["StillageQty"]),Convert.ToDateTime(DateTime.Now), Convert.ToString(dsGetData.Tables[1].Rows[0]["fromSiteID"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["FromWareHouseId"]), SR.SiteID, SR.WareHouseID, StillageID);
                        if (result1 == "success")
                        {
                            obj.PostTransferJournal(Cct, journalId);    
                                                  }
                        string value = obj.InsertHistoryHeaderData(Cct, StillageID, Convert.ToString(dsGetData.Tables[1].Rows[0]["SiteID"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ItemId"]), Convert.ToDecimal(dsGetData.Tables[0].Rows[0]["WorkOrderQty"]));

                       // obj.InsertHistoryDetailData(Cct, StillageID, "", "Transfered with TJ", "Transfered with TJ", SR.LocationId, "", "", "", "", "", "", "No", 0, "", 0, 0, Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["StillageQty"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]),Hold, 0, "", Convert.ToString(dsGetData.Tables[1].Rows[0]["FromWareHouseId"]), SR.WareHouseID, SR.WareHouseID,0);
                        obj.InsertHistoryDetailData(Cct, StillageID, "", "Transfered with TJ", "Transfered with TJ", SR.LocationId, "", "", "", "", "", "", "No", 0, "", 0, 0, Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["StillageQty"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]), Hold, 0, "", Convert.ToString(dsGetData.Tables[1].Rows[0]["FromWareHouseId"]), SR.WareHouseID, SR.WareHouseID);

                        query = "Sp_TransferWebApi";
                        dbcommand = new SqlCommand(query, conn);
                        //dbcommand.Connection.Open();
                        dbcommand.CommandType = CommandType.StoredProcedure;
                        dbcommand.Parameters.AddWithValue("@QueryType", "UpdateTransferTJprocess");
                        dbcommand.Parameters.AddWithValue("@StickerId", StillageID);
                        dbcommand.Parameters.AddWithValue("@UserId", SR.UserId);
                        dbcommand.Parameters.AddWithValue("@WareHouseID", SR.WareHouseID);
                        dbcommand.Parameters.AddWithValue("@TransferID", journalId);
                        dbcommand.Parameters.AddWithValue("@ZoneId", SR.LocationId);
                        dbcommand.CommandTimeout = 0;
                        da = new SqlDataAdapter(dbcommand);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        if (dt.Rows[0]["value"].ToString() == "1")
                        {

                            query = "Sp_TransferWebApi";
                            dbcommand = new SqlCommand(query, conn);
                            //dbcommand.Connection.Open();
                            dbcommand.CommandType = CommandType.StoredProcedure;
                            dbcommand.Parameters.AddWithValue("@QueryType", "ResetDataAfterTransfer");
                            dbcommand.Parameters.AddWithValue("@StickerId", StillageID);

                            dbcommand.CommandTimeout = 0;
                            da = new SqlDataAdapter(dbcommand);
                            DataTable dt3 = new DataTable();
                            da.Fill(dt3);

                            UT.Status = "Success";
                            UT.Message = "Stillage transfered successfully";
                            UT.StillageNotSH = StillageNotSH;
                            //  UT.TransferId = TraId;
                            return UT;
                            //UT.Status = "Success";
                            //UT.Message = "Stillage transfered successfully";
                            //return UT;
                        }

                    }
                    catch (Exception ex)
                    {
                        StillageID = SR.StillageDetailsData[i].StillageID.ToString();
                        query = "Sp_TransferWebApi";
                        dbcommand = new SqlCommand(query, conn);
                        dbcommand.CommandType = CommandType.StoredProcedure;
                        dbcommand.CommandTimeout = 0;
                        dbcommand.Parameters.AddWithValue("@QueryType", "FetchDataTransfer");
                        dbcommand.Parameters.AddWithValue("@StickerId", StillageID);
                        dbcommand.Parameters.AddWithValue("@UserId", SR.UserId);
                        SqlDataAdapter daGetData = new SqlDataAdapter(dbcommand);
                        DataSet dsGetData = new DataSet();
                        daGetData.Fill(dsGetData);
                        var Hold = "";
                        if (dsGetData.Tables[0].Rows[0]["isHold"].ToString() == "True")
                        {
                            Hold = "Qc Hold";
                        }
                        else
                        {
                            Hold = "Qc Release";
                        }

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
                        string value = obj.InsertHistoryHeaderData(Cct, StillageID, Convert.ToString(dsGetData.Tables[1].Rows[0]["SiteID"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ItemId"]), Convert.ToDecimal(dsGetData.Tables[0].Rows[0]["WorkOrderQty"]));

                        //obj.InsertHistoryDetailData(Cct, StillageID, "", "Transfered with TJ but not posted", "Transfered with TJ but not posted", Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), "", "", "", "", "", "", "No", 0, "", 0, 0, Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["StillageQty"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]), Hold, 0, "", Convert.ToString(dsGetData.Tables[1].Rows[0]["FromWareHouseId"]), SR.WareHouseID, Convert.ToString(dsGetData.Tables[1].Rows[0]["FromWareHouseId"]),0);
                        obj.InsertHistoryDetailData(Cct, StillageID, "", "Transfered with TJ but not posted", "Transfered with TJ but not posted", Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), "", "", "", "", "", "", "No", 0, "", 0, 0, Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["StillageQty"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]), Hold, 0, "", Convert.ToString(dsGetData.Tables[1].Rows[0]["FromWareHouseId"]), SR.WareHouseID, Convert.ToString(dsGetData.Tables[1].Rows[0]["FromWareHouseId"]));

                        UT.Status = "Failure";
                        UT.Message = ex.Message;
                        return UT;
                    }

                }


            }

             UT.Status = "Success";
            UT.Message = "Stillage transfered successfully";
            UT.StillageNotSH = StillageNotSH;
            //  UT.TransferId = TraId;
            return UT;

        }

    }
}


public class TransferRequest
{
    public List<StillageDetails> StillageDetailsData { get; set; }
    public string UserId { get; set; }
    public string WareHouseID { get; set; }
    public string TJ { get; set; }
    public string SiteID { get; set; }
    public string LocationId { get; set; }
}
public class StillageDetails
{
    public string StillageID { get; set; }
    public string TransID { get; set; }
}

public class WareHouseDataReq
{
  public string SiteId { get; set; }
}

public class WareHouseDataRes
{
    public  List<WareHouseList> WareHouseListData { get; set; }
    public string Status { get; set; }
    public string Message { get; set; }
}
public class WareHouseList
{
    public string id { get; set; }
    public string name { get; set; }
}



public class ShipReq
{
    public string StickerNo { get; set; }
    public string UserId { get; set; }
    public string TransferId { get; set; }
}

public class ShipRes
{
    public string Status { get; set; }
    public string Message { get; set; }
}


public class UpdateTrasnfer
{
    public string Status { get; set; }
    public string Message { get; set; }
    public string TransferId { get; set; }
    public string StillageNotSH { get; set; }
}

public class TrasnferReq
{
    public string StickerNo { get; set; }
    public Int64 UserId { get; set; }
    public string WareHouseID { get; set; }
}
public class RecievedTransferDetails
{
    public string StickerID { get; set; }
    public decimal StandardQty { get; set; }
    public string ItemId { get; set; }
    public string Description { get; set; }
    public decimal ItemStdQty { get; set; }
    public byte IsRecieved { get; set; }
    public string WareHouseID { get; set; }
    public string SiteID { get; set; }

    public string Status { get; set; }
    public string Message { get; set; }
}

public class FLTList {
    public string id { get; set; }
    public string name { get; set; }

}
public class ScanTransferStillage
{
    public List<FLTList> FLTList { get; set; }

    public List<SiteList> SiteListData { get; set; }
    public string StickerID { get; set; }
    public decimal StandardQty { get; set; }
    public string ItemId { get; set; }
    public string Description { get; set; }
    public decimal ItemStdQty { get; set; }
    public string WareHouseName { get; set; }
    public string Location { get; set; }
    public byte IsTransfered { get; set; }
    public string WareHouseID { get; set; }
    public string Status { get; set; }
    public string Message { get; set; }
    public string TransferId { get; set; }
    public string IsShiped { get; set; }
    public byte isHold { get; set; }
    public byte IsCounted { get; set; }
    public string ToBeTransferWHID { get; set; }
    public string WoStatus { get; set; }
    public byte IsTJ { get; set; }
}
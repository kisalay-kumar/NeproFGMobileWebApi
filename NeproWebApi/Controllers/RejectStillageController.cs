using FRD_InventoryWebApi.Controllers;
using NeproWebApi.Models;
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
    public class RejectStillageController : ApiController
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Conn"].ConnectionString);
        string query = ""; SqlCommand dbcommand;
        [Route("api/Nepro/RejectedStillageDetails")]
        [HttpPost]
        public StillageStickerHold RejectedStillageDetail(StickerReq SR)
        {
            StillageStickerHold SM = new StillageStickerHold();
            try
            {
                if (SR.StickerNo == "")
                {
                    SM.Status = "Failure";
                    SM.Message = "Enter Sticker ID";
                }
                if (SR.UserId == 0)
                {
                    SM.Status = "Failure";
                    SM.Message = "Invalid UserId";
                }
                query = "Sp_RejectStillageWebApi";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.Connection.Open();
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.Parameters.AddWithValue("@QueryType", "GetRejectedStillageDetails");
                dbcommand.Parameters.AddWithValue("@StickerId", SR.StickerNo);
                dbcommand.Parameters.AddWithValue("@UserId", SR.UserId);

                dbcommand.CommandTimeout = 0;
                SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                DataSet dt = new DataSet();
                da.Fill(dt);
                if (dt != null && dt.Tables[0].Rows.Count > 0)
                {
                    if (dt.Tables[0].Rows[0]["value"].ToString() == "1")
                    {
                        SM.Status = "Failure";
                        SM.Message = "This is rejected stillage..";
                        return SM;

                    }
                    else
                    { 
                    StillageSticker SS = new StillageSticker();
                    SM.StickerID = dt.Tables[0].Rows[0]["StickerID"].ToString();
                    SM.StandardQty = Convert.ToDecimal(dt.Tables[0].Rows[0]["StandardQty"]);
                    SM.ItemId = dt.Tables[0].Rows[0]["ItemId"].ToString();
                    SM.WorkOrderNo = dt.Tables[0].Rows[0]["WorkOrderNo"].ToString();
                    SM.Description = dt.Tables[0].Rows[0]["Description"].ToString();
                    SM.ItemStdQty = Convert.ToDecimal(dt.Tables[0].Rows[0]["ItemStdQty"]);
                    SM.isHold = Convert.ToByte(dt.Tables[0].Rows[0]["isHold"]);
                    SM.IsCounted = Convert.ToByte(dt.Tables[0].Rows[0]["IsCounted"]);
                    SM.WareHouseID = Convert.ToString(dt.Tables[0].Rows[0]["WareHouseID"]);
                    SM.IsAssignTransfer = Convert.ToByte(dt.Tables[0].Rows[0]["IsAssignTransfer"]);

                        if (DBNull.Value.Equals(dt.Tables[1].Rows[0]["Prodstatus"]))
                        {
                            SM.Prodstatus = 0;
                            SM.WoStatus = "N/A";
                        }
                        else
                        { 
                            SM.WoStatus = Convert.ToString(dt.Tables[1].Rows[0]["WorkorderStatus"]);
                        SM.Prodstatus = Convert.ToInt16(dt.Tables[1].Rows[0]["Prodstatus"]);
                    }

                  
                    SM.UOM = Convert.ToDecimal(dt.Tables[0].Rows[0]["UOM"]);

                    SM.Status = "Success";
                    SM.Message = "Data retrived successfully";
                    }
                }
                else
                {
                    SM.Status = "Failure";
                    SM.Message = "This stillage does not exist";
                }
            }
            catch (Exception Ex)
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

        [Route("api/Nepro/UpdatedRejectedStillage")]
        [HttpPost]
        public StickerMaster RejectedStillageDetail(RejectedStillageReq RSR)
        {
            StickerMaster SM = new StickerMaster();
            try
            {
                if (RSR.StickerNo == "")
                {
                    SM.Status = "Failure";
                    SM.Message = "Enter Sticker ID";
                }
                if (RSR.UserId == 0)
                {
                    SM.Status = "Failure";
                    SM.Message = "Invalid UserId";
                }

                int flag = 0;

               Decimal StillageQtyPcs = 0;
                query = "Sp_RejectStillageWebApi";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.CommandTimeout = 0;
                dbcommand.Parameters.AddWithValue("@QueryType", "FetchDataRejectST");
                dbcommand.Parameters.AddWithValue("@StillageID", RSR.StickerNo);
                dbcommand.Parameters.AddWithValue("@UserId", RSR.UserId);
                dbcommand.Parameters.AddWithValue("@ActivityID", "Reject");
                dbcommand.Parameters.AddWithValue("@Reason", RSR.Reason);
                SqlDataAdapter daGetData = new SqlDataAdapter(dbcommand);
                DataSet dsGetData = new DataSet();
                daGetData.Fill(dsGetData);
                SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                var Hold = "";
                if (dsGetData.Tables[0].Rows[0]["WorkOrderNo"].ToString() == "NA")
                {
                    SM.Status = "Failure";
                    SM.Message = "This operation doesn't process this stillage sticker";
                    return SM;
                }
                else
                {
                    Hold = "Qc Release";
                }
                if (dsGetData.Tables[0].Rows[0]["isHold"].ToString() == "True")
                {
                    Hold = "Qc Hold";
                }
                decimal StillageQty = 0;


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
                    if(Convert.ToString(dsGetData.Tables[1].Rows[0]["WOStatus"])=="7")
                        {
                        if (RSR.IsKg == 1)
                        {
                            StillageQty = Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["StillageQty"]) - Math.Round(RSR.Quantity / Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["Factor"]));
                        }
                        else
                        {
                            StillageQty = Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["StillageQty"]) - RSR.Quantity;
                        }
                        if (StillageQty == 0)
                        {
                            query = "[Sp_MasterDataWebApi]";
                            dbcommand = new SqlCommand(query, conn);
                            dbcommand.CommandType = CommandType.StoredProcedure;
                            dbcommand.CommandTimeout = 0;
                            dbcommand.Parameters.AddWithValue("@QueryType", "GetActivityMaster");
                            dbcommand.Parameters.AddWithValue("@ActivityID", "Reject & Discarded");
                            SqlDataAdapter daGetData1 = new SqlDataAdapter(dbcommand);
                            DataSet dsGetData1 = new DataSet();
                            daGetData.Fill(dsGetData1);
                            string JournalID = "";
                            if (RSR.IsKg == 1)
                            {
                                obj.UpdateQty(Cct, Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), false, false, Convert.ToString(RSR.Shift), RSR.StickerNo, "HHD", StillageQty, Convert.ToDecimal(-1 * RSR.Quantity));
                                //JournalID = obj.QCRejectHeader(Cct, Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), RSR.StickerNo, "Process");
                                //obj.QCRejectLines(Cct, JournalID, "Startup", Convert.ToDecimal(RSR.Quantity), Convert.ToString(RSR.Reason), Convert.ToString(RSR.Shift), Convert.ToDateTime(DateTime.Now.ToString()));
                                ////obj.QCRejectFGLine(Cct, JournalID);
                                //obj.PostQCReject(Cct, JournalID);
                            }

                            else
                            {
                                obj.UpdateQty(Cct, Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), false, false, Convert.ToString(RSR.Shift), RSR.StickerNo, "HHD", StillageQty, Convert.ToDecimal(-1 * RSR.Quantity));
                                //JournalID = obj.QCRejectHeader(Cct, Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), RSR.StickerNo, "Process");
                                //obj.QCRejectLines(Cct, JournalID, "Process", Convert.ToDecimal(RSR.Quantity), Convert.ToString(RSR.Reason), Convert.ToString(RSR.Shift), Convert.ToDateTime(DateTime.Now.ToString()));
                                ////obj.QCRejectFGLine(Cct, JournalID);
                                //obj.PostQCReject(Cct, JournalID);
                            }
                            //obj.UpdateStillageQty(Cct, RSR.StickerNo, Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), StillageQty);

                            string value = obj.InsertHistoryHeaderData(Cct, RSR.StickerNo, Convert.ToString(dsGetData.Tables[0].Rows[0]["SiteID"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ItemId"]), Convert.ToDecimal(dsGetData.Tables[0].Rows[0]["WorkOrderQty"]));
                            obj.InsertHistoryDetailData(Cct, RSR.StickerNo, "", Convert.ToString(dsGetData1.Tables[1].Rows[0]["ActivityName"]), Convert.ToString(dsGetData1.Tables[1].Rows[0]["ActivityDesc"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), "", "", "", "", Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonName"]), Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonDes"]), "Yes", 0, "", 0, 0, StillageQty, Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]), Hold, RSR.Quantity, "", "", "", Convert.ToString(dsGetData.Tables[0].Rows[0]["WareHouseID"]), 0);

                        }
                        else
                        {
                            string JournalID = "";
                            if (RSR.IsKg == 1)
                            {
                                obj.UpdateQty(Cct, Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), false, false, Convert.ToString(RSR.Shift), RSR.StickerNo, "HHD", StillageQty,Convert.ToDecimal(-1* RSR.Quantity));
                                //JournalID = obj.QCRejectHeader(Cct, Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), RSR.StickerNo, "Process");
                                //obj.QCRejectLines(Cct, JournalID, "Startup", Convert.ToDecimal(RSR.Quantity), Convert.ToString(RSR.Reason), Convert.ToString(RSR.Shift), Convert.ToDateTime(DateTime.Now.ToString()));
                                ////obj.QCRejectFGLine(Cct, JournalID);
                                //obj.PostQCReject(Cct, JournalID);
                            }

                            else
                            {
                                obj.UpdateQty(Cct, Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), false, false, Convert.ToString(RSR.Shift), RSR.StickerNo, "HHD", StillageQty, Convert.ToDecimal(-1 * RSR.Quantity));
                                //JournalID = obj.QCRejectHeader(Cct, Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), RSR.StickerNo, "Process");
                                //obj.QCRejectLines(Cct, JournalID, "Process", Convert.ToDecimal(RSR.Quantity), Convert.ToString(RSR.Reason), Convert.ToString(RSR.Shift), Convert.ToDateTime(DateTime.Now.ToString()));
                                ////obj.QCRejectFGLine(Cct, JournalID);
                                //obj.PostQCReject(Cct, JournalID);
                            }
                            string value = obj.InsertHistoryHeaderData(Cct, RSR.StickerNo, Convert.ToString(dsGetData.Tables[0].Rows[0]["SiteID"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ItemId"]), Convert.ToDecimal(dsGetData.Tables[0].Rows[0]["WorkOrderQty"]));
                            obj.InsertHistoryDetailData(Cct, RSR.StickerNo, "", Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityName"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityDesc"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), "", "", "", "", Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonName"]), Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonDes"]), "Yes", 0, "", 0, 0, StillageQty, Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]), Hold, RSR.Quantity, "", "", "", Convert.ToString(dsGetData.Tables[1].Rows[0]["WareHouseID"]), 0);

                        }

                        query = "Sp_RejectStillageWebApi";
                        dbcommand = new SqlCommand(query, conn);
                        dbcommand.CommandType = CommandType.StoredProcedure;
                        dbcommand.CommandTimeout = 0;
                        dbcommand.Parameters.AddWithValue("@QueryType", "SingleRejectStillage");
                        dbcommand.Parameters.AddWithValue("@StickerId", RSR.StickerNo);
                        dbcommand.Parameters.AddWithValue("@RejectedQty", StillageQty);
                        dbcommand.Parameters.AddWithValue("@Reason", RSR.Reason);
                        dbcommand.Parameters.AddWithValue("@Shift", RSR.Shift);
                        dbcommand.Parameters.AddWithValue("@UserId", RSR.UserId);
                        //dbcommand.Parameters.AddWithValue("@ActivityID", RSR.ActivityID);
                        SqlDataAdapter daGetData0 = new SqlDataAdapter(dbcommand);
                        DataSet dsGetData0 = new DataSet();
                        daGetData0.Fill(dsGetData0);
                        da = new SqlDataAdapter(dbcommand);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        if (dt.Rows[0]["value"].ToString() == "1")
                        {
                            SM.Status = "Success";
                            SM.Message = "Stillage Reject successfully";
                        }
                        else
                        {
                            SM.Status = "Failure";
                            SM.Message = "Stillage Not Reject successful";
                        }

                    }
                    else
                    {
                        if (RSR.IsKg == 1)
                        {
                            StillageQty = Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["StillageQty"]) - Math.Round(RSR.Quantity / Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["Factor"]));
                            StillageQtyPcs = Math.Round(RSR.Quantity / Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["Factor"]));
                        }
                        else
                        {
                            StillageQty = Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["StillageQty"]) - RSR.Quantity;
                        }
                        if(Convert.ToBoolean(dsGetData.Tables[1].Rows[0]["IsCounted"])==false)
                        {
                            flag = 1;
                            if (StillageQty == 0)
                            {
                                query = "[Sp_MasterDataWebApi]";
                                dbcommand = new SqlCommand(query, conn);
                                dbcommand.CommandType = CommandType.StoredProcedure;
                                dbcommand.CommandTimeout = 0;
                                dbcommand.Parameters.AddWithValue("@QueryType", "GetActivityMaster");
                                dbcommand.Parameters.AddWithValue("@ActivityID", "Reject & Discarded");
                                SqlDataAdapter daGetData1 = new SqlDataAdapter(dbcommand);
                                DataSet dsGetData1 = new DataSet();
                                daGetData.Fill(dsGetData1);
                                string JournalID = "";
                                if (RSR.IsKg == 1)
                                {
                                    //obj.ProcessRejectionRAF(Cct, Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), false, false, Convert.ToString(RSR.Shift), RSR.StickerNo, "HHD", false, false, StillageQtyPcs);
                                    JournalID = obj.QCRejectHeader(Cct, Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), RSR.StickerNo, "Process");
                                    obj.QCRejectLines(Cct, JournalID, "Process", StillageQtyPcs, Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonName"]), Convert.ToString(RSR.Shift), Convert.ToDateTime(DateTime.Now.ToString()), RSR.Quantity);
                                    obj.QCRejectFGLine(Cct, JournalID);
                                    obj.PostQCReject(Cct, JournalID);

                                    string value = obj.InsertHistoryHeaderData(Cct, RSR.StickerNo, Convert.ToString(dsGetData.Tables[0].Rows[0]["SiteID"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ItemId"]), Convert.ToDecimal(dsGetData.Tables[0].Rows[0]["WorkOrderQty"]));
                                    obj.InsertHistoryDetailData(Cct, RSR.StickerNo, "", Convert.ToString(dsGetData1.Tables[1].Rows[0]["ActivityName"]), Convert.ToString(dsGetData1.Tables[1].Rows[0]["ActivityDesc"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), "", "", "", "", Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonName"]), Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonDes"]), "No", 0, "", 0, 0, Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["StillageQty"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]), Hold, StillageQtyPcs, "", "", "", Convert.ToString(dsGetData.Tables[0].Rows[0]["WareHouseID"]), 0);

                                }

                                else
                                {
                                    //obj.ProcessRejectionRAF(Cct, Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), false, false, Convert.ToString(RSR.Shift), RSR.StickerNo, "HHD", false, false, RSR.Quantity);
                                    JournalID = obj.QCRejectHeader(Cct, Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), RSR.StickerNo, "Process");
                                    obj.QCRejectLines(Cct, JournalID, "Process", Convert.ToDecimal(RSR.Quantity), Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonName"]), Convert.ToString(RSR.Shift), Convert.ToDateTime(DateTime.Now.ToString()), RSR.Quantity);
                                    obj.QCRejectFGLine(Cct, JournalID);
                                    obj.PostQCReject(Cct, JournalID);

                                    string value = obj.InsertHistoryHeaderData(Cct, RSR.StickerNo, Convert.ToString(dsGetData.Tables[0].Rows[0]["SiteID"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ItemId"]), Convert.ToDecimal(dsGetData.Tables[0].Rows[0]["WorkOrderQty"]));
                                    obj.InsertHistoryDetailData(Cct, RSR.StickerNo, "", Convert.ToString(dsGetData1.Tables[1].Rows[0]["ActivityName"]), Convert.ToString(dsGetData1.Tables[1].Rows[0]["ActivityDesc"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), "", "", "", "", Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonName"]), Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonDes"]), "No", 0, "", 0, 0, Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["StillageQty"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]), Hold, RSR.Quantity, "", "", "", Convert.ToString(dsGetData.Tables[0].Rows[0]["WareHouseID"]), 0);


                                }
                                //obj.UpdateStillageQty(Cct, RSR.StickerNo, Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), StillageQty);


                            }
                            else
                            {
                                string JournalID = "";
                                if (RSR.IsKg == 1)
                                {
                                    //obj.ProcessRejectionRAF(Cct, Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), false, false, Convert.ToString(RSR.Shift), RSR.StickerNo, "HHD", false, false, StillageQtyPcs);
                                    JournalID = obj.QCRejectHeader(Cct, Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), RSR.StickerNo, "Process");
                                    obj.QCRejectLines(Cct, JournalID, "Process", StillageQtyPcs, Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonName"]), Convert.ToString(RSR.Shift), Convert.ToDateTime(DateTime.Now.ToString()), RSR.Quantity);
                                    obj.QCRejectFGLine(Cct, JournalID);
                                    obj.PostQCReject(Cct, JournalID);

                                    string value = obj.InsertHistoryHeaderData(Cct, RSR.StickerNo, Convert.ToString(dsGetData.Tables[0].Rows[0]["SiteID"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ItemId"]), Convert.ToDecimal(dsGetData.Tables[0].Rows[0]["WorkOrderQty"]));
                                    obj.InsertHistoryDetailData(Cct, RSR.StickerNo, "", Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityName"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityDesc"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), "", "", "", "", Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonName"]), Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonDes"]), "No", 0, "", 0, 0, Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["StillageQty"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]), Hold, StillageQtyPcs, "", "", "", Convert.ToString(dsGetData.Tables[1].Rows[0]["WareHouseID"]), 0);


                                }

                                else
                                {
                                    //obj.ProcessRejectionRAF(Cct, Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), false, false, Convert.ToString(RSR.Shift), RSR.StickerNo, "HHD", false, false, RSR.Quantity);
                                    JournalID = obj.QCRejectHeader(Cct, Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), RSR.StickerNo, "Process");
                                    obj.QCRejectLines(Cct, JournalID, "Process", Convert.ToDecimal(RSR.Quantity), Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonName"]), Convert.ToString(RSR.Shift), Convert.ToDateTime(DateTime.Now.ToString()), RSR.Quantity);
                                    obj.QCRejectFGLine(Cct, JournalID);
                                    obj.PostQCReject(Cct, JournalID);

                                    string value = obj.InsertHistoryHeaderData(Cct, RSR.StickerNo, Convert.ToString(dsGetData.Tables[0].Rows[0]["SiteID"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ItemId"]), Convert.ToDecimal(dsGetData.Tables[0].Rows[0]["WorkOrderQty"]));
                                    obj.InsertHistoryDetailData(Cct, RSR.StickerNo, "", Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityName"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityDesc"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), "", "", "", "", Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonName"]), Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonDes"]), "No", 0, "", 0, 0, Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["StillageQty"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]), Hold, RSR.Quantity, "", "", "", Convert.ToString(dsGetData.Tables[1].Rows[0]["WareHouseID"]), 0);

                                }

                            }

                        }
                        else
                        {

                            if (StillageQty == 0)
                            {
                                query = "[Sp_MasterDataWebApi]";
                                dbcommand = new SqlCommand(query, conn);
                                dbcommand.CommandType = CommandType.StoredProcedure;
                                dbcommand.CommandTimeout = 0;
                                dbcommand.Parameters.AddWithValue("@QueryType", "GetActivityMaster");
                                dbcommand.Parameters.AddWithValue("@ActivityID", "Reject & Discarded");
                                SqlDataAdapter daGetData1 = new SqlDataAdapter(dbcommand);
                                DataSet dsGetData1 = new DataSet();
                                daGetData.Fill(dsGetData1);
                                string JournalID = "";
                                if (RSR.IsKg == 1)
                                {
                                    //obj.ProcessRejectionRAF(Cct, Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), false, false, Convert.ToString(RSR.Shift), RSR.StickerNo, "HHD", false, false, StillageQtyPcs);
                                    JournalID = obj.QCRejectHeader(Cct, Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), RSR.StickerNo, "Process");
                                    obj.QCRejectLines(Cct, JournalID, "Process", StillageQtyPcs, Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonName"]), Convert.ToString(RSR.Shift), Convert.ToDateTime(DateTime.Now.ToString()), RSR.Quantity);
                                    obj.QCRejectFGLine(Cct, JournalID);
                                    obj.PostQCReject(Cct, JournalID);

                                    string value = obj.InsertHistoryHeaderData(Cct, RSR.StickerNo, Convert.ToString(dsGetData.Tables[0].Rows[0]["SiteID"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ItemId"]), Convert.ToDecimal(dsGetData.Tables[0].Rows[0]["WorkOrderQty"]));
                                    obj.InsertHistoryDetailData(Cct, RSR.StickerNo, "", Convert.ToString(dsGetData1.Tables[1].Rows[0]["ActivityName"]), Convert.ToString(dsGetData1.Tables[1].Rows[0]["ActivityDesc"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), "", "", "", "", Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonName"]), Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonDes"]), "Yes", 0, "", 0, 0, StillageQty, Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]), Hold, StillageQtyPcs, "", "", "", Convert.ToString(dsGetData.Tables[0].Rows[0]["WareHouseID"]), 0);

                                }

                                else
                                {
                                    //obj.ProcessRejectionRAF(Cct, Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), false, false, Convert.ToString(RSR.Shift), RSR.StickerNo, "HHD", false, false, RSR.Quantity);
                                    JournalID = obj.QCRejectHeader(Cct, Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), RSR.StickerNo, "Process");
                                    obj.QCRejectLines(Cct, JournalID, "Process", Convert.ToDecimal(RSR.Quantity), Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonName"]), Convert.ToString(RSR.Shift), Convert.ToDateTime(DateTime.Now.ToString()), RSR.Quantity);
                                    obj.QCRejectFGLine(Cct, JournalID);
                                    obj.PostQCReject(Cct, JournalID);

                                    string value = obj.InsertHistoryHeaderData(Cct, RSR.StickerNo, Convert.ToString(dsGetData.Tables[0].Rows[0]["SiteID"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ItemId"]), Convert.ToDecimal(dsGetData.Tables[0].Rows[0]["WorkOrderQty"]));
                                    obj.InsertHistoryDetailData(Cct, RSR.StickerNo, "", Convert.ToString(dsGetData1.Tables[1].Rows[0]["ActivityName"]), Convert.ToString(dsGetData1.Tables[1].Rows[0]["ActivityDesc"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), "", "", "", "", Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonName"]), Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonDes"]), "Yes", 0, "", 0, 0, StillageQty, Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]), Hold, RSR.Quantity, "", "", "", Convert.ToString(dsGetData.Tables[0].Rows[0]["WareHouseID"]), 0);


                                }
                                //obj.UpdateStillageQty(Cct, RSR.StickerNo, Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), StillageQty);


                            }
                            else
                            {
                                string JournalID = "";
                                if (RSR.IsKg == 1)
                                {
                                    //obj.ProcessRejectionRAF(Cct, Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), false, false, Convert.ToString(RSR.Shift), RSR.StickerNo, "HHD", false, false, StillageQtyPcs);
                                    JournalID = obj.QCRejectHeader(Cct, Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), RSR.StickerNo, "Process");
                                    obj.QCRejectLines(Cct, JournalID, "Process", StillageQtyPcs, Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonName"]), Convert.ToString(RSR.Shift), Convert.ToDateTime(DateTime.Now.ToString()), RSR.Quantity);
                                    obj.QCRejectFGLine(Cct, JournalID);
                                    obj.PostQCReject(Cct, JournalID);

                                    string value = obj.InsertHistoryHeaderData(Cct, RSR.StickerNo, Convert.ToString(dsGetData.Tables[0].Rows[0]["SiteID"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ItemId"]), Convert.ToDecimal(dsGetData.Tables[0].Rows[0]["WorkOrderQty"]));
                                    obj.InsertHistoryDetailData(Cct, RSR.StickerNo, "", Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityName"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityDesc"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), "", "", "", "", Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonName"]), Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonDes"]), "Yes", 0, "", 0, 0, StillageQty, Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]), Hold, StillageQtyPcs, "", "", "", Convert.ToString(dsGetData.Tables[1].Rows[0]["WareHouseID"]), 0);


                                }

                                else
                                {
                                   // obj.ProcessRejectionRAF(Cct, Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), false, false, Convert.ToString(RSR.Shift), RSR.StickerNo, "HHD", false, false, RSR.Quantity);
                                    JournalID = obj.QCRejectHeader(Cct, Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), RSR.StickerNo, "Process");
                                    obj.QCRejectLines(Cct, JournalID, "Process", Convert.ToDecimal(RSR.Quantity), Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonName"]), Convert.ToString(RSR.Shift), Convert.ToDateTime(DateTime.Now.ToString()), RSR.Quantity);
                                    obj.QCRejectFGLine(Cct, JournalID);
                                    obj.PostQCReject(Cct, JournalID);

                                    string value = obj.InsertHistoryHeaderData(Cct, RSR.StickerNo, Convert.ToString(dsGetData.Tables[0].Rows[0]["SiteID"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ItemId"]), Convert.ToDecimal(dsGetData.Tables[0].Rows[0]["WorkOrderQty"]));
                                    obj.InsertHistoryDetailData(Cct, RSR.StickerNo, "", Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityName"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityDesc"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), "", "", "", "", Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonName"]), Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonDes"]), "Yes", 0, "", 0, 0, StillageQty, Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]), Hold, RSR.Quantity, "", "", "", Convert.ToString(dsGetData.Tables[1].Rows[0]["WareHouseID"]), 0);

                                }

                            }
                        }
                        

                        query = "Sp_RejectStillageWebApi";
                        dbcommand = new SqlCommand(query, conn);
                        dbcommand.CommandType = CommandType.StoredProcedure;
                        dbcommand.CommandTimeout = 0;
                        dbcommand.Parameters.AddWithValue("@QueryType", "SingleRejectStillage");
                        dbcommand.Parameters.AddWithValue("@StickerId", RSR.StickerNo);
                        dbcommand.Parameters.AddWithValue("@RejectedQty", StillageQty);
                        dbcommand.Parameters.AddWithValue("@Reason", RSR.Reason);
                        dbcommand.Parameters.AddWithValue("@Shift", RSR.Shift);
                        dbcommand.Parameters.AddWithValue("@UserId", RSR.UserId);
                        dbcommand.Parameters.AddWithValue("@flag", flag);
                        //dbcommand.Parameters.AddWithValue("@ActivityID", RSR.ActivityID);
                        SqlDataAdapter daGetData0 = new SqlDataAdapter(dbcommand);
                        DataSet dsGetData0 = new DataSet();
                        daGetData0.Fill(dsGetData0);
                        da = new SqlDataAdapter(dbcommand);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        if (dt.Rows[0]["value"].ToString() == "1")
                        {
                            SM.Status = "Success";
                            SM.Message = "Stillage Reject successfully";
                        }
                        else
                        {
                            SM.Status = "Failure";
                            SM.Message = "Stillage Not Reject successful";
                        }

                    }



                }

                 
                else
                {
                    SM.Status = "Failure";
                    SM.Message = "This Stillage Does Not Exist";
                    return SM;
                }
               
            }
            catch (Exception Ex)
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



        [Route("api/Nepro/CompleteRejectedStillage")]
        [HttpPost]
        public CompleteRejectedStillageRes RejectedStillageDetail(CompleteRejectedStillageReq CRS)
        {
            CompleteRejectedStillageRes SM = new CompleteRejectedStillageRes();

            string json = JsonConvert.SerializeObject(CRS.RejectionList);
            try
            {

                query = "Sp_RejectStillageWebApi";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.CommandTimeout = 0;
                dbcommand.Parameters.AddWithValue("@QueryType", "CompleteRejectedStillage");
                dbcommand.Parameters.AddWithValue("@IsKg", CRS.IsKg);
                dbcommand.Parameters.AddWithValue("@dtplan", json);
                dbcommand.CommandTimeout = 0;
                SqlDataAdapter da = new SqlDataAdapter();
                da = new SqlDataAdapter(dbcommand);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows[0]["value"].ToString() == "1")
                {
                    SM.Status = "Success";
                    SM.Message = "Process Rejection successfully in Kg";
                }
                else
                {
                    SM.Status = "Success";
                    SM.Message = "Process Rejection successfully in Pcs";
                }
            }

            catch (Exception Ex)
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





        [Route("api/Nepro/UpdatedHoldUnHoldStillage")]
        [HttpPost]
        public StickerMaster HoldUnHoldStillageDetail(RejectedStillageReq RSR)
        {
            string StillageStatus = "";
            StickerMaster SM = new StickerMaster();
            try
            {
                if (RSR.StickerNo == "")
                {
                    SM.Status = "Failure";
                    SM.Message = "Enter Sticker ID";
                }
                if (RSR.UserId == 0)
                {
                    SM.Status = "Failure";
                    SM.Message = "Invalid UserId";
                }
                if (RSR.IsHold == 1)
                {
                    StillageStatus = "Qc Hold";
                }
                else
                {
                    StillageStatus = "Qc Release";
                }

                query = "Sp_PickAndLoad";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.CommandTimeout = 0;
                dbcommand.Parameters.AddWithValue("@QueryType", "FetchDataHold");
                dbcommand.Parameters.AddWithValue("@StillageID", RSR.StickerNo);
                dbcommand.Parameters.AddWithValue("@UserId", RSR.UserId);
                //dbcommand.Parameters.AddWithValue("@ActivityID", RSR.ActivityID);
                SqlDataAdapter daGetData = new SqlDataAdapter(dbcommand);
                DataSet dsGetData = new DataSet();
                daGetData.Fill(dsGetData);
                SqlDataAdapter da = new SqlDataAdapter(dbcommand);

                if (Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]) == "NA")
                {
                    SM.Status = "Failure";
                    SM.Message = "This operation doesn't process this stillage sticker ";
                    return SM;
                }

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
                    string value = obj.InsertHistoryHeaderData(Cct, RSR.StickerNo, Convert.ToString(dsGetData.Tables[0].Rows[0]["SiteID"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ItemId"]), Convert.ToDecimal(dsGetData.Tables[0].Rows[0]["LoadQuantity"]));
                    obj.InsertHistoryDetailData(Cct, RSR.StickerNo, "", Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityName"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityDesc"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), "", "", "", "", "", "", "No", 0, "", 0, 0, Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["StillageQty"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]), StillageStatus, 0, "","","", Convert.ToString(dsGetData.Tables[1].Rows[0]["WareHouseID"]),0);
                }
                else
                {
                    SM.Status = "Failure";
                    SM.Message = "This stillage Does Not Exist";
                    return SM;
                }


                query = "Sp_RejectStillageWebApi";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.Connection.Open();
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.Parameters.AddWithValue("@QueryType", "UpdateHoldUnHoldStillage");
                dbcommand.Parameters.AddWithValue("@StickerId", RSR.StickerNo);
                dbcommand.Parameters.AddWithValue("@UserId", RSR.UserId);


                dbcommand.CommandTimeout = 0;
                da = new SqlDataAdapter(dbcommand);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows[0]["value"].ToString() == "1")
                {
                    SM.Status = "Success";
                    SM.Message = "Stillage holded successfully";
                }
                else if (dt.Rows[0]["value"].ToString() == "2")
                {
                    SM.Status = "Success";
                    SM.Message = "QC Released Successfully";
                }
                else
                {
                    SM.Status = "Failure";
                    SM.Message = "Operation failure";
                }
            }
            catch (Exception Ex)
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




        [Route("api/Nepro/ProcessRejectedWorkorder")]
        [HttpPost]
        public StickerMaster ProcessRejectedWorkorder(WorkorderRejection WR)
        {
            StickerMaster SM = new StickerMaster();
            try
            {
                if (WR.WorkorderNo == "")
                {
                    SM.Status = "Failure";
                    SM.Message = "Enter Workorder No.";
                }
                if (WR.UserId == 0)
                {
                    SM.Status = "Failure";
                    SM.Message = "Invalid UserId";
                }
                string json = JsonConvert.SerializeObject(WR.WorkorderRejectDetail);
                query = "Sp_StartUpRejection";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.CommandTimeout = 0;
                dbcommand.Parameters.AddWithValue("@QueryType", "GetDataForWorkorderInPCs");
                dbcommand.Parameters.AddWithValue("@WorkOrderNo", WR.WorkorderNo);
                dbcommand.Parameters.AddWithValue("@UserId", WR.UserId);
                dbcommand.Parameters.AddWithValue("@dtplan", json);


                SqlDataAdapter daGetData = new SqlDataAdapter(dbcommand);
                DataSet dsGetData = new DataSet();
                daGetData.Fill(dsGetData);
                SqlDataAdapter da = new SqlDataAdapter(dbcommand);

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

                string JournalID = "";
                string ItemID = "";
                if (WR.WOStatus == "End")
                {
                    foreach (var row in WR.WorkorderRejectDetail)
                    {
                        ItemID = Convert.ToString(row.ItemId);
                        DateTime DATE = DateTime.ParseExact(Convert.ToString(row.date), "dd/MM/yyyy", null);
                        obj.UpdateQty(Cct, Convert.ToString(WR.WorkorderNo), false, false, Convert.ToString(row.Shift), "", "HHD", Convert.ToDecimal(row.Qty), Convert.ToDecimal(-1 * row.Qty));

                    }
                    }
                else
                {
                    JournalID = obj.QCRejectHeader(Cct, Convert.ToString(WR.WorkorderNo), "", "Process");

                    foreach (var row in WR.WorkorderRejectDetail)
                    {
                        ItemID = Convert.ToString(row.ItemId);
                        DateTime DATE = DateTime.ParseExact(Convert.ToString(row.date), "dd/MM/yyyy", null);
                        obj.QCRejectLines(Cct, JournalID, "Process", Convert.ToDecimal(row.Qty), row.ReasonName, row.Shift, DATE, Convert.ToDecimal(row.Qty));
                       
                    }
                    obj.QCRejectFGLine(Cct, JournalID);
                    obj.PostQCReject(Cct, JournalID);
                }

                        query = "Sp_StartUpRejection";
                        dbcommand = new SqlCommand(query, conn);
                        dbcommand.CommandType = CommandType.StoredProcedure;
                        dbcommand.CommandTimeout = 0;
                        dbcommand.Parameters.AddWithValue("@QueryType", "SaveWorkorderProcessRejectionData");
                        dbcommand.Parameters.AddWithValue("@dtplan",json);
                        dbcommand.Parameters.AddWithValue("@WorkOrderNo", WR.WorkorderNo);
                        dbcommand.Parameters.AddWithValue("@ItemID", ItemID);
                        dbcommand.Parameters.AddWithValue("@Type", 1);
                        dbcommand.Parameters.AddWithValue("@UserId", WR.UserId);
                        SqlDataAdapter daGetData0 = new SqlDataAdapter(dbcommand);
                        DataSet dsGetData0 = new DataSet();
                        daGetData0.Fill(dsGetData0);
                        //da = new SqlDataAdapter(dbcommand);
                        //DataTable dt = new DataTable();
                        //da.Fill(dt);
                        if (dsGetData0.Tables[0].Rows[0]["value"].ToString() == "1")
                        {
                            SM.Status = "Success";
                            SM.Message = "Workorder Reject successfully";
                        }
                        else
                        {
                            SM.Status = "Failure";
                            SM.Message = "Stillage Not Reject successful";
                        }


            }
            catch (Exception Ex)
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





    }
}




public class RejectedStillageReq
{
    public string StickerNo { get; set; }
    public Int64 UserId { get; set; }
    public decimal Quantity { get; set; }
    public Int64 Reason { get; set; }
    public string WorkOrderNo { get; set; }
    public int IsHold { get; set; }
    public int IsKg { get; set; }
    public int ActivityID { get; set; }
    public string Shift { get; set; }
}
public class StillageStickerHold
{
    public string StickerID { get; set; }
    public decimal StandardQty { get; set; }
    public string ItemId { get; set; }
    public string Description { get; set; }
    public decimal ItemStdQty { get; set; }
    public string WorkOrderNo { get; set; }
    public byte isHold { get; set; }
    public byte IsCounted { get; set; }
    public string WareHouseID { get; set; }
    public string WoStatus { get; set; }
    public int Prodstatus { get; set; }
    public byte IsAssignTransfer { get; set; }
    public Decimal UOM { get; set; }
    public string Status { get; set; }
    public string Message { get; set; }
}


public class CompleteRejectedStillageReq
{
    public int IsKg { get; set; }
    public List<RejectionList> RejectionList { get; set; }
}

public class RejectionList
{
    public int IsKg { get; set; }
    public decimal Quantity { get; set; }
    public Int64 Reason { get; set; }
    public string ReasonName { get; set; }
    public string Shift { get; set; }
    public string StickerNo { get; set; }
    public Int64 UserId { get; set; }
    public string WorkOrderNo { get; set; }

}
public class CompleteRejectedStillageRes
{
    public string Status { get; set; }
    public string Message { get; set; }


}

public class WorkorderRejection
{
    public string WorkorderNo { get; set; }
    public Int64 UserId { get; set; }
    public string WOStatus { get; set; }
    public List<WorkorderRejectedDetails> WorkorderRejectDetail { get; set; }
}


public class WorkorderRejectedDetails
{
    public string Shift { get; set; }
    public Int32 Qty { get; set; }
    public Int64 Reason { get; set; }
    public string ReasonName { get; set; }
    public string ItemId { get; set; }
    public string date { get; set; }

}
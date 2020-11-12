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
    public class MergeStillageController : ApiController
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Conn"].ConnectionString);
        string query = ""; SqlCommand dbcommand;


        [Route("api/Nepro/ScanMergeStillage")]
        [HttpPost]
        public ScanMergeResponse ScanMergeStillage(StickerReq SR)
        {
            ScanMergeResponse SS = new ScanMergeResponse();
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
                query = "Sp_MergeStillage";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.Connection.Open();
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.Parameters.AddWithValue("@QueryType", "ScanMergeStillage");
                dbcommand.Parameters.AddWithValue("@StickerId", SR.StickerNo);
                dbcommand.Parameters.AddWithValue("@UserId", SR.UserId);
                dbcommand.CommandTimeout = 0;
                SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows[0]["value"].ToString() != "2")
                {
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        SS.StickerID = dt.Rows[0]["StickerID"].ToString();
                        SS.StandardQty = Convert.ToDecimal(dt.Rows[0]["StandardQty"]);
                        SS.ItemId = dt.Rows[0]["ItemId"].ToString();
                        SS.Description = dt.Rows[0]["Description"].ToString();
                        SS.ItemStdQty = Convert.ToDecimal(dt.Rows[0]["ItemStdQty"]);
                        SS.WareHouseName = dt.Rows[0]["WareHouseName"].ToString();
                        SS.IsCounted =Convert.ToByte(dt.Rows[0]["IsCounted"]);
                        SS.WareHouseID = dt.Rows[0]["WareHouseID"].ToString();
                        SS.SiteID = dt.Rows[0]["SiteID"].ToString();
                        SS.IsAssignTransfer = Convert.ToByte(dt.Rows[0]["IsAssignTransfer"]);
                        SS.Status = "Success";
                        SS.Message = "Data retrived successfully";
                    }
                    else
                    {
                        SS.Status = "Failure";
                        SS.Message = "Stillage Id Invalid";
                        return SS;
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


        [Route("api/Nepro/UpdateMergeStillage")]
        [HttpPost]
        public ReceivingResponse UpdateMergeStillage(UpdateMergeResponse UMR)
        {
            ReceivingResponse SM = new ReceivingResponse();
            try
            {
                if (UMR.StickerNo == "")
                {
                    SM.Status = "Failure";
                    SM.Message = "Enter Sticker ID";
                }
                if (UMR.UserId == 0)
                {
                    SM.Status = "Failure";
                    SM.Message = "Invalid UserId";
                }





                query = "Sp_AxWebserviceIntegration";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.Connection.Open();
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.CommandTimeout = 0;
                SqlDataAdapter da = new SqlDataAdapter(dbcommand);
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




                query = "Sp_MergeStillage";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.CommandTimeout = 0;
                dbcommand.Parameters.AddWithValue("@QueryType", "FetchMergeData");
                dbcommand.Parameters.AddWithValue("@StickerId", UMR.StickerNo);
                dbcommand.Parameters.AddWithValue("@UserId", UMR.UserId);
                dbcommand.Parameters.AddWithValue("@ActivityID", "Merge");
                SqlDataAdapter daGetData = new SqlDataAdapter(dbcommand);
                DataSet dsGetData = new DataSet();
                daGetData.Fill(dsGetData);
                var output = "";
                if (dsGetData.Tables[0].Rows[0]["value"].ToString() != "2")
                {

                    string value = obj.InsertHistoryHeaderData(Cct, UMR.StickerNo, Convert.ToString(dsGetData.Tables[0].Rows[0]["SiteID"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ItemId"]), Convert.ToDecimal(dsGetData.Tables[0].Rows[0]["WorkOrderQty"]));

                    string MergeFrom = "";
                    decimal MergeQty = 0;
                    decimal StillageQty = 0;
                    StillageQty = Convert.ToDecimal(dsGetData.Tables[0].Rows[0]["StillageQty"]);
                    string s = UMR.MergeStickers;
                    string[] values = s.Split(',');
                    var dict = new Dictionary<string, string>();
                    for (int i = 0; i < values.Length - 1; i++)
                    {
                        string[] values1 = values[i].Split(':');
                        MergeFrom = values1[0];
                        //values1[i] = values1[i].Trim();
                        MergeQty = Convert.ToDecimal(values1[1]);
                        dict.Add(values1[0], values1[1]);
                        decimal TotalQty = StillageQty + MergeQty;
                        StillageQty = TotalQty;

                        query = "Sp_MergeStillage";
                        dbcommand = new SqlCommand(query, conn);
                        dbcommand.CommandType = CommandType.StoredProcedure;
                        dbcommand.CommandTimeout = 0;
                        dbcommand.Parameters.AddWithValue("@QueryType", "FetchMergeData");
                        dbcommand.Parameters.AddWithValue("@StickerId", MergeFrom);
                        dbcommand.Parameters.AddWithValue("@UserId", UMR.UserId);
                        dbcommand.Parameters.AddWithValue("@ActivityID", "Merge");
                        SqlDataAdapter daData = new SqlDataAdapter(dbcommand);
                        DataSet dsData = new DataSet();
                        daData.Fill(dsData);

                        var Hold = "";
                        if (dsGetData.Tables[0].Rows[0]["isHold"].ToString() == "True")
                        {
                            Hold = "Qc Hold";
                        }
                        else
                        {
                            Hold = "Qc Release";
                        }



                        decimal ReducedQty = Convert.ToDecimal(dsData.Tables[0].Rows[0]["StillageQty"]) - MergeQty;
                        if (ReducedQty == 0)
                        {
                            query = "[Sp_MasterDataWebApi]";
                            dbcommand = new SqlCommand(query, conn);
                            dbcommand.CommandType = CommandType.StoredProcedure;
                            dbcommand.CommandTimeout = 0;
                            dbcommand.Parameters.AddWithValue("@QueryType", "GetActivityMaster");
                            dbcommand.Parameters.AddWithValue("@ActivityID", "Merge & Discarded");
                            SqlDataAdapter daGetData1 = new SqlDataAdapter(dbcommand);
                            DataSet dsGetData1 = new DataSet();
                            daGetData.Fill(dsGetData1);
                            obj.InsertHistoryDetailData(Cct, UMR.StickerNo, "", Convert.ToString(dsGetData1.Tables[0].Rows[0]["ActivityName"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ActivityDesc"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["StillageLocation"]), "", "", "", "", "", "", "No", MergeQty, MergeFrom, 0, 0, StillageQty, Convert.ToString(dsGetData.Tables[0].Rows[0]["UserName"]), Hold, 0, "","","", Convert.ToString(dsGetData.Tables[0].Rows[0]["WareHouseID"]),0);
                            obj.UpdateStillageQty(Cct, UMR.StickerNo, Convert.ToString(dsGetData.Tables[0].Rows[0]["StillageLocation"]), StillageQty);
                            obj.InsertHistoryDetailData(Cct, MergeFrom, "", Convert.ToString(dsGetData1.Tables[0].Rows[0]["ActivityName"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ActivityDesc"]), Convert.ToString(dsData.Tables[0].Rows[0]["StillageLocation"]), "", "", "", "", "", "", "No", MergeQty, "", 0, 0, ReducedQty, Convert.ToString(dsData.Tables[0].Rows[0]["UserName"]), Hold, 0, UMR.StickerNo,"","", Convert.ToString(dsGetData.Tables[0].Rows[0]["WareHouseID"]),0);
                            obj.UpdateStillageQty(Cct, MergeFrom, Convert.ToString(dsData.Tables[0].Rows[0]["StillageLocation"]), ReducedQty);
                        }
                        else
                        {
                            obj.InsertHistoryDetailData(Cct, UMR.StickerNo, "", Convert.ToString(dsGetData.Tables[0].Rows[0]["ActivityName"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ActivityDesc"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["StillageLocation"]), "", "", "", "", "", "", "No", MergeQty, MergeFrom, 0, 0, StillageQty, Convert.ToString(dsGetData.Tables[0].Rows[0]["UserName"]), Hold, 0, "","","", Convert.ToString(dsGetData.Tables[0].Rows[0]["WareHouseID"]),0);
                            obj.UpdateStillageQty(Cct, UMR.StickerNo, Convert.ToString(dsGetData.Tables[0].Rows[0]["StillageLocation"]), StillageQty);
                            obj.InsertHistoryDetailData(Cct, MergeFrom, "", Convert.ToString(dsGetData.Tables[0].Rows[0]["ActivityName"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ActivityDesc"]), Convert.ToString(dsData.Tables[0].Rows[0]["StillageLocation"]), "", "", "", "", "", "", "No", MergeQty, "", 0, 0, ReducedQty, Convert.ToString(dsData.Tables[0].Rows[0]["UserName"]), Hold, 0, UMR.StickerNo,"","", Convert.ToString(dsGetData.Tables[0].Rows[0]["WareHouseID"]),0);
                            obj.UpdateStillageQty(Cct, MergeFrom, Convert.ToString(dsData.Tables[0].Rows[0]["StillageLocation"]), ReducedQty);

                        }
                    }
                     output = Newtonsoft.Json.JsonConvert.SerializeObject(dict);
                }
                else
                {
                    SM.Status = "Failure";
                    SM.Message = "This Stillage Does Not Exist";
                    return SM;
                }

                query = "Sp_MergeStillage";
                dbcommand = new SqlCommand(query, conn);
                //dbcommand.Connection.Open();
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.Parameters.AddWithValue("@QueryType", "UpdateMergeStillage");
                dbcommand.Parameters.AddWithValue("@StickerId", UMR.StickerNo);
                dbcommand.Parameters.AddWithValue("@UserId", UMR.UserId);
                dbcommand.Parameters.AddWithValue("@MergeStillage", output);
                dbcommand.Parameters.AddWithValue("@MergeQuantity", UMR.TotalMergeQty);


                dbcommand.CommandTimeout = 0;
                da = new SqlDataAdapter(dbcommand);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows[0]["value"].ToString() == "1")
                {
                    SM.Status = "Success";
                    SM.Message = "Stillage merged successfully";
                }

                else
                {
                    SM.Status = "Failure";
                    SM.Message = "Stillage not merged successfully";
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

public class ScanMergeResponse
{
    public List<SiteList> SiteListData { get; set; }
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
}

public class UpdateMergeResponse
{
    public string StickerNo { get; set; }
    public Int64 UserId { get; set; }
    public string MergeStickers { get; set; }
    public decimal TotalMergeQty { get; set; }
    public decimal ActivityID { get; set; }
}
public class SiteList
{
    public string id { get; set; }
    public string name { get; set; }
}

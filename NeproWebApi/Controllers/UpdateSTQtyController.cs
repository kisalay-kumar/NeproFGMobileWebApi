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
    public class UpdateSTQtyController : ApiController
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Conn"].ConnectionString);
        string query = ""; SqlCommand dbcommand;


        //public STDetailResponce STDetailUpdate(STDetailRequest st)
        //{
        //    STDetailResponce SM = new STDetailResponce();
        //    try
        //    {
        //            if (st.StickerNo == ""||st.StickerNo==null)
        //            {
        //                SM.Status = "Failure";
        //                SM.Message = "Enter Sticker Id";
        //                return SM;
        //        }
        //            if (st.UserId == ""||st.UserId==null)
        //            {
        //                SM.Status = "Failure";
        //                SM.Message = "Invalid UserId";
        //                return SM;
        //        }
        //        if (st.Quantity == "" || st.Quantity == null )
        //        {
        //            SM.Status = "Failure";
        //            SM.Message = "Please Enter Valid Quantity";
        //            return SM;
        //        }


        //        query = "Sp_AssignWebApi";
        //        dbcommand = new SqlCommand(query, conn);
        //        dbcommand.CommandType = CommandType.StoredProcedure;
        //        dbcommand.CommandTimeout = 0;
        //        dbcommand.Parameters.AddWithValue("@QueryType", "FetchDataUpdateSTQty");
        //        dbcommand.Parameters.AddWithValue("@StillageID", st.StickerNo);
        //        dbcommand.Parameters.AddWithValue("@UserId", st.UserId);
        //        //dbcommand.Parameters.AddWithValue("@ActivityID", st.ActivityID);
        //        dbcommand.Parameters.AddWithValue("@Reason", st.Reason);
        //        SqlDataAdapter daGetData = new SqlDataAdapter(dbcommand);
        //        DataSet dsGetData = new DataSet();
        //        daGetData.Fill(dsGetData);
        //        SqlDataAdapter da = new SqlDataAdapter(dbcommand);

        //        var Hold = "";
        //        if (dsGetData.Tables[0].Rows[0]["isHold"].ToString() == "True")
        //        {
        //            Hold = "Qc Hold";
        //        }
        //        else
        //        {
        //            Hold = "Qc Release";
        //        }


        //        if (dsGetData.Tables[0].Rows[0]["value"].ToString() != "2")
        //        {
        //            query = "Sp_AxWebserviceIntegration";
        //            dbcommand = new SqlCommand(query, conn);
        //            //  dbcommand.Connection.Open();
        //            dbcommand.CommandType = CommandType.StoredProcedure;
        //            dbcommand.CommandTimeout = 0;
        //             da = new SqlDataAdapter(dbcommand);
        //            DataSet ds = new DataSet();
        //            da.Fill(ds);

        //            //  DataSet ds = CommonManger.FillDatasetWithParam("Sp_AxWebserviceIntegration");
        //            AXWebServiceRef1.Iace_FinishedGoodServiceClient obj = new AXWebServiceRef1.Iace_FinishedGoodServiceClient();
        //            obj.ClientCredentials.Windows.ClientCredential.Domain = Convert.ToString(ds.Tables[0].Rows[0]["Domain"]);
        //            obj.ClientCredentials.Windows.ClientCredential.UserName = Convert.ToString(ds.Tables[0].Rows[0]["Username"]);
        //            obj.ClientCredentials.Windows.ClientCredential.Password = Convert.ToString(ds.Tables[0].Rows[0]["Password"]);

        //            AXWebServiceRef1.CallContext Cct = new AXWebServiceRef1.CallContext();
        //            Cct.Company = Convert.ToString(ds.Tables[0].Rows[0]["Company"]);
        //            Cct.Language = Convert.ToString(ds.Tables[0].Rows[0]["Language"]);
        //            string value = obj.InsertHistoryHeaderData(Cct, st.StickerNo, Convert.ToString(dsGetData.Tables[0].Rows[0]["SiteID"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ItemId"]), 0);
        //            obj.InsertHistoryDetailData(Cct, st.StickerNo, "", Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityName"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityDesc"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), "", "", "", "", Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonName"]), Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonDes"]), "No", 0, "", 0, 0, Convert.ToDecimal(st.Quantity), Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]), Hold, 0, "","","", Convert.ToString(dsGetData.Tables[1].Rows[0]["WareHouseID"]),0);
        //            obj.UpdateStillageQty(Cct, st.StickerNo, Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), Convert.ToDecimal(st.Quantity));
        //        }
        //        else
        //        {
        //            SM.Status = "Failure";
        //            SM.Message = "This Stillage Does Not Exist";
        //            return SM;
        //        }
        //        query = "Sp_AssignWebApi";
        //        dbcommand = new SqlCommand(query, conn);
        //        dbcommand.Connection.Open();
        //        dbcommand.CommandType = CommandType.StoredProcedure;
        //        dbcommand.Parameters.AddWithValue("@QueryType", "UpdateSTDetails");
        //        dbcommand.Parameters.AddWithValue("@StickerId", st.StickerNo);
        //        dbcommand.Parameters.AddWithValue("@Reason", st.Reason);
        //        dbcommand.Parameters.AddWithValue("@UpdatedQty", st.Quantity);
        //        dbcommand.Parameters.AddWithValue("@UserId", st.UserId);
        //        dbcommand.CommandTimeout = 0;
        //        da = new SqlDataAdapter(dbcommand);
        //        DataTable dt = new DataTable();
        //        da.Fill(dt);
        //        if (dt.Rows[0]["value"].ToString() == "1")
        //        {
        //            SM.Status = "Success";
        //            SM.Message = "Stillage Quantity Updated Successfully";
        //        }
        //        else
        //        {
        //            SM.Status = "Failure";
        //            SM.Message = "Stillage Quantity Updated Failure";
        //        }

        //    }
        //    catch (Exception Ex)
        //    {
        //        SM.Status = "Failure";
        //        SM.Message = Ex.Message;
        //    }
        //    //finally
        //    //{
        //    //    //dbcommand.Connection.Close();
        //    //}
        //    return SM;
        //}
        [Route("api/Nepro/UpdateStillageDetails")]
        [HttpPost]
        public STDetailResponce VariancePosting(STDetailRequest st)
        {
            STDetailResponce SM = new STDetailResponce();
           
            try
            {
                SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                if (st.StickerNo == "" || st.StickerNo == null)
                {
                    SM.Status = "Failure";
                    SM.Message = "Enter Sticker Id";
                    return SM;
                }
                if (st.UserId == "" || st.UserId == null)
                {
                    SM.Status = "Failure";
                    SM.Message = "Invalid UserId";
                    return SM;
                }
                if (st.Quantity == "" || st.Quantity == null)
                {
                    SM.Status = "Failure";
                    SM.Message = "Please Enter Valid Quantity";
                    return SM;
                }
                query = "Sp_Variance";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.CommandTimeout = 0;
                dbcommand.Parameters.AddWithValue("@QueryType", "FetchVarianceData");
                dbcommand.Parameters.AddWithValue("@StillageID", st.StickerNo);
                dbcommand.Parameters.AddWithValue("@UserId", st.UserId);
                dbcommand.Parameters.AddWithValue("@Reason", st.Reason);
                SqlDataAdapter daGetData = new SqlDataAdapter(dbcommand);
                DataSet dsGetData = new DataSet();
                daGetData.Fill(dsGetData);

                query = "Sp_AxWebserviceIntegration";
                dbcommand = new SqlCommand(query, conn);
               // dbcommand.Connection.Open();
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.CommandTimeout = 0;
                da = new SqlDataAdapter(dbcommand);
                DataSet ds = new DataSet();
                da.Fill(ds);

                AXWebServiceRef1.Iace_FinishedGoodServiceClient obj = new AXWebServiceRef1.Iace_FinishedGoodServiceClient();
                obj.ClientCredentials.Windows.ClientCredential.Domain = Convert.ToString(ds.Tables[0].Rows[0]["Domain"]);
                obj.ClientCredentials.Windows.ClientCredential.UserName = Convert.ToString(ds.Tables[0].Rows[0]["Username"]);
                obj.ClientCredentials.Windows.ClientCredential.Password = Convert.ToString(ds.Tables[0].Rows[0]["Password"]);

                AXWebServiceRef1.CallContext Cct = new AXWebServiceRef1.CallContext();
                Cct.Company = Convert.ToString(ds.Tables[0].Rows[0]["Company"]);
                Cct.Language = Convert.ToString(ds.Tables[0].Rows[0]["Language"]);

                var QCHold = "";
                if (dsGetData.Tables[0].Rows[0]["isHold"].ToString() == "1")
                {
                    QCHold = "Qc Hold";
                }
                else
                {
                    QCHold = "QC Release";
                }
                string ActivityName = "";
                string ActivityDesc = "";
                if (st.Quantity != "0")
                {
                    ActivityName = Convert.ToString(dsGetData.Tables[0].Rows[0]["ActivityName"]);
                    ActivityDesc = Convert.ToString(dsGetData.Tables[0].Rows[0]["ActivityDesc"]);
                }
                else
                {
                    ActivityName = "Updated And Discarded";
                    ActivityDesc = "Updated And Discarded";
                }
                bool Autoroute = true;
                bool Autopick = true; 
                if (st.AutoPick == 1)
                {
                    Autopick = true;
                }
                else
                {
                    Autopick = false;
                }
                if(st.AutoRoute==1)
                {
                    Autoroute = true;
                }
                else
                {
                    Autoroute = false;
                }
                decimal Variance = 0;
                //if (dsGetData.Tables[0].Rows[0]["STRP"].ToString() == "1")
                //{
                //    Variance = Convert.ToDecimal(st.Variance) * Convert.ToDecimal((-1)); 
                //}
                //else
                //{
                    Variance = Convert.ToDecimal(st.Variance);
                //}

                if (dsGetData.Tables[1].Rows[0]["Status"].ToString() == "7")
                {
                    SM.Status = "Failure";
                    SM.Message = "The WO is Ended, please contact Accounts dept. for adjustment";

                }
                else if (dsGetData.Tables[1].Rows[0]["Status"].ToString() == "5" && dsGetData.Tables[1].Rows[0]["RemainingStatus"].ToString() == "5")
                {
                    SM.Status = "Failure";
                    SM.Message = "The WO is Ended, please contact Accounts dept. for adjustment";
                }
                else if (Convert.ToBoolean(dsGetData.Tables[1].Rows[0]["IsCounted"]) != true)
                {

                    obj.UpdateQty(Cct, Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), Autopick, Autoroute, "A", st.StickerNo, "HHD", Convert.ToDecimal(st.Quantity), Convert.ToDecimal(Variance));

                    obj.InsertHistoryHeaderData(Cct, st.StickerNo, Convert.ToString(dsGetData.Tables[0].Rows[0]["SiteID"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ItemId"]), Convert.ToDecimal(dsGetData.Tables[0].Rows[0]["WorkOrderQty"]));

                   // obj.InsertHistoryDetailData(Cct, st.StickerNo, "", ActivityName, ActivityDesc, Convert.ToString(dsGetData.Tables[0].Rows[0]["StillageLocation"]), "", "", "", "", Convert.ToString(dsGetData.Tables[0].Rows[0]["ReasonName"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ReasonDes"]), "Yes", 0, "", 0, Convert.ToDecimal(st.Quantity), Convert.ToDecimal(st.Quantity), Convert.ToString(dsGetData.Tables[0].Rows[0]["UserName"]), QCHold, 0, "", "", "", Convert.ToString(dsGetData.Tables[0].Rows[0]["WareHouseID"]), 0);
                    obj.InsertHistoryDetailData(Cct, st.StickerNo, "", ActivityName, ActivityDesc, Convert.ToString(dsGetData.Tables[0].Rows[0]["StillageLocation"]), "", "", "", "", Convert.ToString(dsGetData.Tables[0].Rows[0]["ReasonName"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ReasonDes"]), "Yes", 0, "", 0, Convert.ToDecimal(st.Quantity), Convert.ToDecimal(st.Quantity), Convert.ToString(dsGetData.Tables[0].Rows[0]["UserName"]), QCHold, 0, "", "", "", Convert.ToString(dsGetData.Tables[0].Rows[0]["WareHouseID"]));

                    //dbcommand.Connection.Close();
                    query = "Sp_Variance";
                    dbcommand = new SqlCommand(query, conn);
                    dbcommand.Connection.Open();
                    dbcommand.CommandType = CommandType.StoredProcedure;
                    dbcommand.Parameters.AddWithValue("@QueryType", "UpdateQty");
                    dbcommand.Parameters.AddWithValue("@EntQty", st.Quantity);
                    dbcommand.Parameters.AddWithValue("@StillageId", st.StickerNo);
                    dbcommand.CommandTimeout = 0;
                    da = new SqlDataAdapter(dbcommand);
                    ds = new DataSet();
                    da.Fill(ds);
                    SM.Status = "Success";
                    SM.Message = "Quantity Updated and Variance Posted Successfully..!";
                }
                else
                {

                    obj.UpdateQty(Cct, Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), Autopick, Autoroute, "A", st.StickerNo, "HHD", Convert.ToDecimal(Variance), Convert.ToDecimal(Variance));

                    obj.InsertHistoryHeaderData(Cct, st.StickerNo, Convert.ToString(dsGetData.Tables[0].Rows[0]["SiteID"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ItemId"]), Convert.ToDecimal(dsGetData.Tables[0].Rows[0]["WorkOrderQty"]));

                    //obj.InsertHistoryDetailData(Cct, st.StickerNo, "", ActivityName, ActivityDesc, Convert.ToString(dsGetData.Tables[0].Rows[0]["StillageLocation"]), "", "", "", "", Convert.ToString(dsGetData.Tables[0].Rows[0]["ReasonName"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ReasonDes"]), "Yes", 0, "", 0, Convert.ToDecimal(Variance), Convert.ToDecimal(st.Quantity), Convert.ToString(dsGetData.Tables[0].Rows[0]["UserName"]), QCHold, 0, "", "", "", Convert.ToString(dsGetData.Tables[0].Rows[0]["WareHouseID"]), 0);
                    obj.InsertHistoryDetailData(Cct, st.StickerNo, "", ActivityName, ActivityDesc, Convert.ToString(dsGetData.Tables[0].Rows[0]["StillageLocation"]), "", "", "", "", Convert.ToString(dsGetData.Tables[0].Rows[0]["ReasonName"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ReasonDes"]), "Yes", 0, "", 0, Convert.ToDecimal(Variance), Convert.ToDecimal(st.Quantity), Convert.ToString(dsGetData.Tables[0].Rows[0]["UserName"]), QCHold, 0, "", "", "", Convert.ToString(dsGetData.Tables[0].Rows[0]["WareHouseID"]));

                    //dbcommand.Connection.Close();
                    query = "Sp_Variance";
                    dbcommand = new SqlCommand(query, conn);
                    dbcommand.Connection.Open();
                    dbcommand.CommandType = CommandType.StoredProcedure;
                    dbcommand.Parameters.AddWithValue("@QueryType", "UpdateQty");
                    dbcommand.Parameters.AddWithValue("@EntQty", st.Quantity);
                    dbcommand.Parameters.AddWithValue("@StillageId", st.StickerNo);
                    dbcommand.CommandTimeout = 0;
                    da = new SqlDataAdapter(dbcommand);
                    ds = new DataSet();
                    da.Fill(ds);
                    SM.Status = "Success";
                    SM.Message = "Quantity Updated and Variance Posted Successfully..!";

                }
              
               
                return SM;
            }

            catch (Exception Ex)
            {
                SM.Status = "Failed";
                SM.Message = Ex.Message;
                return SM;
            }
            finally
            {
                dbcommand.Connection.Close();
            }
        }
        }
}
public class STDetailRequest
{
    public string StickerNo { get; set; }
    //public string ActivityID { get; set; }
    public string Quantity { get; set; }
    public string Reason { get; set; }
    public string UserId { get; set; }
    public string Variance { get; set; }
    public int AutoRoute { get; set; }
    public int AutoPick { get; set; }
}
public class STDetailResponce
{
    public string Status { get; set; }
    public string Message { get; set; }


}
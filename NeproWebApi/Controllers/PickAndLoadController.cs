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
                    obj.InsertHistoryDetailData(Cct, PAL.StickerNo, Convert.ToString(dsGetData.Tables[1].Rows[0]["PlanningId"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityName"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityDesc"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), "", "", "", "", Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonName"]), Convert.ToString(dsGetData.Tables[2].Rows[0]["ReasonDes"]), "No", Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["MergeQty"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["FromMerge"]), PAL.LoadQuantity, 0, Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["StillageQty"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]), "Reserved", 0, "","","", Convert.ToString(dsGetData.Tables[1].Rows[0]["WareHouseID"]),0);
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
    }
}


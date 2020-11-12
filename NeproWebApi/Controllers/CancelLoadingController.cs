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

namespace NeproWebApi.Controllers
{
    public class CancelLoadingController : ApiController
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Conn"].ConnectionString);
        string query = ""; SqlCommand dbcommand;

        [Route("api/Nepro/CancelLoading")]
        [HttpPost]
        public CancelLoadingkResponse CancelLoadingProcess(CancelLoading CL)
        {
            CancelLoadingkResponse SM = new CancelLoadingkResponse();
            try
            {

                if (CL.UserId == "" || CL.UserId == null)
                {
                    SM.Status = "Failure";
                    SM.Message = "Invalid UserId";
                    return SM;
                }
                if (CL.LPID == "" || CL.LPID == null)
                {
                    SM.Status = "Failure";
                    SM.Message = "Invalid LoadingId";
                    return SM;
                }





             
                query = "Sp_LadingPlanWebApi";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.CommandTimeout = 0;
                dbcommand.Parameters.AddWithValue("@QueryType", "FetchStillagesId");
                dbcommand.Parameters.AddWithValue("@LPID", CL.LPID);
                dbcommand.Parameters.AddWithValue("@UserId", CL.UserId);
                SqlDataAdapter daGetData1 = new SqlDataAdapter(dbcommand);
                DataSet dsGetData1 = new DataSet();
                daGetData1.Fill(dsGetData1);





                    foreach (DataRow row in dsGetData1.Tables[0].Rows)
                {
                    query = "Sp_LadingPlanWebApi";
                    dbcommand = new SqlCommand(query, conn);
                    dbcommand.CommandType = CommandType.StoredProcedure;
                    dbcommand.CommandTimeout = 0;
                    dbcommand.Parameters.AddWithValue("@QueryType", "FetchStillagesData");
                    dbcommand.Parameters.AddWithValue("@StillageID", row["StickerID"].ToString());
                    dbcommand.Parameters.AddWithValue("@UserId", CL.UserId);
                    dbcommand.Parameters.AddWithValue("@LoadingId", CL.LPID);
                    SqlDataAdapter daGetData = new SqlDataAdapter(dbcommand);
                    DataSet dsGetData = new DataSet();
                    daGetData.Fill(dsGetData);


                    query = "Sp_AxWebserviceIntegration";
                    dbcommand = new SqlCommand(query, conn);
                    //  dbcommand.Connection.Open();
                    dbcommand.CommandType = CommandType.StoredProcedure;
                    dbcommand.CommandTimeout = 0;
                    SqlDataAdapter da1 = new SqlDataAdapter(dbcommand);
                    DataSet ds = new DataSet();
                    da1.Fill(ds);

                    //  DataSet ds = CommonManger.FillDatasetWithParam("Sp_AxWebserviceIntegration");
                    AXWebServiceRef1.Iace_FinishedGoodServiceClient obj = new AXWebServiceRef1.Iace_FinishedGoodServiceClient();
                    obj.ClientCredentials.Windows.ClientCredential.Domain = Convert.ToString(ds.Tables[0].Rows[0]["Domain"]);
                    obj.ClientCredentials.Windows.ClientCredential.UserName = Convert.ToString(ds.Tables[0].Rows[0]["Username"]);
                    obj.ClientCredentials.Windows.ClientCredential.Password = Convert.ToString(ds.Tables[0].Rows[0]["Password"]);

                    AXWebServiceRef1.CallContext Cct = new AXWebServiceRef1.CallContext();
                    Cct.Company = Convert.ToString(ds.Tables[0].Rows[0]["Company"]);
                    Cct.Language = Convert.ToString(ds.Tables[0].Rows[0]["Language"]);
                    string value = obj.InsertHistoryHeaderData(Cct, Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageID"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["SiteID"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ItemId"]), Convert.ToDecimal(dsGetData.Tables[0].Rows[0]["WorkOrderQty"]));
                    obj.InsertHistoryDetailData(Cct, Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageID"]), "", Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityName"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityDesc"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), "", "", "", "", "", "", "No", 0, "", 0, 0, Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["StillageQty"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]),"QC Release",0,"","","", Convert.ToString(dsGetData.Tables[1].Rows[0]["WareHouseID"]),0);


                }













                query = "Sp_LadingPlanWebApi";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.Connection.Open();
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.Parameters.AddWithValue("@QueryType", "CancelLoadingProcess");
                dbcommand.Parameters.AddWithValue("@LoadingId", CL.LPID);
                dbcommand.Parameters.AddWithValue("@UserId", CL.UserId);
                dbcommand.CommandTimeout = 0;
                SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows[0]["value"].ToString() == "1")
                {
                    SM.Status = "Success";
                    SM.Message = "Loading Cancel Successfully";
                }
                else
                {
                    SM.Status = "Failure";
                    SM.Message = "Loading Cancel Failure";
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

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using static NeproWebApi.Models.GetWorkOrder;
using NeproWebApi.Models;

namespace NeproWebApi.Controllers
{
    public class SearchItemController : ApiController
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Conn"].ConnectionString);
        string query = ""; SqlCommand dbcommand;

        [Route("api/Nepro/SearchItemProcess")]
        [HttpPost]
        public SearchItemResponse SearchItemProcess(SearchItemRequest SIR)
        {
            SearchItemResponse SM = new SearchItemResponse();
            try
            {

                if (SIR.UserId == "" || SIR.UserId == null)
                {
                    SM.Status = "Failure";
                    SM.Message = "Invalid UserId";
                    return SM;
                }
                if (SIR.WorkOrderNo == "" || SIR.WorkOrderNo == null)
                {
                    SM.Status = "Failure";
                    SM.Message = "Invalid WorkOrderNo";
                    return SM;
                }
                if (SIR.ItemName == "" || SIR.ItemName == null)
                {
                    SM.Status = "Failure";
                    SM.Message = "Invalid ItemName";
                    return SM;
                }

                query = "Sp_WorkOrderWebApi";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.Connection.Open();
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.Parameters.AddWithValue("@QueryType", "SearchItemProcess");
                dbcommand.Parameters.AddWithValue("@WorkOrderNo", SIR.WorkOrderNo);
                dbcommand.Parameters.AddWithValue("@UserId", SIR.UserId);
                dbcommand.Parameters.AddWithValue("@ItemName", SIR.ItemName);
                dbcommand.CommandTimeout = 0;
                SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    SM.ItemName = dt.Rows[0]["ItemName"].ToString();
                    SM.WorkOrderNo = dt.Rows[0]["WorkOrderNo"].ToString();
                    SM.ItemId = dt.Rows[0]["ItemId"].ToString();
                    SM.Status = "Success";
                    SM.Message = "Record Found";

                }
                else
                {
                    SM.Status = "Failure";
                    SM.Message = "Invalid Item Name";

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

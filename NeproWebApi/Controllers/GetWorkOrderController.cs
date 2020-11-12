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
using static NeproWebApi.Models.GetWorkOrder;

namespace NeproWebApi.Controllers
{
    public class GetWorkOrderController : ApiController
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Conn"].ConnectionString);
        string query = ""; SqlCommand dbcommand;

        [Route("api/Nepro/WorkOrderProcess")]
        [HttpPost]
        public GetOrderRespose WorkOrderProcess(GetOrderRequest GOR)
        {
            GetOrderRespose SM = new GetOrderRespose();
            try
            {

                if (GOR.UserId == "" || GOR.UserId == null)
                {
                    SM.Status = "Failure";
                    SM.Message = "Enter UserId";
                    return SM;
                }
                if (GOR.WorkOrderNo == "" || GOR.WorkOrderNo == null)
                {
                    SM.Status = "Failure";
                    SM.Message = "Invalid WorkOrderNo";
                    return SM;
                }

                query = "Sp_WorkOrderWebApi";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.Connection.Open();
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.Parameters.AddWithValue("@QueryType", "WorkOrderProcess");
                dbcommand.Parameters.AddWithValue("@WorkOrderNo", GOR.WorkOrderNo);
                dbcommand.Parameters.AddWithValue("@UserId", GOR.UserId);
                dbcommand.CommandTimeout = 0;
                SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows.Count > 0)
                {
                    SM.WorkOrderId = dt.Rows[0]["WorkOrderId"].ToString();
                    SM.WorkOrderNo = dt.Rows[0]["WorkOrderNo"].ToString();
                    SM.Status = "Success";
                    SM.Message = "Record Found";

                }
                else
                {
                    SM.Status = "Failure";
                    SM.Message = "Invalid WorkOrder No";
                    return SM;

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

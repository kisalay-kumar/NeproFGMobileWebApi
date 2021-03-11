using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
//using NeproWebApi.Models;
//using static NeproWebApi.Models.WorkOrder_Start_End;

namespace NeproWebApi.Controllers
{
    public class WorkOrder_Start_EndController : ApiController
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Conn"].ConnectionString);
        string query = ""; SqlCommand dbcommand;

        [Route("api/Nepro/WorkOrder_Start_EndProcess")]
        [HttpPost]
        public WorkOrder_Start_EndRES WorkOrderProcess(WorkOrder_Start_EndREQ WOSE)
        {
            WorkOrder_Start_EndRES SM = new WorkOrder_Start_EndRES();
            try
            {

                if (WOSE.UserId == "" || WOSE.UserId == null)
                {
                    SM.Status = "Failure";
                    SM.Message = "Invalid UserId";
                    return SM;
                }
                if (WOSE.WorkOrderNo == "" || WOSE.WorkOrderNo == null)
                {
                    SM.Status = "Failure";
                    SM.Message = "Invalid WorkOrderNo";
                    return SM;
                }

                query = "Sp_WorkOrderWebApi";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.Connection.Open();
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.Parameters.AddWithValue("@QueryType", "WorkOrder_Start_EndProcess");
                dbcommand.Parameters.AddWithValue("@WorkOrderNo", WOSE.WorkOrderNo);
                dbcommand.Parameters.AddWithValue("@UserId", WOSE.UserId);
                dbcommand.CommandTimeout = 0;
                SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                DataSet dt = new DataSet();
                da.Fill(dt);
                if (dt.Tables[0].Rows.Count > 0)
                {
                    SM.WorkOrderNo = dt.Tables[0].Rows[0]["WorkOrderNo"].ToString();
                    SM.ItemId = dt.Tables[0].Rows[0]["ItemId"].ToString();
                    SM.ItemName = dt.Tables[0].Rows[0]["ItemName"].ToString();
                    SM.ItemDescription = dt.Tables[0].Rows[0]["ItemDescription"].ToString();
                    SM.ProductionLine = dt.Tables[0].Rows[0]["ProductionLine"].ToString();
                    SM.WareHouse = dt.Tables[0].Rows[0]["WareHouse"].ToString();
                    SM.Quantity = dt.Tables[1].Rows[0]["CWQty"].ToString();
                    SM.Site = dt.Tables[0].Rows[0]["Site"].ToString();
                    SM.WOStatus = dt.Tables[0].Rows[0]["WOStatus"].ToString();
                    SM.StatusId = dt.Tables[0].Rows[0]["StatusId"].ToString();
                    SM.BalanceQuantity = dt.Tables[1].Rows[0]["BalanceCWQty"].ToString();
                    SM.RafQuantity = dt.Tables[2].Rows[0]["RAFQty"].ToString();
                    SM.StartedQty = dt.Tables[0].Rows[0]["StartedQty"].ToString();
                    SM.WareHouseId = dt.Tables[0].Rows[0]["WareHouseId"].ToString();
                    SM.SiteID = dt.Tables[0].Rows[0]["SiteID"].ToString();
                    SM.IsFinancialEnd = Convert.ToByte(dt.Tables[0].Rows[0]["IsFinancialEnd"]);

                    SM.Status = "Success";
                    SM.Message = "Records FOUND";

                }
                else
                {
                    SM.Status = "Failure";
                    SM.Message = "This WorkOrder does not exist";

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

        [Route("api/Nepro/WorkOrderStartService")]
        [HttpPost]
        public WorkOrderStartEndRES WorkOrderStartService(WorkOrder_Start_EndREQ WOSE)
        {
            WorkOrderStartEndRES SM = new WorkOrderStartEndRES();
            try
            {

                if (WOSE.UserId == "" || WOSE.UserId == null)
                {
                    SM.Status = "Failure";
                    SM.Message = "Invalid UserId";
                    return SM;
                }
                if (WOSE.WorkOrderNo == "" || WOSE.WorkOrderNo == null)
                {
                    SM.Status = "Failure";
                    SM.Message = "Invalid WorkOrderNo";
                    return SM;
                }

                bool AutoPicked = true;
                bool AutoRoute = true;
                if (WOSE.AutoPick == "1")
                {
                    AutoPicked = true;
                }
                else
                {
                    AutoPicked = false;
                }
                if (WOSE.AutoRoute == "1")
                {
                    AutoRoute = true;
                }
                else
                {
                    AutoRoute = false;
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
                Global global = new Global();
                string Username = global.GetUsername(WOSE.UserId.ToString());
                string value = obj.StartWorkOrder(Cct, WOSE.WorkOrderNo, AutoPicked, AutoRoute, Convert.ToDecimal(WOSE.StartQty), Username);


                query = "Sp_WorkOrderWebApi";
                dbcommand = new SqlCommand(query, conn);
                //dbcommand.Connection.Open();
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.Parameters.AddWithValue("@QueryType", "WorkOrder_Start");
                dbcommand.Parameters.AddWithValue("@WorkOrderNo", WOSE.WorkOrderNo);
                dbcommand.Parameters.AddWithValue("@StartQty", WOSE.StartQty);
                dbcommand.Parameters.AddWithValue("@UserId", WOSE.UserId);
                dbcommand.CommandTimeout = 0;
                da = new SqlDataAdapter(dbcommand);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows[0]["value"].ToString() == "1")
                {

                    SM.Status = "Success";
                    SM.Message = "WorkOrder Started";

                }
                else
                {
                    SM.Status = "Failure";
                    SM.Message = "Invalid WorkOrder No";

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

        [Route("api/Nepro/WorkOrderEndService")]
        [HttpPost]
        public WorkOrderStartEndRES WorkOrderEndService(WorkOrder_Start_EndREQ WOSE)
        {
            WorkOrderStartEndRES SM = new WorkOrderStartEndRES();
            try
            {

                if (WOSE.UserId == "" || WOSE.UserId == null)
                {
                    SM.Status = "Failure";
                    SM.Message = "Invalid UserId";
                    return SM;
                }
                if (WOSE.WorkOrderNo == "" || WOSE.WorkOrderNo == null)
                {
                    SM.Status = "Failure";
                    SM.Message = "Invalid WorkOrderNo";
                    return SM;
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
                Global global = new Global();
                string Username = global.GetUsername(WOSE.UserId.ToString());
                string value = obj.EndWorkOrder(Cct, WOSE.WorkOrderNo,Username);





                query = "Sp_WorkOrderWebApi";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.Connection.Open();
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.Parameters.AddWithValue("@QueryType", "WorkOrder_End");
                dbcommand.Parameters.AddWithValue("@WorkOrderNo", WOSE.WorkOrderNo);
                dbcommand.Parameters.AddWithValue("@UserId", WOSE.UserId);
                dbcommand.CommandTimeout = 0;
                da = new SqlDataAdapter(dbcommand);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows[0]["value"].ToString() == "1")
                {

                    SM.Status = "Success";
                    SM.Message = "WorkOrder Ended Successfully";

                }
                else
                {
                    SM.Status = "Failure";
                    SM.Message = "Invalid WorkOrder No";

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

        [Route("api/Nepro/WorkOrderFinancialEnd")]
        [HttpPost]
        public WorkOrderStartEndRES WorkOrderFinancialEnd(WorkOrder_Start_EndREQ WOSE)
        {
            WorkOrderStartEndRES SM = new WorkOrderStartEndRES();
           //// string date = DateTime.Now.ToString("dd/MM/yyyy");
           // Random rand = new Random();
           // string RandomNumer = rand.Next(11111, 99999).ToString();
            try
            {

                //if (WOSE.UserId == "" || WOSE.UserId == null)
                //{
                //    SM.Status = "Failure";
                //    SM.Message = "Invalid UserId";
                //    return SM;
                //}
                //if (WOSE.WorkOrderNo == "" || WOSE.WorkOrderNo == null)
                //{
                //    SM.Status = "Failure";
                //    SM.Message = "Invalid WorkOrderNo";
                //    return SM;
                //}

                //query = "Sp_AxWebserviceIntegration";
                //dbcommand = new SqlCommand(query, conn);
                ////  dbcommand.Connection.Open();
                //dbcommand.CommandType = CommandType.StoredProcedure;
                //dbcommand.CommandTimeout = 0;
                //SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                //DataSet ds = new DataSet();
                //da.Fill(ds);

                ////  DataSet ds = CommonManger.FillDatasetWithParam("Sp_AxWebserviceIntegration");
                //AXWebServiceRef1.Iace_FinishedGoodServiceClient obj = new AXWebServiceRef1.Iace_FinishedGoodServiceClient();
                //obj.ClientCredentials.Windows.ClientCredential.Domain = Convert.ToString(ds.Tables[0].Rows[0]["Domain"]);
                //obj.ClientCredentials.Windows.ClientCredential.UserName = Convert.ToString(ds.Tables[0].Rows[0]["Username"]);
                //obj.ClientCredentials.Windows.ClientCredential.Password = Convert.ToString(ds.Tables[0].Rows[0]["Password"]);

                //AXWebServiceRef1.CallContext Cct = new AXWebServiceRef1.CallContext();
                //Cct.Company = Convert.ToString(ds.Tables[0].Rows[0]["Company"]);
                //Cct.Language = Convert.ToString(ds.Tables[0].Rows[0]["Language"]);

                ////string value = obj.EndWorkOrder(Cct, WOSE.WorkOrderNo);
              
                //string value = obj.ReportAsFinished(Cct, WOSE.WorkOrderNo, true, true, "A",WOSE.WorkOrderNo, "HHD",true, true, true);



                //query = "Sp_WorkOrderWebApi";
                //dbcommand = new SqlCommand(query, conn);
                //dbcommand.Connection.Open();
                //dbcommand.CommandType = CommandType.StoredProcedure;
                //dbcommand.Parameters.AddWithValue("@QueryType", "WorkOrderFinancialEnd");
                //dbcommand.Parameters.AddWithValue("@WorkOrderNo", WOSE.WorkOrderNo);
                //dbcommand.Parameters.AddWithValue("@UserId", WOSE.UserId);
                //dbcommand.CommandTimeout = 0;
                //da = new SqlDataAdapter(dbcommand);
                //DataTable dt = new DataTable();
                //da.Fill(dt);
                //if (dt.Rows[0]["value"].ToString() == "1")
                //{

                //    SM.Status = "Success";
                //    SM.Message = "Workorder end job successfully";

                //}
                //else
                //{
                //    SM.Status = "Failure";
                //    SM.Message = "Invalid WorkOrder No";

                //}

                SM.Status = "Failure";
                SM.Message = "This process is not working. Please contact to Admin";
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



public class WorkOrder_Start_EndREQ
{
    public string WorkOrderNo { get; set; }
    public string UserId { get; set; }
    public string AutoPick { get; set; }
    public string AutoRoute { get; set; }
    public string StartQty { get; set; }
}
public class WorkOrder_Start_EndRES
{

    public byte IsFinancialEnd { get; set; }
    public string WorkOrderNo { get; set; }
    public string ItemId { get; set; }
    public string ItemName { get; set; }
    public string ItemDescription { get; set; }
    public string ProductionLine { get; set; }
    public string WareHouse { get; set; }
    public string Quantity { get; set; }
    public string Site { get; set; }
    public string WOStatus { get; set; }
    public string StatusId { get; set; }
    public string BalanceQuantity { get; set; }
    public string RafQuantity { get; set; }
    public string StartedQty { get; set; }
    public string WareHouseId { get; set; }
    public string SiteID { get; set; }
    public string Status { get; set; }
    public string Message { get; set; }



}
public class WorkOrderStartEndRES
{
    public string Status { get; set; }
    public string Message { get; set; }

}


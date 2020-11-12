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
    public class SubmitProductionJournalController : ApiController
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Conn"].ConnectionString);
        string query = ""; SqlCommand dbcommand;

        [Route("api/Nepro/GetProductionJournalData")]
        [HttpPost]
        public ProductionJournalResponse GetProductionJournalData(ProductionJournalRequest PJR)
        {
            ProductionJournalResponse SM = new ProductionJournalResponse();
            try
            {

                if (PJR.UserId == "" || PJR.UserId == null)
                {
                    SM.Status = "Failure";
                    SM.Message = "Invalid UserId";
                    return SM;
                }
                if (PJR.WorkOrderNo == "" || PJR.WorkOrderNo == null)
                {
                    SM.Status = "Failure";
                    SM.Message = "Invalid WorkOrderId";
                    return SM;
                }


                query = "Sp_WorkOrderWebApi";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.Connection.Open();
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.Parameters.AddWithValue("@QueryType", "GetProductionJournalData");
                dbcommand.Parameters.AddWithValue("@WorkOrderNo", PJR.WorkOrderNo);
                dbcommand.Parameters.AddWithValue("@UserId", PJR.UserId);
                dbcommand.CommandTimeout = 0;
                SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                DataSet data = new DataSet();
                da.Fill(data);

                if (data.Tables[0].Rows[0]["value"].ToString() != "2")
                {
                    SM.WorkOrderNo = PJR.WorkOrderNo;
                    SM.ItemId = data.Tables[0].Rows[0]["ITEMID"].ToString();
                    SM.ItemName = data.Tables[0].Rows[0]["NAME"].ToString();
                    SM.Quantity = data.Tables[0].Rows[0]["QTYSCHED"].ToString();
                    SM.WareHouseId = data.Tables[3].Rows[0]["WareHouseId"].ToString();
                    SM.SiteID = data.Tables[3].Rows[0]["SiteID"].ToString();


                    List<PickingList> PickingList = new List<PickingList>();
                    foreach (DataRow row in data.Tables[1].Rows)
                    {
                        PickingList A = new PickingList();
                        A.ItemName = row["NAME"].ToString();
                        A.Unit = row["UNITID"].ToString();
                        A.ItemId = row["ITEMID"].ToString();
                        PickingList.Add(A);
                    }
                    SM.PickingListData = PickingList;

                    List<RoutingCardList> RoutingList = new List<RoutingCardList>();
                    foreach (DataRow row in data.Tables[2].Rows)
                    {
                        RoutingCardList A = new RoutingCardList();
                        A.OperationName = row["Operation"].ToString(); 
                        A.Resource = row["Resource"].ToString();
                        A.ResourceType = row["ResourceType"].ToString();
                        A.OperationId = row["OperationNo"].ToString();
                        A.Priority = row["PRIORITY"].ToString();
                        RoutingList.Add(A);
                    }
                    SM.RoutingListData = RoutingList;

                    SM.Status = "Success";
                    SM.Message = "Picking List Success ";
                }
                else
                {
                    SM.Status = "Failure";
                    SM.Message = "This Workorder does not exist";
                    return SM;
                }
                //if (ds.Rows[0]["value"].ToString() == "1")
                //{
                //    SM.Status = "Success";
                //    SM.Message = "SubmitProductionJournalProcess Successfully";
                //}
                //else
                //{
                //    SM.Status = "Failure";
                //    SM.Message = "SubmitProductionJournalProcess Failure";
                //}

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


            [Route("api/Nepro/SubmitPickingJournalData")]
        [HttpPost]
        public SubmitProductionJournalResponse SubmitPickingJournalData(SubmitProductionJournalRequest SPJR)
        {
            SubmitProductionJournalResponse SM = new SubmitProductionJournalResponse();
            try
            {

                if (SPJR.UserId == "" || SPJR.UserId == null)
                {
                    SM.Status = "Failure";
                    SM.Message = "Invalid UserId";
                    return SM;
                }
                if (SPJR.WorkOrderNo == "" || SPJR.WorkOrderNo == null)
                {
                    SM.Status = "Failure";
                    SM.Message = "Invalid WorkOrderId";
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

                var JournalIdPicking = obj.CreatePickingJournalHeader(Cct, SPJR.WorkOrderNo,"HHD");

               // var JournalIdRoute = obj.CreateRouteJournalHeader(Cct, SPJR.WorkOrderNo);

                List<PickingListData> PickingList = new List<PickingListData>();
                PickingList = SPJR.PickingList;
                foreach (var row in PickingList)
                {
                    PickingListData A = new PickingListData();
                    A.ItemId = row.ItemId;
                    A.Quantity = row.Quantity;
                    if(row.Shift=="")
                    {
                        A.Shift = "";
                    }
                    else
                    {
                        A.Shift = row.Shift;
                    }
                    A.Date = row.Date;
                    obj.CreatePickingJournalDetails(Cct, JournalIdPicking, A.ItemId, A.Quantity,A.Shift, Convert.ToDateTime(A.Date));
                }
                //obj.CreatePickingJournalDetailsFG(Cct, JournalIdPicking);
                //obj.PostPickingJournal(Cct, JournalIdPicking);
                obj.PostQCReject(Cct, JournalIdPicking);
                //List<RoutingCardListData> RoutingCardList = new List<RoutingCardListData>();
                //RoutingCardList = SPJR.RoutingCardList;
                //foreach (var row in RoutingCardList)
                //{
                //    RoutingCardListData A = new RoutingCardListData();
                //    A.OperationName = row.OperationName;
                //    A.Resource = row.Resource;
                //    A.ResourceType = row.ResourceType;
                //    A.Shift = row.Shift;
                //    A.OperationId = row.OperationId;
                //    A.Quantity = row.Quantity;
                //    A.Hours = row.Hours;
                //    A.Priority = row.Priority;
                //    obj.CreateRouteJournalDetails(Cct, JournalIdRoute, A.OperationId, A.Priority, A.Quantity, A.Hours, A.Shift);
                //}


                SM.Status = "Success";
                SM.Message = "Picking Journal Posted Successfully ";

                //if (ds.Rows[0]["value"].ToString() == "1")
                //{
                //    SM.Status = "Success";
                //    SM.Message = "SubmitProductionJournalProcess Successfully";
                //}
                //else
                //{
                //    SM.Status = "Failure";
                //    SM.Message = "SubmitProductionJournalProcess Failure";
                //}

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


        [Route("api/Nepro/SubmitRouteJournalData")]
        [HttpPost]
        public SubmitProductionJournalResponse SubmitRouteJournalData(SubmitProductionJournalRequest SPJR)
        {
            SubmitProductionJournalResponse SM = new SubmitProductionJournalResponse();
            try
            {

                if (SPJR.UserId == "" || SPJR.UserId == null)
                {
                    SM.Status = "Failure";
                    SM.Message = "Invalid UserId";
                    return SM;
                }
                if (SPJR.WorkOrderNo == "" || SPJR.WorkOrderNo == null)
                {
                    SM.Status = "Failure";
                    SM.Message = "Invalid WorkOrderId";
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

               // var JournalIdPicking = obj.CreatePickingJournalHeader(Cct, SPJR.WorkOrderNo);

                var JournalIdRoute = obj.CreateRouteJournalHeader(Cct, SPJR.WorkOrderNo);

                //List<PickingListData> PickingList = new List<PickingListData>();
                //PickingList = SPJR.PickingList;
                //foreach (var row in PickingList)
                //{
                //    PickingListData A = new PickingListData();
                //    A.ItemId = row.ItemId;
                //    A.Quantity = row.Quantity;
                //    obj.CreatePickingJournalDetails(Cct, JournalIdPicking, A.ItemId, A.Quantity);
                //}
                List<RoutingCardListData> RoutingCardList = new List<RoutingCardListData>();
                RoutingCardList = SPJR.RoutingCardList;
                foreach (var row in RoutingCardList)
                {
                    RoutingCardListData A = new RoutingCardListData();
                    A.OperationName = row.OperationName;
                    A.Resource = row.Resource;
                    A.ResourceType = row.ResourceType;
                    A.Shift = row.Shift;
                    A.OperationId = row.OperationId;
                    A.Quantity = row.Quantity;
                    A.Hours = row.Hours;
                    A.Priority = row.Priority;
                    obj.CreateRouteJournalDetails(Cct, JournalIdRoute, A.OperationId, A.Priority, A.Quantity, A.Hours, A.Shift);
                }
                obj.PostRouteJournal(Cct, JournalIdRoute);

                SM.Status = "Success";
                SM.Message = "Route Journal Successfully ";

                //if (ds.Rows[0]["value"].ToString() == "1")
                //{
                //    SM.Status = "Success";
                //    SM.Message = "SubmitProductionJournalProcess Successfully";
                //}
                //else
                //{
                //    SM.Status = "Failure";
                //    SM.Message = "SubmitProductionJournalProcess Failure";
                //}

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

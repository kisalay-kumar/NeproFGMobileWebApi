using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Web.Http;
using NeproWebApi.Models;
using Newtonsoft.Json;

namespace FRD_InventoryWebApi.Controllers
{
    public class ItemMovingController : ApiController
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Conn"].ConnectionString);
        string query = ""; SqlCommand dbcommand;
        //    ServiceReference1.CallContext Cct = new ServiceReference1.CallContext();
        //    ServiceReference1.AMY_FRDServiceClient obj = new ServiceReference1.AMY_FRDServiceClient();

        [Route("api/Nepro/StillageMoving")]
        [HttpPost]
        public ItemMovingResponse StillageMoving(MovingReq movingReq)
        {

            ItemMovingResponse res = new ItemMovingResponse();
            try
            {

                if (movingReq.StickerNo == "" || movingReq.StickerNo == null)
                {
                    res.Status = "Failure";
                    res.Message = "Enter StickerNo";
                    return res;
                }
                if (movingReq.UserId == "" || movingReq.UserId == null)
                {
                    res.Status = "Failure";
                    res.Message = "Invalid UserId";
                    return res;
                }


                query = "Sp_MovingWebApi";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.Connection.Open();
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.Parameters.AddWithValue("@QueryType", "GetMovingData");
                dbcommand.Parameters.AddWithValue("@StickerId", movingReq.StickerNo);
                dbcommand.Parameters.AddWithValue("@UserID", movingReq.UserId);
                dbcommand.CommandTimeout = 0;
                SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                DataSet ds = new DataSet();
                da.Fill(ds);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    if (ds.Tables[0].Rows[0]["value"].ToString() != "2")
                    {
                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        {
                            List<AisleList> AisleList = new List<AisleList>();
                            foreach (DataRow row in ds.Tables[1].Rows)
                            {
                                AisleList A = new AisleList();
                                A.name = row["name"].ToString();
                                A.id = row["id"].ToString();
                                AisleList.Add(A);
                            }
                            res.AisleList = AisleList;

                            //List<BinList> BinList = new List<BinList>();
                            //foreach (DataRow row in ds.Tables[3].Rows)
                            //{
                            //    BinList A = new BinList();
                            //    A.name = row["name"].ToString();
                            //    A.id = row["id"].ToString();
                            //    BinList.Add(A);
                            //}
                            //res.BinList = BinList;

                            //List<RackList> RackList = new List<RackList>();
                            //foreach (DataRow row in ds.Tables[2].Rows)
                            //{
                            //    RackList A = new RackList();
                            //    A.name = row["name"].ToString();
                            //    A.id = row["id"].ToString();
                            //    RackList.Add(A);
                            //}
                            //res.RackList = RackList;

                            List<ZoneList> ZoneList = new List<ZoneList>();
                            foreach (DataRow row in ds.Tables[2].Rows)
                            {
                                ZoneList A = new ZoneList();
                                A.name = row["name"].ToString();
                                A.id = row["id"].ToString();
                                ZoneList.Add(A);
                            }
                            res.ZoneList = ZoneList;
                            if (ds != null && ds.Tables[3].Rows.Count > 0)
                            {
                                res.AssignedAisleId = Convert.ToString(ds.Tables[3].Rows[0]["AssignedAisleId"]);
                                res.AssignedBinId = Convert.ToString(ds.Tables[3].Rows[0]["AssignedBinId"]);
                                res.AssignedRackId = Convert.ToString(ds.Tables[3].Rows[0]["AssignedRackId"]);
                                res.AssignedAisleName = Convert.ToString(ds.Tables[3].Rows[0]["AssignedAisleName"]);
                                res.AssignedBinName = Convert.ToString(ds.Tables[3].Rows[0]["AssignedBinName"]);
                                res.AssignedRackName = Convert.ToString(ds.Tables[3].Rows[0]["AssignedRackName"]);
                                res.AssignedZoneName = Convert.ToString(ds.Tables[3].Rows[0]["ZoneName"]);
                                res.AssignedZoneId = Convert.ToString(ds.Tables[3].Rows[0]["ZONEID"]);
                            }
                            else
                            {
                                res.AssignedAisleId = "";
                                res.AssignedBinId = "";
                                res.AssignedRackId = "";
                            }

                            res.WareHouseID = ds.Tables[0].Rows[0]["WareHouseID"].ToString();
                            res.StickerID = ds.Tables[0].Rows[0]["StickerID"].ToString();
                            res.StandardQty = Convert.ToDecimal(ds.Tables[0].Rows[0]["StandardQty"]);
                            res.ItemId = ds.Tables[0].Rows[0]["ItemId"].ToString();
                            res.Description = ds.Tables[0].Rows[0]["Description"].ToString();
                            res.ItemStdQty = Convert.ToDecimal(ds.Tables[0].Rows[0]["ItemStdQty"]);
                            res.AssignedLocation = ds.Tables[0].Rows[0]["AssignedLocation"].ToString();
                            res.IsMovedFromProdLine = Convert.ToByte(ds.Tables[0].Rows[0]["IsMovedFromProdLine"]);
                            res.StillageLocationName = Convert.ToString(ds.Tables[0].Rows[0]["StillageLocationName"]);
                            res.IsAssignTransfer = Convert.ToByte(ds.Tables[0].Rows[0]["IsAssignTransfer"]);
                            if (res.StillageLocationName == "Production Line")
                            {
                                res.LoadingAreaId = res.LoadingAreaId = Convert.ToString(ds.Tables[4].Rows[0]["ZONEID"]);
                            }
                            else
                            {
                                res.LoadingAreaId = "";
                            }




                            res.Status = "Success";
                            res.Message = "Data retrived successfully";

                        }
                        else
                        {
                            res.Status = "Failure";
                            res.Message = "Invalid Sticker Number.";
                        }



                    }
                    else
                    {
                        res.Status = "Failure";
                        res.Message = "This Stillage Does not Exist";
                        return res;
                    }
                }
                else
                {
                    res.Status = "Failure";
                    res.Message = "This stillage is assigned to another user" ;
                }
            }
            catch (Exception ex)
            {
                res.Status = "Failure";
                res.Message = ex.Message;

            }
            finally
            {
                dbcommand.Connection.Close();
            }
            return res;
        }
        [Route("api/Nepro/UpdateMovingStatus")]
        [HttpPost]
        public ReceivingResponse Receiving(ConfirmMovingReq CMR)
        {

            ReceivingResponse res = new ReceivingResponse();

            try
            {
                if (CMR.UserId == "" || CMR.UserId == null)
                {
                    res.Status = "Failure";
                    res.Message = "Object reference not set to an instance of an object.";
                }
                if (CMR.StickerNo == "" || CMR.StickerNo == null)
                {
                    res.Status = "Failure";
                    res.Message = "Invalid Sticker No.";
                }


                query = "Sp_MovingWebApi";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.CommandTimeout = 0;
                dbcommand.Parameters.AddWithValue("@QueryType", "FetchDataMoving");
                dbcommand.Parameters.AddWithValue("@StillageID", CMR.StickerNo);
                dbcommand.Parameters.AddWithValue("@UserId", CMR.UserId);
                dbcommand.Parameters.AddWithValue("@Aisle", CMR.Aisle);
                dbcommand.Parameters.AddWithValue("@Bin", CMR.Bin);
                dbcommand.Parameters.AddWithValue("@Rack", CMR.Rack);
                dbcommand.Parameters.AddWithValue("@Zone", CMR.Zone);
                dbcommand.Parameters.AddWithValue("@LoadingAreaId", CMR.LoadingAreaId);
                SqlDataAdapter daGetData = new SqlDataAdapter(dbcommand);
                DataSet dsGetData = new DataSet();
                daGetData.Fill(dsGetData);
                SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                var Hold = "";
                if (dsGetData.Tables[0].Rows[0]["isHold"].ToString() == "True")
                {
                    Hold = "QC Hold";
                }
                else
                {
                    Hold = "Qc Release";

                }



                if (dsGetData.Tables[0].Rows[0].ToString() != "2")
                {
                    query = "Sp_AxWebserviceIntegration";
                    dbcommand = new SqlCommand(query, conn);
                    //  dbcommand.Connection.Open();
                    dbcommand.CommandType = CommandType.StoredProcedure;
                    dbcommand.CommandTimeout = 0;
                    da = new SqlDataAdapter(dbcommand);
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    String StillageLocation = "";
                    if (CMR.LoadingAreaId == "")
                    {
                        StillageLocation = Convert.ToString(dsGetData.Tables[2].Rows[0]["ZONEID"]);

                    }
                    else
                    {
                        StillageLocation = CMR.LoadingAreaId;
                    }

                    //  DataSet ds = CommonManger.FillDatasetWithParam("Sp_AxWebserviceIntegration");
                    NeproWebApi.AXWebServiceRef1.Iace_FinishedGoodServiceClient obj = new NeproWebApi.AXWebServiceRef1.Iace_FinishedGoodServiceClient();
                    obj.ClientCredentials.Windows.ClientCredential.Domain = Convert.ToString(ds.Tables[0].Rows[0]["Domain"]);
                    obj.ClientCredentials.Windows.ClientCredential.UserName = Convert.ToString(ds.Tables[0].Rows[0]["Username"]);
                    obj.ClientCredentials.Windows.ClientCredential.Password = Convert.ToString(ds.Tables[0].Rows[0]["Password"]);

                    NeproWebApi.AXWebServiceRef1.CallContext Cct = new NeproWebApi.AXWebServiceRef1.CallContext();
                    Cct.Company = Convert.ToString(ds.Tables[0].Rows[0]["Company"]);
                    Cct.Language = Convert.ToString(ds.Tables[0].Rows[0]["Language"]);
                    string value = obj.InsertHistoryHeaderData(Cct, CMR.StickerNo, Convert.ToString(dsGetData.Tables[0].Rows[0]["SiteID"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ItemId"]), Convert.ToDecimal(dsGetData.Tables[0].Rows[0]["WorkOrderQty"]));
                    obj.InsertHistoryDetailData(Cct, CMR.StickerNo, "", Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityName"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityDesc"]), StillageLocation, Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]), "", StillageLocation, "", "", "", "No", 0, "", 0, 0, Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["StillageQty"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]), Hold, 0, "","","", Convert.ToString(dsGetData.Tables[1].Rows[0]["WareHouseID"]),0);
                    obj.UpdateStillageQty(Cct, CMR.StickerNo, StillageLocation, Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["StillageQty"]));
                }
                else
                {
                    res.Status = "Failure";
                    res.Message = "This Stillage Does Not Exist";
                    return res;

                }
                query = "Sp_MovingWebApi";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.Connection.Open();
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.Parameters.AddWithValue("@QueryType", "UpdateMovingData");
                dbcommand.Parameters.AddWithValue("@StickerId", CMR.StickerNo);
                dbcommand.Parameters.AddWithValue("@Aisle", CMR.Aisle);
                dbcommand.Parameters.AddWithValue("@Rack", CMR.Rack);
                dbcommand.Parameters.AddWithValue("@Bin", CMR.Bin);
                dbcommand.Parameters.AddWithValue("@UserId", CMR.UserId);
                dbcommand.Parameters.AddWithValue("@WareHouseID", CMR.WareHouseID);
                dbcommand.Parameters.AddWithValue("@Zone", CMR.Zone);
                dbcommand.Parameters.AddWithValue("@LoadingAreaId", CMR.LoadingAreaId);

                dbcommand.CommandTimeout = 0;
                da = new SqlDataAdapter(dbcommand);
                DataTable dt = new DataTable();
                da.Fill(dt);
                if (dt.Rows[0]["value"].ToString() == "1")
                {
                    res.Status = "Success";
                    res.Message = "Stillage moved successfully";
                }
                else if (dt.Rows[0]["value"].ToString() == "2")
                {
                    res.Status = "Failure";
                    res.Message = "Invalid Sticker Id";
                }
                else
                {
                    res.Status = "Failure";
                    res.Message = "Stillage not Move successfully";
                }

            }
            catch (Exception Ex)
            {
                res.Status = "Failure";
                res.Message = Ex.Message;
            }
            //finally
            //{
            //    dbcommand.Connection.Close();
            //}
            return res;
        }

        [Route("api/Nepro/MovingStillageList")]
        [HttpPost]
        public MovingData StickerList(MovingReq Req)
        {
            MovingData SM = new MovingData();

            try
            {
                if (Req.UserId == "" || Req.UserId == null)
                {
                    SM.Status = "Failure";
                    SM.Message = "Invalid UserId";
                }
                else
                {
                    query = "Sp_MovingWebApi";
                    dbcommand = new SqlCommand(query, conn);
                    dbcommand.Connection.Open();
                    dbcommand.CommandType = CommandType.StoredProcedure;
                    dbcommand.Parameters.AddWithValue("@QueryType", "GetMovingList");
                    dbcommand.Parameters.AddWithValue("@StickerId", Req.StickerNo);
                    dbcommand.Parameters.AddWithValue("@UserId", Req.UserId);
                    SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows[0]["value"].ToString() != "2")
                    {

                        if (dt != null && dt.Rows.Count > 0)
                        {
                            List<MovingListData> StillageSticker = new List<MovingListData>();
                            foreach (DataRow row in dt.Rows)
                            {
                                MovingListData SS = new MovingListData();
                                SS.StickerID = row["StickerID"].ToString();
                                SS.StandardQty = Convert.ToDecimal(row["StandardQty"]);
                                SS.ItemId = row["ItemId"].ToString();
                                SS.Description = row["Description"].ToString();
                                SS.ItemStdQty = Convert.ToDecimal(row["ItemStdQty"]);
                                SS.AssignedLocation = row["AssignedLocation"].ToString();
                                SS.IsMovedFromProdLine = Convert.ToByte(row["IsMovedFromProdLine"]);
                                StillageSticker.Add(SS);
                            }

                            SM.Status = "Success";
                            SM.Message = "Data retrived successfully";
                            SM.StillageList = StillageSticker;
                        }
                        else
                        {
                            SM.Status = "Failure";
                            SM.Message = "No Stillage is assigned to this user.";
                            return SM;
                        }
                    }
                    else
                    {
                        SM.Status = "Failure";
                        SM.Message = "This Stillage Does Not Exist";
                        return SM;
                    }
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


        [Route("api/Nepro/GetLocationDetails")]
        [HttpPost]
        public LocationDetail GetLocationFromLocID(Location Loc)
        {
            LocationDetail LD = new LocationDetail();

            try
            {
                if (Loc.LocationId == "")
                {
                    LD.Status = "Failure";
                    LD.Message = "Invalid Location Id";
                }
                else
                {
                    query = "Sp_MovingWebApi";
                    dbcommand = new SqlCommand(query, conn);
                    dbcommand.Connection.Open();
                    dbcommand.CommandType = CommandType.StoredProcedure;
                    dbcommand.Parameters.AddWithValue("@QueryType", "GetLocationDetail");
                    dbcommand.Parameters.AddWithValue("@LocationID", Loc.LocationId);
                    dbcommand.Parameters.AddWithValue("@WareHouseID", Loc.WareHouseId);
                    dbcommand.Parameters.AddWithValue("@UserId", Loc.UserId);


                    SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    if (dt != null && dt.Rows.Count > 0)
                    {

                        if (Convert.ToInt16(dt.Rows[0]["Result"]) == 1)
                        {
                            LD.Status = "Failure";
                            LD.Message = "Invalid Location Id";
                        }
                        else
                        {
                            LD.Status = "Success";
                            LD.Message = "Data retrived successfully";
                            LD.Aisle = (dt.Rows[0]["Aisle"]).ToString();
                            LD.Rack = (dt.Rows[0]["Rack"]).ToString();
                            LD.Bin = (dt.Rows[0]["Bin"]).ToString();
                            LD.AisleName = (dt.Rows[0]["AisleName"]).ToString();
                            LD.RackName = (dt.Rows[0]["RackName"]).ToString();
                            LD.BinName = (dt.Rows[0]["BinName"]).ToString();
                            LD.Zone = (dt.Rows[0]["Zone"]).ToString();
                            LD.ZoneName = (dt.Rows[0]["ZoneName"]).ToString();
                        }
                    }
                    else
                    {
                        LD.Status = "Failure";
                        LD.Message = "Location not found";
                    }
                }
            }
            catch (Exception Ex)
            {
                LD.Status = "Failure";
                LD.Message = Ex.Message;
            }
            finally
            {
                dbcommand.Connection.Close();
            }
            return LD;
        }


        [Route("api/Nepro/GetAssigningData")]
        [HttpPost]
        public AssigningResponse AssignData(AssigningReq CMR)
        {

            AssigningResponse res = new AssigningResponse();

            try
            {
                if (CMR == null)
                {
                    res.Status = "Failure";
                    res.Message = "Object reference not set to an instance of an object.";
                }
                else if (CMR.StickerNo == "")
                {
                    res.Status = "Failure";
                    res.Message = "Invalid Sticker No.";
                }
                else
                {
                    query = "Sp_AssignWebApi";
                    dbcommand = new SqlCommand(query, conn);
                    dbcommand.Connection.Open();
                    dbcommand.CommandType = CommandType.StoredProcedure;
                    dbcommand.Parameters.AddWithValue("@QueryType", "GetAssignedData");
                    dbcommand.Parameters.AddWithValue("@StickerId", CMR.StickerNo);
                    dbcommand.Parameters.AddWithValue("@UserId", CMR.UserId);
                    dbcommand.CommandTimeout = 0;
                    SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                    DataSet ds = new DataSet();
                    da.Fill(ds);
                    if (ds.Tables[0].Rows[0]["value"].ToString() != "3")
                    {
                        res.StillageLocation = Convert.ToString(ds.Tables[0].Rows[0]["StillageLocation"]);
                        if (ds != null && ds.Tables[0].Rows.Count > 0)
                        {
                            if (ds.Tables[0].Rows[0]["value"].ToString() == "1")
                            {
                                res.Status = "Failure";
                                res.Message = "Don't have access to get the details";
                            }

                            //else if (res.StillageLocation != "NPRO-000001")
                            else
                            {
                                res.StickerID = ds.Tables[0].Rows[0]["StickerID"].ToString();
                                res.StandardQty = Convert.ToDecimal(ds.Tables[0].Rows[0]["StandardQty"]);
                                res.ItemId = ds.Tables[0].Rows[0]["ItemId"].ToString();
                                res.Description = ds.Tables[0].Rows[0]["Description"].ToString();
                                res.ItemStdQty = Convert.ToDecimal(ds.Tables[0].Rows[0]["ItemStdQty"]);
                                res.WareHouseID = Convert.ToString(ds.Tables[0].Rows[0]["WareHouseID"]);
                                res.IsAssignTransfer = Convert.ToByte(ds.Tables[0].Rows[0]["IsAssignTransfer"]);

                                List<AisleList> AisleList = new List<AisleList>();
                                foreach (DataRow row in ds.Tables[1].Rows)
                                {
                                    AisleList A = new AisleList();
                                    A.name = row["name"].ToString();
                                    A.id = row["id"].ToString();
                                    AisleList.Add(A);
                                }
                                res.AisleList = AisleList;

                                //List<BinList> BinList = new List<BinList>();
                                //foreach (DataRow row in ds.Tables[3].Rows)
                                //{
                                //    BinList A = new BinList();
                                //    A.name = row["name"].ToString();
                                //    A.id = row["id"].ToString();
                                //    BinList.Add(A);
                                //}
                                //res.BinList = BinList;

                                //List<RackList> RackList = new List<RackList>();
                                //foreach (DataRow row in ds.Tables[2].Rows)
                                //{
                                //    RackList A = new RackList();
                                //    A.name = row["name"].ToString();
                                //    A.id = row["id"].ToString();
                                //    RackList.Add(A);
                                //}
                                //res.RackList = RackList;
                                List<ZoneList> ZoneList = new List<ZoneList>();
                                foreach (DataRow row in ds.Tables[2].Rows)
                                {
                                    ZoneList A = new ZoneList();
                                    A.name = row["name"].ToString();
                                    A.id = row["id"].ToString();
                                    ZoneList.Add(A);
                                }
                                res.ZoneList = ZoneList;

                                List<FLTList> FLTList = new List<FLTList>();
                                foreach (DataRow row in ds.Tables[3].Rows)
                                {
                                    FLTList A = new FLTList();
                                    A.name = row["name"].ToString();
                                    A.id =Convert.ToInt64(row["id"]);
                                    FLTList.Add(A);
                                }
                                res.FLTList = FLTList;



                                res.Status = "Success";
                                res.Message = "Data retrived successfully";
                            }
                            //else
                            //{
                            //    res.Status = "Operation Invalid";
                            //    res.Message = "Stillage will only move to Loading Area.";

                            //}
                        }
                        else
                        {
                            res.Status = "Failure";
                            res.Message = "Invalid Sticker Id";

                        }
                    }
                    else
                    {
                        res.Status = "Failure";
                        res.Message = "This stillage does not exist";
                        return res;
                    }
                }
            }
            catch (Exception Ex)
            {
                res.Status = "Failure";
                res.Message = Ex.Message;
            }
            finally
            {
                dbcommand.Connection.Close();
            }
            return res;
        }



        [Route("api/Nepro/GetRack")]
        [HttpPost]
        public AssigningResponse RackData(AssigningReq CMR)
        {

            AssigningResponse res = new AssigningResponse();

            try
            {
                if (CMR == null)
                {
                    res.Status = "Failure";
                    res.Message = "Object reference not set to an instance of an object.";
                }
                else
                {
                    query = "Sp_AssignWebApi";
                    dbcommand = new SqlCommand(query, conn);
                    dbcommand.Connection.Open();
                    dbcommand.CommandType = CommandType.StoredProcedure;
                    dbcommand.Parameters.AddWithValue("@QueryType", "GetRackData");
                    dbcommand.Parameters.AddWithValue("@WareHouseID", CMR.WareHouseID);
                    dbcommand.Parameters.AddWithValue("@aisle", CMR.Aisle);

                    dbcommand.CommandTimeout = 0;
                    SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                    DataSet ds = new DataSet();
                    da.Fill(ds);

                    List<RackList> RackList = new List<RackList>();
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        RackList A = new RackList();
                        A.name = row["name"].ToString();
                        A.id = row["id"].ToString();
                        RackList.Add(A);
                    }
                    res.RackList = RackList;
                    res.Status = "Success";
                    res.Message = "Data retrived successfully";

                }
            }
            catch (Exception Ex)
            {
                res.Status = "Failure";
                res.Message = Ex.Message;
            }
            finally
            {
                dbcommand.Connection.Close();
            }
            return res;
        }







        [Route("api/Nepro/GetBin")]
        [HttpPost]
        public AssigningResponse BinData(AssigningReq CMR)
        {

            AssigningResponse res = new AssigningResponse();

            try
            {
                if (CMR == null)
                {
                    res.Status = "Failure";
                    res.Message = "Object reference not set to an instance of an object.";
                }
                else
                {
                    query = "Sp_AssignWebApi";
                    dbcommand = new SqlCommand(query, conn);
                    dbcommand.Connection.Open();
                    dbcommand.CommandType = CommandType.StoredProcedure;
                    dbcommand.Parameters.AddWithValue("@QueryType", "GetBinData");
                    dbcommand.Parameters.AddWithValue("@WareHouseID", CMR.WareHouseID);
                    dbcommand.Parameters.AddWithValue("@aisle", CMR.Aisle);
                    dbcommand.Parameters.AddWithValue("@rack", CMR.Rack);

                    dbcommand.CommandTimeout = 0;
                    SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                    DataSet ds = new DataSet();
                    da.Fill(ds);

                    List<BinList> BinList = new List<BinList>();
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        BinList A = new BinList();
                        A.name = row["name"].ToString();
                        A.id = row["id"].ToString();
                        BinList.Add(A);
                    }
                    res.BinList = BinList;
                    res.Status = "Success";
                    res.Message = "Data retrived successfully";

                }
            }
            catch (Exception Ex)
            {
                res.Status = "Failure";
                res.Message = Ex.Message;
            }
            finally
            {
                dbcommand.Connection.Close();
            }
            return res;
        }












        [Route("api/Nepro/UpdateAssigningData")]
        [HttpPost]
        public ReceivingResponse UpdateAssignation(AssigningReq AR)
        {

            ReceivingResponse res = new ReceivingResponse();

            try
            {
                if (AR == null)
                {
                    res.Status = "Failure";
                    res.Message = "Object reference not set to an instance of an object.";
                }
                if (AR.StickerNo == "")
                {
                    res.Status = "Failure";
                    res.Message = "Invalid Sticker No.";
                }
                else
                {

                    query = "Sp_AssignWebApi";
                    dbcommand = new SqlCommand(query, conn);
                    dbcommand.CommandType = CommandType.StoredProcedure;
                    dbcommand.CommandTimeout = 0;
                    dbcommand.Parameters.AddWithValue("@QueryType", "FetchDataAssigning");
                    dbcommand.Parameters.AddWithValue("@StillageID", AR.StickerNo);
                    dbcommand.Parameters.AddWithValue("@UserId", AR.UserId);
                    dbcommand.Parameters.AddWithValue("@Aisle", AR.Aisle);
                    dbcommand.Parameters.AddWithValue("@Bin", AR.Bin);
                    dbcommand.Parameters.AddWithValue("@Rack", AR.Rack);
                    dbcommand.Parameters.AddWithValue("@Zone", AR.Zone);
                    dbcommand.Parameters.AddWithValue("@AssignedUser", AR.AssignedFLT);
                    dbcommand.Parameters.AddWithValue("@WareHouseID", AR.WareHouseID);
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
                        NeproWebApi.AXWebServiceRef1.Iace_FinishedGoodServiceClient obj = new NeproWebApi.AXWebServiceRef1.Iace_FinishedGoodServiceClient();
                        obj.ClientCredentials.Windows.ClientCredential.Domain = Convert.ToString(ds.Tables[0].Rows[0]["Domain"]);
                        obj.ClientCredentials.Windows.ClientCredential.UserName = Convert.ToString(ds.Tables[0].Rows[0]["Username"]);
                        obj.ClientCredentials.Windows.ClientCredential.Password = Convert.ToString(ds.Tables[0].Rows[0]["Password"]);

                        NeproWebApi.AXWebServiceRef1.CallContext Cct = new NeproWebApi.AXWebServiceRef1.CallContext();
                        Cct.Company = Convert.ToString(ds.Tables[0].Rows[0]["Company"]);
                        Cct.Language = Convert.ToString(ds.Tables[0].Rows[0]["Language"]);
                        string value = obj.InsertHistoryHeaderData(Cct, AR.StickerNo, Convert.ToString(dsGetData.Tables[0].Rows[0]["SiteID"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["WorkOrderNo"]), Convert.ToString(dsGetData.Tables[0].Rows[0]["ItemId"]), Convert.ToDecimal(dsGetData.Tables[0].Rows[0]["WorkOrderQty"]));
                        //obj.InsertHistoryDetailData(Cct, AR .StickerNo, "", Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityName"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityDesc"]), Convert.ToString(dsGetData.Tables[2].Rows[0]["ZONEID"]), "", Convert.ToString(dsGetData.Tables[1].Rows[0]["AssignedFLT"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), "", "", "", "No", 0, "", 0, 0, Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["StillageQty"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]), "", 0, "");

                        obj.InsertHistoryDetailData(Cct, AR.StickerNo, "", Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityName"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["ActivityDesc"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["StillageLocation"]), "", Convert.ToString(dsGetData.Tables[1].Rows[0]["AssignedFLT"]), "", Convert.ToString(dsGetData.Tables[2].Rows[0]["ZONEID"]), "", "", "No", 0, "", 0, 0, Convert.ToDecimal(dsGetData.Tables[1].Rows[0]["StillageQty"]), Convert.ToString(dsGetData.Tables[1].Rows[0]["UserName"]), Hold, 0, "","","", Convert.ToString(dsGetData.Tables[1].Rows[0]["WareHouseID"]),0);
                    }
                    else
                    {
                        res.Status = "Failure";
                        res.Message = "This Stillage Does Not Exist";
                        return res;
                    }




                    query = "[Sp_AssignWebApi]";
                    dbcommand = new SqlCommand(query, conn);
                    dbcommand.Connection.Open();
                    dbcommand.CommandType = CommandType.StoredProcedure;
                    dbcommand.Parameters.AddWithValue("@QueryType", "UpdateAssign");
                    dbcommand.Parameters.AddWithValue("@StickerId", AR.StickerNo);
                    dbcommand.Parameters.AddWithValue("@Aisle", AR.Aisle);
                    dbcommand.Parameters.AddWithValue("@Rack", AR.Rack);
                    dbcommand.Parameters.AddWithValue("@Bin", AR.Bin);
                    dbcommand.Parameters.AddWithValue("@UserId", AR.UserId);
                    dbcommand.Parameters.AddWithValue("@AssignedUser", AR.AssignedFLT);
                    dbcommand.Parameters.AddWithValue("@WareHouseID", AR.WareHouseID);
                    dbcommand.Parameters.AddWithValue("@Zone", AR.Zone);

                    dbcommand.CommandTimeout = 0;
                    da = new SqlDataAdapter(dbcommand);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    if (dt.Rows[0]["value"].ToString() == "1")
                    {
                        res.Status = "Success";
                        res.Message = "Stillage assigned successfully";
                    }
                    else
                    {
                        res.Status = "Failure";
                        res.Message = "Stillage not successfully assigned.!";
                    }
                }
            }
            catch (Exception Ex)
            {
                res.Status = "Failure";
                res.Message = Ex.Message;
            }
            finally
            {
                dbcommand.Connection.Close();
            }
            return res;
        }
    }

    public class Location
    {
        public string LocationId { get; set; }
        public Int64 UserId { get; set; }
        public string WareHouseId { get; set; }
    }
    public class LocationDetail
    {
        public string Aisle { get; set; }
        public string Rack { get; set; }
        public string Bin { get; set; }
        public string AisleName { get; set; }
        public string RackName { get; set; }
        public string BinName { get; set; }
        public string Zone { get; set; }
        public string ZoneName { get; set; }
        
        public string Status { get; set; }
        public string Message { get; set; }
    }
    public class MovingReq
    {
        public string StickerNo { get; set; }
        public string UserId { get; set; }
    }

    public class ConfirmMovingReq
    {
        public string StickerNo { get; set; }
        public string Aisle { get; set; }
        public string Rack { get; set; }
        public string Bin { get; set; }
        public string UserId { get; set; }
        public string WareHouseID { get; set; }
        public string LoadingAreaId { get; set; }
        public string Zone { get; set; }

    }

    public class AssigningReq
    {
        public string StickerNo { get; set; }
        public string Aisle { get; set; }
        public string Rack { get; set; }
        public string Bin { get; set; }
        public string Zone { get; set; }
        public Int64 UserId { get; set; }
        public Int64 AssignedFLT { get; set; }
        public string WareHouseID { get; set; }

    }

    public class ItemMovingResponse
    {
        public List<AisleList> AisleList { get; set; }
        public List<BinList> BinList { get; set; }
        public List<RackList> RackList { get; set; }
        public List<ZoneList> ZoneList { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string StickerID { get; set; }
        public decimal StandardQty { get; set; }
        public string ItemId { get; set; }
        public string Description { get; set; }
        public decimal ItemStdQty { get; set; }
        public string AssignedLocation { get; set; }
        public byte IsMovedFromProdLine { get; set; }
        public byte IsAssignTransfer { get; set; }
        public string StillageLocationName { get; set; }
        public string AssignedAisleId { get; set; }
        public string AssignedRackId { get; set; }
        public string AssignedBinId { get; set; }
        public string AssignedZoneId { get; set; }
        public string AssignedZoneName { get; set; }
        public string AssignedAisleName { get; set; }
        public string AssignedRackName { get; set; }
        public string AssignedBinName { get; set; }
        public string LoadingAreaId { get; set; }
        public string WareHouseID { get; set; }
    }

    public class MovingData
    {
        public List<MovingListData> StillageList { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }
    public class MovingListData
    {
        public string StickerID { get; set; }
        public decimal StandardQty { get; set; }
        public string ItemId { get; set; }
        public string Description { get; set; }
        public decimal ItemStdQty { get; set; }
        public string AssignedLocation { get; set; }
        public byte IsMovedFromProdLine { get; set; }
    }


    public class ReceivingRequest
    {
        public string RequisitionNo { get; set; }


        public List<ReceivingRequestItem> ItemList
        { get; set; }


    }
    public class ReceivingRequestItem
    {

        public string ItemId { get; set; }
        public string ReceivingQty { get; set; }
        public string Reason { get; set; }
        public string TONum { get; set; }
        public string Location { get; set; }
        public string Configuration { get; set; }
    }

    public class ReceivingResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
    }

    public class AssigningResponse
    {
        public List<AisleList> AisleList { get; set; }
        public List<BinList> BinList { get; set; }
        public List<RackList> RackList { get; set; }
        public List<ZoneList> ZoneList { get; set; }
        public List<FLTList> FLTList { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string StickerID { get; set; }
        public decimal StandardQty { get; set; }
        public string ItemId { get; set; }
        public string Description { get; set; }
        public decimal ItemStdQty { get; set; }
        public string WareHouseID { get; set; }
        public string StillageLocation { get; set; }
        public Byte IsAssignTransfer { get; set; }
    }
  
    public class AisleList
    {
        public string name { get; set; }
        public string id { get; set; }
    }
    public class BinList
    {
        public string name { get; set; }
        public string id { get; set; }
    }
    public class RackList
    {
        public string name { get; set; }
        public string id { get; set; }
    }
    public class ZoneList
    {
        public string name { get; set; }
        public string id { get; set; }
    }
}

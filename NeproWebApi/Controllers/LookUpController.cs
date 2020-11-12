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
    public class LookUpController : ApiController
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Conn"].ConnectionString);
        string query = ""; SqlCommand dbcommand;


        [Route("api/Nepro/ScanLookUpStillage")]
        [HttpPost]
        public ScanMergeResponse ScanLookUpStillage(StickerReq SR)
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
                 
              
                    query = "Sp_MasterDataWebApi";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.Connection.Open();
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.Parameters.AddWithValue("@QueryType", "GetScanStillageLookUp");
                dbcommand.Parameters.AddWithValue("@StickerId", SR.StickerNo);
                dbcommand.Parameters.AddWithValue("@UserId", SR.UserId);


                dbcommand.CommandTimeout = 0;
                SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                DataSet dt = new DataSet();
                da.Fill(dt);
                if (dt != null && dt.Tables[0].Rows.Count > 0)
                {
                    SS.StickerID = dt.Tables[0].Rows[0]["StickerID"].ToString();
                    SS.StandardQty = Convert.ToDecimal(dt.Tables[0].Rows[0]["StandardQty"]);
                    SS.ItemId = dt.Tables[0].Rows[0]["ItemId"].ToString();
                    SS.Description = dt.Tables[0].Rows[0]["Description"].ToString();
                    SS.ItemStdQty = Convert.ToDecimal(dt.Tables[0].Rows[0]["ItemStdQty"]);
                    SS.Location = dt.Tables[0].Rows[0]["Location"].ToString();
                    SS.IsTransfered =Convert.ToByte(dt.Tables[0].Rows[0]["IsTransfered"]);
                    SS.WareHouseID = dt.Tables[3].Rows[0]["WareHouseID"].ToString();
                    SS.WareHouseName = dt.Tables[0].Rows[0]["WareHouseName"].ToString();
                    SS.TransferId = dt.Tables[0].Rows[0]["TransferId"].ToString();
                    SS.IsShiped = dt.Tables[0].Rows[0]["IsShiped"].ToString();
                    SS.isHold= Convert.ToByte(dt.Tables[0].Rows[0]["isHold"]);
                    SS.IsCounted = Convert.ToByte(dt.Tables[0].Rows[0]["IsCounted"]);
                    SS.ToBeTransferWHID= dt.Tables[0].Rows[0]["ToBeTransferWHID"].ToString();
                  
                    SS.STRP =Convert.ToInt16(dt.Tables[0].Rows[0]["STRP"]);
                    SS.UOM = Convert.ToString(dt.Tables[0].Rows[0]["UOM"]);
                    if (DBNull.Value.Equals(dt.Tables[2].Rows[0]["Prodstatus"]) )
                    {
                        SS.Prodstatus = 0;
                        SS.WoStatus = "N/A";
                    }
                    else
                    {
                        SS.Prodstatus = Convert.ToInt16(dt.Tables[2].Rows[0]["Prodstatus"]);
                        SS.WoStatus = dt.Tables[2].Rows[0]["WorkorderStatus"].ToString();
                    }


                    List<SiteList> SiteList = new List<SiteList>();
                    foreach (DataRow row in dt.Tables[1].Rows)
                    {
                        SiteList A = new SiteList();
                        A.id = row["id"].ToString();
                        A.name = row["name"].ToString();
                        SiteList.Add(A);
                    }



                    List<ReasonList> ReasonList = new List<ReasonList>();
                    foreach (DataRow row in dt.Tables[4].Rows)
                    {
                        ReasonList A = new ReasonList();
                        A.id = Convert.ToString(row["id"]);
                        A.name = row["name"].ToString();

                        ReasonList.Add(A);
                    }
                    SS.ReasonList = ReasonList;



                    SS.SiteListData = SiteList;
                    SS.Status = "Success";
                    SS.Message = "Data retrived successfully";
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
    }
}

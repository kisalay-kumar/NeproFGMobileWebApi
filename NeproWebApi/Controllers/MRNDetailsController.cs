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
//using FRD_InventoryWebApi.ServiceReference1;
using NeproWebApi.Models;
using System.Text.RegularExpressions;

namespace FRD_InventoryWebApi.Controllers
{
    public class MRNDetailsController : ApiController
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Conn"].ConnectionString);
        string query = ""; SqlCommand dbcommand;

        [Route("api/FRD/GetMrn")]
        [HttpPost]
        public GetMRN GetMrn()
        {

            GetMRN res = new GetMRN();
            List<GetMRNs> ListView = new List<GetMRNs>();
            try
            {


                query = "SP_Login";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.Connection.Open();
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.Parameters.AddWithValue("@QueryType", "GetMrn");

                SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                DataTable dt = new DataTable();
                da.Fill(dt);
                foreach (DataRow row in dt.Rows)
                {
                    GetMRNs m = new GetMRNs();
                    m.MRNNumber = row["MRNNumber"].ToString();
                    m.MRNDate = row["MRNDate"].ToString();
                    m.ActivityNumber = row["ActivityNumber"].ToString();

                    ListView.Add(m);
                }
                res.MRNList = ListView;
                res.Status = "Success";
                res.Message = "Data retrived successfully";



            }
            catch (Exception ex)
            {
                res.Status = "Failure";
                res.Message = "Data did not retrived successfully";
                
            }
            finally
            {
                dbcommand.Connection.Close();
            }
            return res;
        }

        [Route("api/FRD/GetMrnDetails")]
        [HttpPost]
        public GetMrnDeta GetMrnDetails(MRNRequest MR)
        {
            GetMrnDeta res = new GetMrnDeta();
            List<MRNDetails> ListView = new List<MRNDetails>();
            
            try
            {

                query = "InsertMRN";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.Connection.Open();
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.Parameters.AddWithValue("@QueryType", "APIGetMRNDetails");
                dbcommand.Parameters.AddWithValue("@MRNNumber", MR.MNRNumber);
                dbcommand.Parameters.AddWithValue("@userId", MR.UserId);
                SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                DataSet ds = new DataSet();
                da.Fill(ds);
                if (ds.Tables[0].Rows[0]["BatchNo"].ToString() == "-1")
                {
                    res.Status = "Failure";
                    res.Message = "This MRN Open By Another User";
                }
                else { 
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    MRNDetails bnk = new MRNDetails();
                    List<StickerSeq> listStickerSeq = new List<StickerSeq>();
                    bnk.ItemId = row["ItemId"].ToString();
                    bnk.BatchNo = row["BatchNo"].ToString();
                    bnk.ReceivedQuantity = row["BatchQuantity"].ToString();
                    bnk.Config = row["CONFIGID"].ToString();
                    bnk.itemArabicName = row["itemArabicName"].ToString();
                    bnk.itemname = row["itemname"].ToString();
                    bnk.ExpiryDate = row["ExpiryDate"].ToString();
                    bnk.LineNo = row["ItemLineNumber"].ToString();

            
                    for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                    {

                        
                        if (bnk.BatchNo == ds.Tables[1].Rows[i]["BatchNo"].ToString())
                        {
                            StickerSeq sq = new StickerSeq();
                            string qrsequence = ds.Tables[1].Rows[i]["Stickersequence"].ToString();
                            string qty = ds.Tables[1].Rows[i]["StickerQty"].ToString(); 
                            sq.StickerSequence = qrsequence;
                            sq.StickerQty = qty;
                            listStickerSeq.Add(sq);
                            bnk.StickerSeq = listStickerSeq;

                        }
                    
                       
                    }

                    bnk.VendorCode = row["VendorId"].ToString();
                    bnk.VendorName = row["Name"].ToString();
                    bnk.PurchaseOrderNo = row["PurchaseOrderId"].ToString();
                    ListView.Add(bnk);
                }
               
               
                res.MRNDetailsList = ListView;
                res.Status = "Success";
                res.Message = "Data retrived successfully";
            }
            }
            catch (Exception ex)
            {
                res.Status = "Failure";
                res.Message = "Data did not retrived successfully";
                
            }
            finally
            {
                dbcommand.Connection.Close();
            }
            return res;
        }

        [Route("api/FRD/GetCountData")]
        [HttpPost]
        public DatacountResponse GetCountData(DatacountRequest dr)
        {
            DatacountResponse res = new DatacountResponse();
            

            try
            {

                query = "InsertMRN";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.Connection.Open();
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.Parameters.AddWithValue("@QueryType", "APICountRecord");
                dbcommand.Parameters.AddWithValue("@userId", dr.userId);
              
                SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                DataSet ds = new DataSet();
                da.Fill(ds);
               
              
               
                res.Status = "Success";
                res.Message = "Data retrived successfully";
                res.TotalMrn = Convert.ToString(ds.Tables[0].Rows[0]["TotalMRN"]);
                res.TotalPending = Convert.ToString(ds.Tables[1].Rows[0]["TotalPending"]);
                res.Totalcomplete = Convert.ToString(ds.Tables[2].Rows[0]["TotalComplete"]);
                
            }
            catch (Exception ex)
            {
                res.Status = "Failure";
                res.Message = "Data did not retrived successfully";
                
            }
            finally
            {
                dbcommand.Connection.Close();
            }
            return res;
        }

        [Route("api/FRD/PackingSlip")]
        [HttpPost]
        //public PackingSlipResponse MRNRegisterPackingSlip(PackingSlipRequest MR)
        //{
        //    PackingSlipResponse res = new PackingSlipResponse();
        //    List<MRNDetails> ListView = new List<MRNDetails>();
        //    ServiceReference1.CallContext Cct = new ServiceReference1.CallContext();
        //    ServiceReference1.AMY_FRDServiceClient obj = new ServiceReference1.AMY_FRDServiceClient();
        //    try
        //    {
        //        string conn = ConfigurationManager.ConnectionStrings["Conn"].ToString();
        //        SqlConnection Conn = new SqlConnection(conn);

        //        SqlCommand cmd1 = new SqlCommand("Sp_AxWebserviceIntegration", Conn);
        //        cmd1.CommandType = CommandType.StoredProcedure;
        //        DataSet sdAX = new DataSet();
        //        SqlDataAdapter sd = new SqlDataAdapter();

        //        sd.SelectCommand = cmd1;

        //        sd.Fill(sdAX);

               

        //        obj.ClientCredentials.Windows.ClientCredential.Domain = Convert.ToString(sdAX.Tables[0].Rows[0]["Domain"]);
        //        obj.ClientCredentials.Windows.ClientCredential.UserName = Convert.ToString(sdAX.Tables[0].Rows[0]["Username"]);
        //        obj.ClientCredentials.Windows.ClientCredential.Password = Convert.ToString(sdAX.Tables[0].Rows[0]["Password"]);

                

        //        Cct.Company = Convert.ToString(sdAX.Tables[0].Rows[0]["Company"]);
        //        Cct.Language = Convert.ToString(sdAX.Tables[0].Rows[0]["Language"]);



              

        //        SqlCommand cmd = new SqlCommand("InsertMRN", Conn);
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        cmd.Parameters.AddWithValue("@QueryType", "APIGetLineNo");
        //        cmd.Parameters.AddWithValue("@MRNNumber", MR.MRNNumber);
        //        cmd.Parameters.AddWithValue("@activityNo", MR.ActivityNo);
        //        DataTable dt = new DataTable();
        //        SqlDataAdapter sd1 = new SqlDataAdapter();

        //        sd1.SelectCommand = cmd;

        //        sd1.Fill(dt);
        //     //   string jsondata = DataTableToJSON(dt);

        //        for (int i = 0; i < dt.Rows.Count; i++)
        //        {

        //            decimal lineNo = Convert.ToDecimal(dt.Rows[i]["ItemLineNumber"]);
        //            decimal Qty = Convert.ToDecimal(dt.Rows[i]["BatchQuantity"]);
        //            string PurchId = Convert.ToString(dt.Rows[i]["PurchaseOrderId"]);
        //            string ItemId = Convert.ToString(dt.Rows[i]["ItemId"]);
        //            string MrnNo = Convert.ToString(dt.Rows[i]["MRNNumber"]);
        //            string BatchId = Convert.ToString(dt.Rows[i]["BatchNo"]);



        //            string a = obj.RegisterPackingSlip(Cct, PurchId, ItemId, lineNo, BatchId, Qty);

                   
        //            if (a == "Registration success")
        //            {
                       
        //                string b = obj.InsertPackingSlipData(Cct, PurchId, Qty, MrnNo, lineNo);
        //            }


        //        }

        //        string PurchId1 = Convert.ToString(dt.Rows[0]["PurchaseOrderId"]);
               
        //        obj.PackingSlip(Cct, PurchId1);
        //        obj.DeleteData(Cct);
        //        SqlCommand cm = new SqlCommand("InsertMRN", Conn);
        //        cm.CommandType = CommandType.StoredProcedure;
        //        cm.Parameters.AddWithValue("@QueryType", "UpdateMRNFlag");
        //        cm.Parameters.AddWithValue("@MRNNumber", MR.MRNNumber);
        //        // cm.Parameters.AddWithValue("@dtQty",jsondata);
        //        DataTable tab = new DataTable();
        //        SqlDataAdapter sda = new SqlDataAdapter();
        //        sda.SelectCommand = cm;
        //        sda.Fill(tab);


        //        res.Status = "Success";
        //        res.Message = "Data posting successfully";
               


        //    }
        //    catch (Exception ex)
        //    {
        //        obj.DeleteData(Cct);
        //        res.Status = "Failure";
        //          res.Message = ex.Message;
                    
               
        //    }

        //    return res;
        //}
        public string DataTableToJSON(DataTable table)
        {
            var JSONString = new StringBuilder();
            if (table.Rows.Count > 0)
            {
                JSONString.Append("[");
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    JSONString.Append("{");
                    for (int j = 0; j < table.Columns.Count; j++)
                    {
                        if (j < table.Columns.Count - 1)
                        {
                            JSONString.Append("\"" + table.Columns[j].ColumnName.ToString() + "\":" + "\"" + table.Rows[i][j].ToString() + "\",");
                        }
                        else if (j == table.Columns.Count - 1)
                        {
                            JSONString.Append("\"" + table.Columns[j].ColumnName.ToString() + "\":" + "\"" + table.Rows[i][j].ToString() + "\"");
                        }
                    }
                    if (i == table.Rows.Count - 1)
                    {
                        JSONString.Append("}");
                    }
                    else
                    {
                        JSONString.Append("},");
                    }
                }
                JSONString.Append("]");
            }
            return JSONString.ToString();
        }

        public DataTable JsonStringToDataTable(string jsonString)
        {
            DataTable dt = new DataTable();
            string[] jsonStringArray = Regex.Split(jsonString.Replace("[", "").Replace("]", ""), "},{");
            List<string> ColumnsName = new List<string>();
            foreach (string jSA in jsonStringArray)
            {
                string[] jsonStringData = Regex.Split(jSA.Replace("{", "").Replace("}", ""), ",");
                foreach (string ColumnsNameData in jsonStringData)
                {
                    try
                    {
                        int idx = ColumnsNameData.IndexOf(":");
                        string ColumnsNameString = ColumnsNameData.Substring(0, idx - 1).Replace("\"", "");
                        if (!ColumnsName.Contains(ColumnsNameString))
                        {
                            ColumnsName.Add(ColumnsNameString);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("Error Parsing Column Name : {0}", ColumnsNameData));
                    }
                }
                break;
            }
            foreach (string AddColumnName in ColumnsName)
            {
                dt.Columns.Add(AddColumnName);
            }
            foreach (string jSA in jsonStringArray)
            {
                string[] RowData = Regex.Split(jSA.Replace("{", "").Replace("}", ""), ",");
                DataRow nr = dt.NewRow();
                foreach (string rowData in RowData)
                {
                    try
                    {
                        int idx = rowData.IndexOf(":");
                        string RowColumns = rowData.Substring(0, idx - 1).Replace("\"", "");
                        string RowDataString = rowData.Substring(idx + 1).Replace("\"", "");
                        nr[RowColumns] = RowDataString;
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
                dt.Rows.Add(nr);
            }
            return dt;
        }
    }
    public class DatacountRequest
    {
        public string userId { get; set; }
      

    }
    public class DatacountResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public string TotalMrn { get; set; }
        public string TotalPending { get; set; }
        public string Totalcomplete { get; set; }
    }

    public class PackingSlipResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
    }
    public class PackingSlipReq
    {
        public List<PackingSlipRequest> MRNDetailsList
        { get; set; }
    }
    public class PackingSlipRequest
        {
        public string PurchId { get; set; }
        public string ItemId { get; set; }
        
        public string LineNo { get; set; }
        public string BatchId { get; set; }
        public string ActivityNo { get; set; }
        public string MRNNumber { get; set; }

    }
    public class GetMrnDeta
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public List<MRNDetails> MRNDetailsList
        { get; set; }
        
    }
    public class MRNDetails
    {
        
        public string ItemId { get; set; }
        public string BatchNo { get; set; }
        public string Config { get; set; }
        public string ExpiryDate { get; set; }
        public string ReceivedQuantity { get; set; }
        public string itemArabicName { get; set; }
        public string itemname { get; set; }
        public string LineNo { get; set; }
        public List<StickerSeq> StickerSeq
        { get; set; }
        public string VendorCode { get; set; }
        public string VendorName { get; set; }
        public string PurchaseOrderNo { get; set; }

    }
    public class StickerSeq
    {
        public string StickerSequence { get; set; }
        public string StickerQty { get; set; }

    }

    public class GetMRN
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public List<GetMRNs> MRNList
        { get; set; }
    }
    public class GetMRNs
    {

        public string MRNNumber { get; set; }
        public string MRNDate { get; set; }

        public string ActivityNumber { get; set; }
       

           

    }
    public class MRNRequest
    {
        public string MNRNumber { get; set; }
        public string UserId { get; set; }
    }
}

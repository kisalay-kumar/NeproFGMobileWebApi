
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
using FRD_InventoryWebApi.Models;
using NeproWebApi.Models;

namespace FRD_InventoryWebApi.Controllers
{
    public class UserLoginController : ApiController
    {
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Conn"].ConnectionString);
        string query = ""; SqlCommand dbcommand;
        UserLogin res = new UserLogin();
        List<UserLoginResponse> ListView = new List<UserLoginResponse>();
        List<UserMultipleSiteInfo> UserMultipleSiteInfo = new List<UserMultipleSiteInfo>();
        UserLoginResponse Ulr = new UserLoginResponse();
        [Route("api/Nepro/Login")]
        [HttpPost]
        public UserLogin Login(UserLoginRequest ul)
        {



            if (ul.UserPin.Trim() != "" && Validation.ValidateUserPin(ul.UserPin.Trim()) != true)
            {

                res.Message = "UserPin Must be 4 digit";
                res.Status = "Failure";
                res.UserLoginResponse = ListView;
                Ulr.UserId = "";
                Ulr.UserPin = "";
                ListView.Add(Ulr);
                return res;



            }

            else
            {
                //bool Flag = true;

                try
                {
                    query = "SP_LoginWebApi";
                    dbcommand = new SqlCommand(query, conn);
                    dbcommand.Connection.Open();
                    dbcommand.CommandType = CommandType.StoredProcedure;
                    dbcommand.Parameters.AddWithValue("@QueryType", "UserAccess");
                    dbcommand.Parameters.AddWithValue("@UserPin", ul.UserPin);
                    SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                    DataSet ds = new DataSet();
                    da.Fill(ds);

                    if (ul.UserPin.Trim() != "" && ul.Password != "")
                    {

                        //if (Convert.ToString(ds.Tables[0].Rows[0]["UserPin"]) == "0")
                        //{

                        //    res.Message = "User has been disabled, please contact administrator!";
                        //    res.Status = "Failure";
                        //    res.UserLoginResponse = ListView;
                        //    Ulr.UserId = "";
                        //    Ulr.UserPin = "";
                        //    ListView.Add(Ulr);
                        //    return res;

                        //}
                        //else if (Convert.ToString(ds.Tables[0].Rows[0]["UserPin"]) == "-1")
                        //{

                        //    res.Message = "Invalid User Pin!";
                        //    res.Status = "Failure";
                        //    res.UserLoginResponse = ListView;
                        //    Ulr.UserId = "";
                        //    Ulr.UserPin = "";
                        //    ListView.Add(Ulr);
                        //    return res;

                        //}
                        //else
                        //{

                        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        {
                            //bool IsLoginfirsttime = Convert.ToBoolean(ds.Tables[0].Rows[0]["LoginFlag"]);
                            string UserType = Convert.ToString(ds.Tables[0].Rows[0]["UserRoleID"]);

                            if (UserType == "1")
                            {
                                #region For Admin
                                string strDbPassword = DBsecurity.Decrypt(Convert.ToString(ds.Tables[0].Rows[0]["Password"]), Convert.ToString(ds.Tables[0].Rows[0]["PasswordKey"]));
                                if (strDbPassword.Trim() != ul.Password.Trim())
                                {

                                    res.Message = "Wrong Password.";
                                    res.Status = "Failure";
                                    res.UserLoginResponse = ListView;
                                    Ulr.UserPin = "";
                                    Ulr.UserId = "";
                                    ListView.Add(Ulr);
                                    return res;

                                }
                                //
                                else
                                {
                                    res.Message = "Valid User.";
                                    res.Status = "success";
                                    res.UserLoginResponse = ListView;
                                    Ulr.UserId = Convert.ToString(ds.Tables[0].Rows[0]["UserID"]);
                                    Ulr.UserPin = "";
                                    ListView.Add(Ulr);
                                    return res;




                                }
                                #endregion
                            }
                            else
                            {
                                //if (IsLoginfirsttime == true)
                                //{
                                //    #region For First Time User Login
                                //    string strDbPassword = DBsecurity.Decrypt(Convert.ToString(ds.Tables[0].Rows[0]["Password"]), Convert.ToString(ds.Tables[0].Rows[0]["PasswordKey"]));
                                //    if (strDbPassword.Trim() != ul.Password.Trim())
                                //    {


                                //        res.Message = "Wrong Password.";
                                //        res.Status = "Failure";
                                //        res.UserLoginResponse = ListView;
                                //        Ulr.UserId ="";
                                //        Ulr.UserPin = "";
                                //        ListView.Add(Ulr);
                                //        return res;
                                //    }
                                //    else
                                //    {
                                //        res.Message = "Valid User.";
                                //        res.Status = "success";
                                //        res.UserLoginResponse = ListView;
                                //        Ulr.UserId = Convert.ToString(ds.Tables[0].Rows[0]["UserID"]);
                                //        Ulr.UserPin = "";
                                //        ListView.Add(Ulr);
                                //        return res;


                                //    }
                                //    #endregion
                                //}
                                //else
                                //{
                                #region For Current user
                                //if (Convert.ToString(ConfigurationManager.AppSettings["DefaultPassword"]) == ul.Password.Trim())
                                //{
                                res.Message = "Valid User.";
                                res.Status = "Success";
                                res.UserSiteInfo = UserMultipleSiteInfo;
                                foreach (DataRow row in ds.Tables[0].Rows)
                                {
                                    UserMultipleSiteInfo UserMultipleSite = new UserMultipleSiteInfo();
                                    UserMultipleSite.Site = row["AXSiteId"].ToString();
                                    UserMultipleSite.WareHouse = row["WareHouseAX"].ToString();
                                    UserMultipleSiteInfo.Add(UserMultipleSite);
                                }
                                res.UserLoginResponse = ListView;
                                Ulr.UserId = Convert.ToString(ds.Tables[0].Rows[0]["UserID"]);
                                Ulr.UserPin = ul.UserPin;
                                Ulr.IsMove = Convert.ToByte(ds.Tables[0].Rows[0]["IsMove"]);
                                Ulr.IsReportAsFinished = Convert.ToByte(ds.Tables[0].Rows[0]["IsReportAsFinished"]);
                                Ulr.IsQualityCheck = Convert.ToByte(ds.Tables[0].Rows[0]["IsQualityCheck"]);
                                Ulr.IsAssignedPlannedAndUnplanned = Convert.ToByte(ds.Tables[0].Rows[0]["IsAssignedPlannedAndUnplanned"]);
                                Ulr.IsPickAndCount = Convert.ToByte(ds.Tables[0].Rows[0]["IsPickAndCount"]);
                                Ulr.IsMergeStillage = Convert.ToByte(ds.Tables[0].Rows[0]["IsMergeStillage"]);
                                Ulr.IsReturnStillage = Convert.ToByte(ds.Tables[0].Rows[0]["IsReturnStillage"]);
                                Ulr.IsRecieveReturnStillage = Convert.ToByte(ds.Tables[0].Rows[0]["IsRecieveReturnStillage"]);
                                Ulr.IsLookUp = Convert.ToByte(ds.Tables[0].Rows[0]["IsLookUp"]);
                                Ulr.IsUpdateQty = Convert.ToByte(ds.Tables[0].Rows[0]["IsUpdateQty"]);
                                Ulr.IsProductionJournal = Convert.ToByte(ds.Tables[0].Rows[0]["IsProductionJournal"]);
                                Ulr.IsWorkOrderStartEnd = Convert.ToByte(ds.Tables[0].Rows[0]["IsWorkOrderStartEnd"]);
                                Ulr.UserName = Convert.ToString(ds.Tables[0].Rows[0]["UserName"]);
                                Ulr.EmailId = Convert.ToString(ds.Tables[0].Rows[0]["EmailId"]);
                                ListView.Add(Ulr);
                                return res;


                                //}
                                //else
                                //{

                                //    res.Message = "Wrong Password.";
                                //    res.Status = "Failure";
                                //    res.UserLoginResponse = ListView;
                                //    Ulr.UserId = "";
                                //    Ulr.UserPin = "";
                                //    ListView.Add(Ulr);
                                //    return res;


                                //}
                                #endregion
                                //}
                            }


                        }
                        else
                        {
                            res.Message = "Invalid User.";
                            res.Status = "Failure";
                            res.UserLoginResponse = ListView;
                            Ulr.UserId = "";
                            Ulr.UserPin = "";
                            ListView.Add(Ulr);
                            return res;


                        }
                        // }
                    }
                    else
                    {


                        res.Message = "Wrong User Pin or Password.";
                        res.Status = "Failure";
                        res.UserLoginResponse = ListView;
                        Ulr.UserId = "";
                        Ulr.UserPin = "";
                        ListView.Add(Ulr);
                        return res;

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
        }
        [Route("api/FRD/ResetPassword")]
        [HttpPost]
        //public UserLogin ResetPassword(ResetPassword rp)
        //{
        //    try
        //    {
        //        string EmailId = rp.EmailId;
        //        if (SendMail(EmailId) == 1)
        //        {

        //            int response = UpdatePassword(EmailId);
        //            if (response > 0)
        //            {
        //                res.Message = "Password has been Reset";
        //                res.Status = "success";
        //                res.UserLoginResponse = ListView;
        //                Ulr.UserId = "";
        //                Ulr.UserPin = "";
        //                ListView.Add(Ulr);
        //                return res;





        //            }

        //        }
        //        else
        //        {
        //            res.Message = "Invalid EmailId";
        //            res.Status = "Failure";
        //            res.UserLoginResponse = ListView;
        //            Ulr.UserId = "";
        //            Ulr.UserPin = "";
        //            ListView.Add(Ulr);
        //            return res;

        //        }

        //        return res;

        //    }
        //    catch (Exception ex)
        //    {
        //        res.Status = "Failure";
        //        res.Message = ex.Message;
        //        return res;
        //    }



        //}



        private int UpdatePassword(string emailId)
        {


            try
            {

                query = "SP_Login";
                dbcommand = new SqlCommand(query, conn);
                dbcommand.Connection.Open();
                dbcommand.CommandType = CommandType.StoredProcedure;
                dbcommand.Parameters.AddWithValue("@QueryType", "UpdateDefaultPassword");
                dbcommand.Parameters.AddWithValue("@Emailid", emailId);
                SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                DataTable dt = new DataTable();
                da.Fill(dt);
                conn.Close();
                return dt.Rows.Count;
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
            return 0;
        }


        private int SendMail(string EmailId)
        {


            query = "SP_Login";
            dbcommand = new SqlCommand(query, conn);
            dbcommand.Connection.Open();
            dbcommand.CommandType = CommandType.StoredProcedure;
            dbcommand.Parameters.AddWithValue("@QueryType", "ForgotPassword");
            dbcommand.Parameters.AddWithValue("@Emailid", EmailId);
            SqlDataAdapter da = new SqlDataAdapter(dbcommand);
            DataSet ds = new DataSet();
            da.Fill(ds);

            string verifyedEmail = Convert.ToString(ds.Tables[0].Rows[0]["EmailId"]);


            if (verifyedEmail != "" && verifyedEmail != "0")
            {
                try
                {
                    string AdminEmail = Convert.ToString(ds.Tables[1].Rows[0]["EmailId"]);
                    StringBuilder sb = new StringBuilder();

                    string SMTPHost = ConfigurationManager.AppSettings["SMTPHost"].ToString();
                    string UserId = ConfigurationManager.AppSettings["UserId"].ToString();
                    string MailPassword = ConfigurationManager.AppSettings["MailPassword"].ToString();
                    string SMTPPort = ConfigurationManager.AppSettings["SMTPPort"].ToString();
                    string SMTPEnableSsl = ConfigurationManager.AppSettings["SMTPEnableSsl"].ToString();

                    sb.Append("Dear  " + Convert.ToString(ds.Tables[0].Rows[0]["UserName"]) + ",<br> <br>");
                    sb.Append(" Reset Default Password is: " + ConfigurationManager.AppSettings["DefaultPassword"] + "<br> <br>");

                    sb.Append("<div><p style='font-size:16px; line-height:22px; color:#ed7d31; font-weight:bold; margin-bottom: 2px;'>Thanks & Regards</p> <div style='background-color:#e7e6e6; padding:6px 0px 15px 6px; border-right: 5px solid #dc9004; width:330px; margin-bottom: 15px;'><p style='font-size:18px; line-height:22px; color:#787878; font-weight:normal; margin-bottom: 5px;'>Support team</p><div><div style='display:inline-block; '><img src='../assets/img/globe-icon.png' style='border:none'/><p style='font-size:12px; color:#787878; text-decoration:underline; padding-left:5px; padding-right:7px; border-right: 1px solid #dc9004'>www.amysoftech.in</p> </div> <div style='display:inline-block; padding-left:4px'><img src='../assets/img/email-icon.png' style='border:none'/><p style='font-size:12px; color:#787878; text-decoration:underline; padding-left:5px'>support@amysoftech.in</p></div></div> </div> <p style=' margin-bottom: 0px; font-weight: bold; color: black; font-size: 16px; overflow: hidden; height: 15px;'>   ************************************************************************</p><p style=' font-size:14px; line-height:18px; color:#ed7d31; font-weight:normal; margin-bottom:0px; padding-bottom:5px'><strong>Note:</strong> This is a system generated email, do not reply on this email.</p><p style=' margin-bottom: 0px; font-weight: bold; color: black; font-size: 16px; overflow: hidden; height: 15px;'> ************************************************************************</p></div>");


                    SmtpClient smtpClient = new SmtpClient();

                    MailMessage mailmsg = new MailMessage();
                    MailAddress mailaddress = new MailAddress(UserId);

                    mailmsg.To.Add(Convert.ToString(ds.Tables[0].Rows[0]["EmailId"]));
                    // mailmsg.To.Add("satyendaryadav093@gmail.com");


                    mailmsg.Body = Convert.ToString(sb);


                    mailmsg.From = mailaddress;

                    mailmsg.Subject = "Change Password";
                    mailmsg.IsBodyHtml = true;




                    smtpClient.Host = SMTPHost;
                    smtpClient.Port = Convert.ToInt32(SMTPPort);
                    smtpClient.EnableSsl = Convert.ToBoolean(SMTPEnableSsl);
                    smtpClient.UseDefaultCredentials = true;
                    smtpClient.Credentials = new System.Net.NetworkCredential(UserId, MailPassword);
                    smtpClient.Send(mailmsg);
                    conn.Close();
                    return 1;
                }
                catch (Exception ex)
                {
                    res.Status = "Failure";
                    res.Message = ex.Message;
                    return -1;

                }
                finally
                {
                    dbcommand.Connection.Close();
                }


            }

            else
            {
                //res.ResDescription = "Invalid EmailId!";
                //res.Status = "Failure";

                return 0;
            }

        }


        [Route("api/FRD/ChangePassword")]
        [HttpPost]
        public UserLogin ChangePassword(UserLoginRequest ul)
        {
            try
            {
                if (ul.UserPin.Trim() == "")
                {
                    res.Message = "User cannot be blank";
                    res.Status = "Failure";
                    res.UserLoginResponse = ListView;
                    Ulr.UserId = "";
                    Ulr.UserPin = "";
                    ListView.Add(Ulr);
                    return res;


                }
                else
                {


                    if (ul.Password.Trim() != "")
                    {
                        string pass = string.Empty;
                        string passkey = string.Empty;

                        pass = DBsecurity.Encrypt(ul.Password.Trim(), ref passkey);

                        query = "SP_Login";
                        dbcommand = new SqlCommand(query, conn);
                        dbcommand.Connection.Open();
                        dbcommand.CommandType = CommandType.StoredProcedure;
                        dbcommand.Parameters.AddWithValue("@QueryType", "APIUpdatePassword");
                        dbcommand.Parameters.AddWithValue("@UserPin", ul.UserPin);
                        dbcommand.Parameters.AddWithValue("@Password", pass);
                        dbcommand.Parameters.AddWithValue("@PasswordKey", passkey);
                        SqlDataAdapter da = new SqlDataAdapter(dbcommand);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        conn.Close();



                        if (dt.Rows.Count > 0)
                        {
                            res.Message = "Password has been changed";
                            res.Status = "success";
                            res.UserLoginResponse = ListView;
                            Ulr.UserId = "";
                            Ulr.UserPin = "";
                            ListView.Add(Ulr);


                        }
                    }
                    else
                    {
                        res.Message = "Password cannot be blank";
                        res.Status = "Failure";
                        res.UserLoginResponse = ListView;
                        Ulr.UserId = "";
                        Ulr.UserPin = "";
                        ListView.Add(Ulr);

                    }
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
    }
    public class UserLogin
    {

        public string Status { get; set; }
        public string Message { get; set; }
        public List<UserMultipleSiteInfo> UserSiteInfo { get; set; }
        public List<UserLoginResponse> UserLoginResponse
        { get; set; }

    }

    public class UserMultipleSiteInfo
    {
        public string Site { get; set; }
        public string WareHouse { get; set; }
    }
    public class UserLoginRequest
    {

        public string UserPin { get; set; }
        public string Password { get; set; }

    }
    public class UserLoginResponse
    {
        public string UserId { get; set; }
        public string UserPin { get; set; }
        public byte IsMove { get; set; }
        public byte IsReportAsFinished { get; set; }
        public byte IsQualityCheck { get; set; }
        public byte IsAssignedPlannedAndUnplanned { get; set; }
        public byte IsPickAndCount { get; set; }
        public byte IsMergeStillage { get; set; }
        public byte IsReturnStillage { get; set; }
        public byte IsRecieveReturnStillage { get; set; }
        public byte IsLookUp { get; set; }
        public byte IsUpdateQty { get; set; }
        public byte IsProductionJournal { get; set; }
        public byte IsWorkOrderStartEnd { get; set; }
        public string UserName { get; set; }
        public string EmailId { get; set; }

    }
}

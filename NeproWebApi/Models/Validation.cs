using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace FRD_InventoryWebApi.Models
{
    public class Validation
    {

        //public static bool ValidateAppID(string AppID)
        //{
        //    bool status = false;
        //    try
        //    {
        //        string temp = ConfigurationManager.AppSettings["AppID"];
        //        if (temp.Trim() == AppID)
        //            status = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.Out.WriteLine("-----------------");
        //        Console.Out.WriteLine(ex.Message);
        //    }
        //    return status;
        //}
     
     
        public static bool ValidateUserPin(string value)
        {
            UpdateTrasnfer UT = new UpdateTrasnfer();

            bool status = false;
            try
            {
                if (value.Length == 4)
                {
                    Int64 check;
                    if (Int64.TryParse(value, out check))
                        status = true;
                }
                else
                {
                    status = false;
                }
            }
            catch (Exception ex)
            {
                UT.Status = "Failure";
                UT.Message = ex.Message;
            }
            return status;
        }
       
       



    }
}
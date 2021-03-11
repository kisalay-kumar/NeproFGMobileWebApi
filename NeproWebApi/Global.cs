using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace NeproWebApi
{
    public class Global
    {
        public string GetUsername(string UserId)
        {
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["Conn"].ConnectionString);
            string query = ""; SqlCommand dbcommand;
            query = "Sp_MasterDataWebApi";
            dbcommand = new SqlCommand(query, conn);
            //dbcommand.Connection.Open();
            dbcommand.CommandType = CommandType.StoredProcedure;
            dbcommand.Parameters.AddWithValue("@QueryType", "GetUsername");
            dbcommand.Parameters.AddWithValue("@UserId", UserId);
            dbcommand.CommandTimeout = 0;
            SqlDataAdapter da = new SqlDataAdapter(dbcommand);
            da = new SqlDataAdapter(dbcommand);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt.Rows[0]["UserName"].ToString();
        }
    }
}
using AddressTransfer.BO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AddressTransfer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnTransfer_Click(object sender, EventArgs e)
        {
            /*--INSERT INTO table2 (column1, column2, column3, ...)
                --SELECT column1, column2, column3, ...
                --FROM table1
                --WHERE condition;

                INSERT INTO [BaseStation].[dbo].[Table_AddressEx](dbLat,dbLon,strAddress)
                SELECT rs.lat, rs.lon, rs.messagebody FROM [GPSDB].[dbo].[Request_SMS] rs
                WHERE rs.lat > 0 OR rs.lon > 0 AND rs.lat = (SELECT dbLat FROM [BaseStation].[dbo].[Table_AddressEx])*/

            string GpsConStr = ConfigurationManager.ConnectionStrings["GpsDbConStr"].ConnectionString;
            string BaseStationConStr = ConfigurationManager.ConnectionStrings["BaseStationConStr"].ConnectionString;
            SqlConnection con;
            SqlCommand cmd;
            SqlDataReader rd;

            // get all from Request_SMS
            con = new SqlConnection(GpsConStr);
            cmd = new SqlCommand();
            cmd.Connection = con;
            con.Open();
            cmd.CommandText = "SELECT [id],[phone],[messagebody],[datetime],[status],[sentmsg],[lat],[lon] FROM [Request_SMS]";
            rd = cmd.ExecuteReader();
            List<Request_SMS> Request_SMSs = new List<Request_SMS>();
            while (rd.Read())
            {
                Request_SMS Request_Sms = new Request_SMS();
                Request_Sms.id = rd.GetInt32(0);
                Request_Sms.phone = rd.GetString(1);
                Request_Sms.messagebody = rd.GetString(2);
                Request_Sms.datetime = rd.GetString(3);
                Request_Sms.status = rd.GetInt32(4);
                Request_Sms.sentmsg = rd.GetString(5);
                
                decimal lat = 0;
                decimal lon = 0;
                if (decimal.TryParse(rd.GetString(6), out lat) && decimal.TryParse(rd.GetString(7), out lon))
                {
                    Request_Sms.lat = lat;
                    Request_Sms.lon = lon;
                    Request_SMSs.Add(Request_Sms);
                }
                
            }
            rd.Close();
            con.Close();

            // get all from Table_AddressEx
            con = new SqlConnection(BaseStationConStr);
            cmd = new SqlCommand();
            cmd.Connection = con;
            con.Open();
            cmd.CommandText = "SELECT [nID],[dbLon],[dbLat],[strAddress] FROM [Table_AddressEx]";
            rd = cmd.ExecuteReader();
            List<Table_AddressEx> Table_AddressExS = new List<Table_AddressEx>();
            while (rd.Read())
            {
                Table_AddressEx Table_AddressEx = new Table_AddressEx();
                Table_AddressEx.nID = rd.GetInt32(0);
                Table_AddressEx.dbLon = rd.GetDecimal(1);
                Table_AddressEx.dbLat = rd.GetDecimal(2);
                Table_AddressEx.strAddress = rd.GetString(3);

                Table_AddressExS.Add(Table_AddressEx);
            }
            rd.Close();
            con.Close();

            // connection for search
            con = new SqlConnection(BaseStationConStr);
            cmd = new SqlCommand();
            cmd.Connection = con;
            con.Open();

            // loop in Request_SMS
            List<Table_AddressEx> insertTable_AddressExS = new List<Table_AddressEx>();
            foreach (var Request_SMS in Request_SMSs)
            {
                var insertTable_AddressEx = (from o in Table_AddressExS where o.dbLat == Request_SMS.lat && o.dbLon == Request_SMS.lon select o).FirstOrDefault();
                if (insertTable_AddressEx == null)
                {
                    //insertTable_AddressEx = new Table_AddressEx();
                    //insertTable_AddressEx.nID = Request_SMS.id;
                    //insertTable_AddressEx.dbLat = Request_SMS.lat;
                    //insertTable_AddressEx.dbLon = Request_SMS.lon;
                    string sentmsg = Request_SMS.sentmsg;
                    sentmsg = sentmsg.Substring(0, sentmsg.IndexOf("Speed:") - 1);
                    sentmsg = sentmsg.Replace("'", "''");
                    //insertTable_AddressEx.strAddress = sentmsg;
                    //insertTable_AddressExS.Add(insertTable_AddressEx);

                    // insert

                    cmd.CommandText = "INSERT INTO [Table_AddressEx]([dbLon],[dbLat],[strAddress]) VALUES('" + Request_SMS.lon + "','" + Request_SMS.lat + "','" + sentmsg + "')";
                    cmd.ExecuteNonQuery();
                    
                }
            }

            // connection close
            con.Close();

            // insert
            //con = new SqlConnection(BaseStationConStr);
            //cmd = new SqlCommand();
            //cmd.Connection = con;
            //con.Open();
            //foreach (var insert in insertTable_AddressExS)
            //{
            //    cmd.CommandText = "INSERT INTO [Table_AddressEx]([dbLon],[dbLat],[strAddress]) VALUES('" + insert.dbLon + "','" + insert.dbLat + "','" + insert.strAddress + "')";
            //    cmd.ExecuteNonQuery();
            //}
            //con.Close();

            MessageBox.Show("Successed! (" + insertTable_AddressExS.Count + ") addresses saved in Table_AddressEx of BaseStation");
        }
    }
}

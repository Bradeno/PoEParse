using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Net;
using System.Data;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace PoEParse
{
    class PoEFuncs
    {
        string SQLConnectionString = @"Server=SUPER-COMPUTER; Database=POE; Integrated Security=True";

        public void SaveTheJson(string nextStashId)
        {
            using (var w = new WebClient())
            {
                var Json_Data = w.DownloadString("http://betaapi.pathofexile.com/api/public-stash-tabs?id=" + nextStashId);
                StashData root = JsonConvert.DeserializeObject<StashData>(Json_Data);

                SaveChangeId(root);
                SaveStashData(root);

            }
        }

        public void SaveChangeId(StashData root)
        {
            List<StashData> ChangeIdList = new List<StashData>();
            ChangeIdList.Add(root);

            ListtoDataTableConverter converter = new ListtoDataTableConverter();
            DataTable dt = converter.ToDataTable(ChangeIdList);

            using (SqlConnection conn = new SqlConnection(SQLConnectionString))
            using (SqlCommand comm = new SqlCommand("usp_AddChangeId", conn))
            {
                conn.Open();
                comm.CommandType = CommandType.StoredProcedure;
                comm.Parameters.AddWithValue("@newId", dt).SqlDbType = SqlDbType.Structured;
                comm.ExecuteNonQuery();
                comm.Parameters.Clear();
            }
        }


        public void SaveStashData(StashData root)
        {
            List<Stash> StashDataList = new List<Stash>();
            foreach (Stash s in root.stashes)
            {
                StashDataList.Add(s);
                //For each Stash, transfer it's set of items.
                //SaveItemData(s);
            }
            
            ListtoDataTableConverter converter = new ListtoDataTableConverter();
            DataTable dt = converter.ToDataTable(StashDataList);

            using (SqlConnection conn = new SqlConnection(SQLConnectionString))
            using (SqlCommand comm = new SqlCommand("usp_StashParse", conn))
            {
                conn.Open();
                comm.CommandType = CommandType.StoredProcedure;
                comm.Parameters.AddWithValue("@newStashData", dt).SqlDbType = SqlDbType.Structured;
                comm.ExecuteNonQuery();
                comm.Parameters.Clear();
            }     
        
        }

        public void SaveItemData(Stash rootStash)
        {
            List<Item> ItemDataList = new List<Item>();
            foreach (Item i in rootStash.items)
            {
                ItemDataList.Add(i);
            }

            ListtoDataTableConverter converter = new ListtoDataTableConverter();
            DataTable dt = converter.ToDataTable(ItemDataList);

            using (SqlConnection conn = new SqlConnection(SQLConnectionString))
            using (SqlCommand comm = new SqlCommand("usp_BaseItemsParse", conn))
            {
                conn.Open();
                comm.CommandType = CommandType.StoredProcedure;
                comm.Parameters.AddWithValue("@newItemsData", dt).SqlDbType = SqlDbType.Structured;
                comm.Parameters.Add(new SqlParameter("@stashId", rootStash.id));
                comm.Parameters.Add(new SqlParameter("@accountName", rootStash.accountName));
                comm.ExecuteNonQuery();
                comm.Parameters.Clear();
            }

        }









        public class ListtoDataTableConverter
        {
            public DataTable ToDataTable<T>(List<T> items)
            {
                DataTable dataTable = new DataTable(typeof(T).Name);
                //Get all the properties
                PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (PropertyInfo prop in Props)
                {
                    //Setting column names as Property names
                    dataTable.Columns.Add(prop.Name);
                }
                foreach (T item in items)
                {
                    var values = new object[Props.Length];
                    for (int i = 0; i < Props.Length; i++)
                    {
                        //inserting property values to datatable rows
                        values[i] = Props[i].GetValue(item, null);
                    }
                    dataTable.Rows.Add(values);
                }
                //put a breakpoint here and check datatable
                return dataTable;
            }
        }


        string GetNextStashId(SqlConnection conn)
        {
            string nextId = "";
            return (nextId);
        }


    }
}

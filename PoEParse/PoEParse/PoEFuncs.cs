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

                //SaveChangeId(root);
                //SaveStashData(root);


                //shove all the data in datatables for processing.
                ListEverythingTest(root);

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

        //trying to make a do everything loop to populate all list objects.
        public void ListEverythingTest (StashData root)
        {
            ListtoDataTableConverter dataTableConverter = new ListtoDataTableConverter();

            List<StashData> ChangeIdList = new List<StashData>();
            List<Stash> StashList = new List<Stash>();
            List<Item> ItemList = new List<Item>();
            List<Socket> SocketList = new List<Socket>();
            List<Property1> PropertyList1 = new List<Property1>();
            List<Additionalproperty> AdditionalPropertyList = new List<Additionalproperty>();
            List<Requirement> RequirementList = new List<Requirement>();
            List<Socketeditem> SocketedItemList = new List<Socketeditem>();
            List<Property2> PropertyList2 = new List<Property2>();
            List<Additionalproperty1> AdditionalPropertyList2 = new List<Additionalproperty1>();
            List<Requirement1> RequirementList2 = new List<Requirement1>();
            List<Nextlevelrequirement> NextLevelRequirementList = new List<Nextlevelrequirement>();
            List<Nextlevelrequirement1> NextLevelRequirementList2 = new List<Nextlevelrequirement1>();

            //next change ID
            ChangeIdList.Add(root);

            //start megaloop
            foreach (Stash stash in root.stashes)
            {
                StashList.Add(stash);

                foreach (Item item in stash.items)
                {
                    ItemList.Add(item);

                    foreach (Socket socket in item.sockets)
                    {
                        SocketList.Add(socket);
                    }

                    foreach (Socketeditem socketedItem in item.socketedItems)
                    {
                        SocketedItemList.Add(socketedItem);

                        foreach (Requirement1 requirement1 in socketedItem.requirements)
                        {
                            RequirementList2.Add(requirement1);
                        }

                        if (socketedItem.nextLevelRequirements != null)
                        {
                            foreach (Nextlevelrequirement nextLevelReq in socketedItem.nextLevelRequirements)
                            {
                                NextLevelRequirementList.Add(nextLevelReq);
                            }

                        }

                        foreach (Property2 property2 in socketedItem.properties)
                        {
                            PropertyList2.Add(property2);
                        }

                        foreach (Additionalproperty1 additionalProps1 in socketedItem.additionalProperties)
                        {
                            AdditionalPropertyList2.Add(additionalProps1);
                        }
                    }

                    if (item.nextLevelRequirements != null)
                    {
                        foreach (Nextlevelrequirement1 nextLevelReq1 in item.nextLevelRequirements)
                        {
                            //NextLevelRequirementList2.Add(nextLevelReq1);
                        }
                    }                    

                    if (item.properties != null)
                    {
                        foreach (Property1 property1 in item.properties)
                        {
                            PropertyList1.Add(property1);
                        }
                    }
                    

                    if (item.additionalProperties != null)
                    {
                        foreach (Additionalproperty additionalProp in item.additionalProperties)
                        {
                            AdditionalPropertyList.Add(additionalProp);
                        }
                    }                    

                    if (item.requirements != null)
                    {
                        foreach (Requirement requirement in item.requirements)
                        {
                            RequirementList.Add(requirement);
                        }
                    }
                    
                }                
            }

            //Convert each list to a datatable
            DataTable ChangeId_DT = dataTableConverter.ToDataTable(ChangeIdList);
            DataTable Stashes_DT = dataTableConverter.ToDataTable(StashList);
            DataTable Items_DT = dataTableConverter.ToDataTable(ItemList);
            DataTable SocketedItems_DT = dataTableConverter.ToDataTable(SocketedItemList);
            DataTable Sockets_DT = dataTableConverter.ToDataTable(SocketList);
            DataTable Properties1_DT = dataTableConverter.ToDataTable(PropertyList1);
            DataTable Properties2_DT = dataTableConverter.ToDataTable(PropertyList2);
            DataTable AdditionalProperties1_DT = dataTableConverter.ToDataTable(AdditionalPropertyList);
            DataTable AdditionalProperties2_DT = dataTableConverter.ToDataTable(AdditionalPropertyList2);
            DataTable Requirements1_DT = dataTableConverter.ToDataTable(RequirementList);
            DataTable Requirements2_DT = dataTableConverter.ToDataTable(RequirementList2);                 
            DataTable NextLevelRequirement1_DT = dataTableConverter.ToDataTable(NextLevelRequirementList);
            DataTable NextLevelRequirement2_DT = dataTableConverter.ToDataTable(NextLevelRequirementList2);


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

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

            //Object Lists
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

            //Custom Tables
            DataTable Mods_DT = new DataTable("Mods");
            Mods_DT.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("itemId"),
                new DataColumn("modName"),
                new DataColumn("modValue1"),
                new DataColumn("modValue2"),
                new DataColumn("modValue3"),
                new DataColumn("modValue4"),
                new DataColumn("modValue5"),
                new DataColumn("modValue6"),
                new DataColumn("modValue7"),
                new DataColumn("modValue8"),
                new DataColumn("modValue9"),
                new DataColumn("modValue10"),
                new DataColumn("modValue11"),
                new DataColumn("modValue12"),
            });

            //next change ID
            ChangeIdList.Add(root);

            //start megaloop
            foreach (Stash stash in root.stashes)
            {
                StashList.Add(stash);

                foreach (Item item in stash.items)
                {
                    if (item.flavourText != null)
                    {
                        Item aie = new Item();
                        aie.flavourText = item.flavourText;
                        int len = aie.flavourText.Count();

                        for (int i = 0; i < len; i++)
                        {
                            item.favourTextVal += item.flavourText[i] + " ";
                        }
                    }
                    else
                    {
                        item.favourTextVal = "";
                    }                    

                    ItemList.Add(item);

                    if (item.explicitMods != null)
                    {
                        String[] modValue;
                        modValue = new String[12];
                        modValue.DefaultIfEmpty("");

                        List<String> ModsList = new List<string>();
                        ModsList = item.explicitMods.ToList();

                        int numMods = ModsList.Count();

                        for (int i = 0; i < numMods; i++)
                        {
                            modValue[i] = ModsList[i].ToString();
                        }

                        //Mods Datatable to send to SQL
                        Mods_DT.Rows.Add(item.id, "Explicit", modValue[0], modValue[1], modValue[2], modValue[3], modValue[4], modValue[5], modValue[6], modValue[7], modValue[8], modValue[9], modValue[10], modValue[11]);
                    }

                    if (item.implicitMods != null)
                    {
                        String[] modValue;
                        modValue = new String[12];
                        modValue.DefaultIfEmpty("");

                        List<String> ModsList = new List<string>();
                        ModsList = item.implicitMods.ToList();

                        int numMods = ModsList.Count();

                        for (int i = 0; i < numMods; i++)
                        {
                            modValue[i] = ModsList[i].ToString();
                        }

                        //Mods Datatable to send to SQL
                        Mods_DT.Rows.Add(item.id, "Implicit", modValue[0], modValue[1], modValue[2], modValue[3], modValue[4], modValue[5], modValue[6], modValue[7], modValue[8], modValue[9], modValue[10], modValue[11]);
                    }

                    if (item.craftedMods != null)
                    {
                        String[] modValue;
                        modValue = new String[12];
                        modValue.DefaultIfEmpty("");

                        List<String> ModsList = new List<string>();
                        ModsList = item.craftedMods.ToList();

                        int numMods = ModsList.Count();

                        for (int i = 0; i < numMods; i++)
                        {
                            modValue[i] = ModsList[i].ToString();
                        }

                        //Mods Datatable to send to SQL
                        Mods_DT.Rows.Add(item.id, "Cosmetic", modValue[0], modValue[1], modValue[2], modValue[3], modValue[4], modValue[5], modValue[6], modValue[7], modValue[8], modValue[9], modValue[10], modValue[11]);
                    }

                    if (item.utilityMods != null)
                    {
                        String[] modValue;
                        modValue = new String[12];
                        modValue.DefaultIfEmpty("");

                        List<String> ModsList = new List<string>();
                        ModsList = item.utilityMods.ToList();

                        int numMods = ModsList.Count();

                        for (int i = 0; i < numMods; i++)
                        {
                            modValue[i] = ModsList[i].ToString();
                        }

                        //Mods Datatable to send to SQL
                        Mods_DT.Rows.Add(item.id, "Utility", modValue[0], modValue[1], modValue[2], modValue[3], modValue[4], modValue[5], modValue[6], modValue[7], modValue[8], modValue[9], modValue[10], modValue[11]);
                    }

                    if (item.enchantMods != null)
                    {
                        String[] modValue;
                        modValue = new String[12];
                        modValue.DefaultIfEmpty("");

                        List<String> ModsList = new List<string>();
                        ModsList = item.enchantMods.ToList();

                        int numMods = ModsList.Count();

                        for (int i = 0; i < numMods; i++)
                        {
                            modValue[i] = ModsList[i].ToString();
                        }

                        //Mods Datatable to send to SQL
                        Mods_DT.Rows.Add(item.id, "Enchanted", modValue[0], modValue[1], modValue[2], modValue[3], modValue[4], modValue[5], modValue[6], modValue[7], modValue[8], modValue[9], modValue[10], modValue[11]);
                    }


                    foreach (Socket socket in item.sockets)
                    {
                        SocketList.Add(socket);
                    }

                    foreach (Socketeditem socketedItem in item.socketedItems)
                    {
                        SocketedItemList.Add(socketedItem);

                        foreach (Requirement1 requirement1 in socketedItem.requirements)
                        {
                            requirement1.id = socketedItem.id;
                            requirement1.amount = requirement1.values[0][0];
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
                            property2.id = socketedItem.id;
                            PropertyList2.Add(property2);
                        }

                        foreach (Additionalproperty1 additionalProps1 in socketedItem.additionalProperties)
                        {
                            additionalProps1.id = socketedItem.id;
                            additionalProps1.amount = additionalProps1.values[0][0];
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
                            property1.id = item.id;                          
                            PropertyList1.Add(property1);
                        }
                    }
                    

                    if (item.additionalProperties != null)
                    {
                        foreach (Additionalproperty additionalProp in item.additionalProperties)
                        {
                            additionalProp.id = item.id;
                            additionalProp.amount = additionalProp.values[0][0];
                            AdditionalPropertyList.Add(additionalProp);
                        }
                    }                    

                    if (item.requirements != null)
                    {
                        foreach (Requirement requirement in item.requirements)
                        {
                            requirement.id = item.id;
                            requirement.amount = requirement.values[0][0];
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

            //Clean up the DataTables
            Stashes_DT.Columns.Remove("items");

            if (Items_DT.Columns.Contains("sockets")){ Items_DT.Columns.Remove("sockets"); }            
            if (Items_DT.Columns.Contains("socketedItems")) { Items_DT.Columns.Remove("socketedItems"); }            
            if (Items_DT.Columns.Contains("nextLevelRequirements")) { Items_DT.Columns.Remove("nextLevelRequirements"); }            
            if (Items_DT.Columns.Contains("properties")) { Items_DT.Columns.Remove("properties"); }            
            if (Items_DT.Columns.Contains("additionalProperties")) { Items_DT.Columns.Remove("additionalProperties"); }            
            if (Items_DT.Columns.Contains("requirements")) { Items_DT.Columns.Remove("requirements"); }
            if (Items_DT.Columns.Contains("flavourText")) { Items_DT.Columns.Remove("flavourText"); }
            if (Items_DT.Columns.Contains("explicitMods")) { Items_DT.Columns.Remove("explicitMods"); }
            if (Items_DT.Columns.Contains("cosmeticMods")) { Items_DT.Columns.Remove("cosmeticMods"); }
            if (Items_DT.Columns.Contains("implicitMods")) { Items_DT.Columns.Remove("implicitMods"); }
            if (Items_DT.Columns.Contains("craftedMods")) { Items_DT.Columns.Remove("craftedMods"); }
            if (Items_DT.Columns.Contains("utilityMods")) { Items_DT.Columns.Remove("utilityMods"); }
            if (Items_DT.Columns.Contains("enchantMods")) { Items_DT.Columns.Remove("enchantMods"); }

            if (SocketedItems_DT.Columns.Contains("requirements")) { SocketedItems_DT.Columns.Remove("requirements"); }
            if (SocketedItems_DT.Columns.Contains("nextLevelRequirements")) { SocketedItems_DT.Columns.Remove("nextLevelRequirements"); }            
            if (SocketedItems_DT.Columns.Contains("properties")) { SocketedItems_DT.Columns.Remove("properties"); }            
            if (SocketedItems_DT.Columns.Contains("additionalProperties")) { SocketedItems_DT.Columns.Remove("additionalProperties"); }
            if (SocketedItems_DT.Columns.Contains("socketedItems")) { SocketedItems_DT.Columns.Remove("socketedItems"); }

            if (Requirements1_DT.Columns.Contains("values")) { Requirements1_DT.Columns.Remove("values"); }
            if (Requirements2_DT.Columns.Contains("values")) { Requirements2_DT.Columns.Remove("values"); }

            if (AdditionalProperties1_DT.Columns.Contains("values")) { AdditionalProperties1_DT.Columns.Remove("values"); }
            if (AdditionalProperties2_DT.Columns.Contains("values")) { AdditionalProperties2_DT.Columns.Remove("values"); }

            if (Properties1_DT.Columns.Contains("values")) { Properties1_DT.Columns.Remove("values"); }
            if (Properties2_DT.Columns.Contains("values")) { Properties2_DT.Columns.Remove("values"); }

            //, "sockets", "socketedItems", "properties", "additionalProperties", "requirements");

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

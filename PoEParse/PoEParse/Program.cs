using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PoEParse
{
    class Program
    {
        
        static void Main(string[] args)
        {
            PoEFuncs PoE = new PoEFuncs();
            PoE.SaveTheJson("0-0-0-0");

            Console.WriteLine("test");
            Console.ReadLine();
        }
    }
}

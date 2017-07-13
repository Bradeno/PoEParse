using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace PoEParse
{
    class Program
    {
        private static Timer timer;

        static void Main(string[] args)
        {

            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromMilliseconds(1500);
            timer = new System.Threading.Timer((t) =>
            {

                PoEFuncs PoE = new PoEFuncs();
                if (PoE.Get_Next_Change_ID() == null)
                {
                    Console.WriteLine("Using 0-0-0-0");
                    PoE.SaveTheJson("0-0-0-0");
                }
                else
                {
                    Console.WriteLine("Using: " + PoE.Get_Next_Change_ID());
                    PoE.SaveTheJson(PoE.Get_Next_Change_ID());
                }


            }, null, startTimeSpan, periodTimeSpan);


            

            Console.WriteLine("test");
            Console.ReadLine();
        }



    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SteamAuth;

namespace SteamGuardCode
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Steam Guard Code";
            try
            {
                if (File.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}\License.lic"))
                {
                    string key = "";
                    using (StreamReader sr = new StreamReader($@"{AppDomain.CurrentDomain.BaseDirectory}\License.lic"))
                    {
                        key = sr.ReadToEnd();
                    }
                    key = key.Replace("\r\n", "");

                    if (PcInfo.GetCurrentPCInfo() == key)
                    {
                        Console.WriteLine("Enter the secret key");
                        string k = Console.ReadLine();

                        SteamGuardAccount acc = new SteamGuardAccount();
                        acc.SharedSecret = k;
                        string str = acc.GenerateSteamGuardCode();
                        Console.WriteLine(str);
                    }
                    else
                    {
                        Console.WriteLine("[SYSTEM] License not found");
                        Thread.Sleep(5000);
                        Environment.Exit(0);
                    }
                }
                else
                {
                    Console.WriteLine("[SYSTEM] License not found");
                    Thread.Sleep(5000);
                    Environment.Exit(0);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Thread.Sleep(5000);
                Environment.Exit(0);
            }

            Console.ReadKey();
        }
    }
}

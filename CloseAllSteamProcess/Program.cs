using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CloseAllSteamProcess
{
    class Program
    {
        private static void CheckSubscribe(string key)
        {
            MySqlConnection conn = new MySqlConnection();
            try
            {
                conn = new MySqlConnection(Properties.Resources.String1);
                conn.Open();

                var com = new MySqlCommand("USE subs; " +
                 "select * from `subs` where keyLic = @keyLic AND subEnd > NOW() AND activeLic = 1 limit 1", conn);
                com.Parameters.AddWithValue("@keyLic", key);

                using (DbDataReader reader = com.ExecuteReader())
                {
                    if (reader.HasRows) //тут уходит на else если нет данных
                    {
                        reader.Read();
                        string dataEnd = reader.GetString(2);
                        Console.WriteLine($"Subscription will end {dataEnd}");
                        reader.Close();
                    }
                    else
                    {
                        conn.Close();
                        Console.WriteLine("[500][SYSTEM] License is not active");
                        Thread.Sleep(5000);
                        Environment.Exit(0);
                    }
                }
                conn.Close();
            }
            catch
            {
                conn.Close();
                Console.WriteLine("[SYSTEM][404] Something went wrong!");
                Thread.Sleep(5000);
                Environment.Exit(0);
            }
            finally
            {
                conn.Close();
            }
        }

        static void Main(string[] args)
        {
            Console.Title = "Close All Steam Process";
            
            try
            {              

                if (File.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}\License.lic")) //File.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}\License.lic")
                {
                    string key = "";
                    using (StreamReader sr = new StreamReader($@"{AppDomain.CurrentDomain.BaseDirectory}\License.lic"))
                    {
                        key = sr.ReadToEnd();
                    }
                    key = key.Replace("\r\n", "");

                    if (PcInfo.GetCurrentPCInfo() == key) //PcInfo.GetCurrentPCInfo() == key
                    {
                        CheckSubscribe(key);

                        int i = 0;
                        foreach (Process process3 in from pr in Process.GetProcesses()
                                                     where pr.ProcessName == "steam"
                                                     select pr)
                        {
                            process3.Kill();
                            i += 1;
                        }

                        Console.WriteLine($"Processes closed {i}");
                        Console.WriteLine("Done");
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

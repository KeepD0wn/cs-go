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
        static void Main(string[] args)
        {
            Console.Title = "Close All Steam Process";
            
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

                    MySqlConnection conn = new MySqlConnection();
                    try
                    {
                        conn = new MySqlConnection(Properties.Resources.String1);
                        conn.Open();

                        var com = new MySqlCommand("USE `MySQL-5846`; " +
                         "select * from `subs` where keyLic = @keyLic AND subEnd > NOW() AND activeLic = 1 limit 1", conn);
                        com.Parameters.AddWithValue("@keyLic", key);

                        using (DbDataReader reader = com.ExecuteReader())
                        {
                            if (reader.HasRows) //тут уходит на else если нет данных
                            {

                            }
                            else
                            {
                                conn.Close();
                                Console.WriteLine("[SYSTEM] License is not active");
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

                    if (PcInfo.GetCurrentPCInfo() == key)
                    {
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

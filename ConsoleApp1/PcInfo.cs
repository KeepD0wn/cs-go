﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class PcInfo
    {
        public static string GetCurrentPCInfo()
        {
            string result = "";
            try
            {
                string final = "";

                String host = System.Net.Dns.GetHostName();
                System.Net.IPAddress ip = System.Net.Dns.GetHostByName(host).AddressList[0];
                string ipAdress = ip.ToString();
                final += ipAdress;

                string userName = Environment.UserName;
                final += userName;

                string for2videocards = "";
                ManagementObjectSearcher searcher11 = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_VideoController");
                foreach (ManagementObject queryObj in searcher11.Get())
                {
                    for2videocards = queryObj["VideoProcessor"].ToString();
                }
                final += for2videocards;

                ManagementObjectSearcher searcher8 = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
                foreach (ManagementObject queryObj in searcher8.Get())
                {
                    final += queryObj["Name"];
                    final += queryObj["NumberOfCores"];
                    final += queryObj["ProcessorId"];
                }

                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                foreach (ManagementObject info in searcher.Get())
                {
                    if (info["DeviceID"].ToString().Contains("PHYSICALDRIVE0"))
                    {
                        final += info["Model"];
                        final += info["InterfaceType"];
                        final += info["SerialNumber"];
                    }
                }

                final = final.Replace(" ", "").Replace("-", "").ToLower();
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] checkSum = md5.ComputeHash(Encoding.UTF8.GetBytes(final));
                result = BitConverter.ToString(checkSum).Replace("-", String.Empty);

                return result;
            }
            catch
            {
                Console.WriteLine("[SYSTEM] Error, try without sandbox");
                Thread.Sleep(5000);
                Environment.Exit(0);
            }
            return result;
        }
    }
}

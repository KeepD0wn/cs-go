using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Kill
{
    class Program
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern UInt32 GetWindowThreadProcessId(IntPtr hwnd, ref Int32 pid);

        static void Main(string[] args)
        {
            while (FindWindow(null, $"Steam Login").ToString() != "0")
            {
                IntPtr steamWindow1 = FindWindow(null, $"Steam Login");
                int k = 0;
                GetWindowThreadProcessId(steamWindow1, ref k);
                Process pleshProc = Process.GetProcessById(k);
                pleshProc.Kill();
            }
            Environment.Exit(0);

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}

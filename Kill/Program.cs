using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kill
{
    class Program
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern UInt32 GetWindowThreadProcessId(IntPtr hwnd, ref Int32 pid);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        static uint MOUSEEVENTF_LEFTDOWN = 0x02;

        static uint MOUSEEVENTF_LEFTUP = 0x04;

        static void Main(string[] args)
        {
            foreach (Process processCS in from pr in Process.GetProcesses()
                                         where pr.ProcessName == "csgo"
                                         select pr)
            {
                processCS.Kill();
                Thread.Sleep(1000);
            }

            foreach (Process processSteam in from pr in Process.GetProcesses()
                                         where pr.ProcessName == "steam"
                                         select pr)
            {
                processSteam.Kill();
                Thread.Sleep(1000);
            }

            int xCs = Convert.ToInt32(args[0]);
            int yCs = Convert.ToInt32(args[1]);
            SetCursorPos(xCs, yCs);
            mouse_event(MOUSEEVENTF_LEFTDOWN, (uint)xCs, (uint)yCs, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, (uint)xCs, (uint)yCs, 0, 0);

            SendKeys.SendWait("{+}+{F10}");

            for (int i = 0; i < 10; i++)
            {
                SendKeys.SendWait("{UP}");
                Thread.Sleep(50);
            }
            SendKeys.SendWait("{ENTER}");

            Thread.Sleep(5000);
            //CSGO IDLE MACHINE.exe
            Process.Start("CSGO IDLE MACHINE.exe", "1");
            Environment.Exit(0);

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}

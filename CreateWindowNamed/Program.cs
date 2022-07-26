using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace CreateWindowNamed
{
    class Program
    {
        [DllImport("User32.dll")]
        public static extern bool SetWindowText(IntPtr hwnd, string title);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern UInt32 GetWindowThreadProcessId(IntPtr hwnd, ref Int32 pid);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        static void Main(string[] args)
        {
            Console.WriteLine("Введите название окна");

            string c = Console.ReadLine();
            Console.Title = c;
            Console.WriteLine("закончили");
            Console.ReadLine();
        }
    }
}

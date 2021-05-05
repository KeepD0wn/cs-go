using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using ConsoleApp1;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using SteamAuth;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Resources;
using System.Reflection;
using MySql.Data.MySqlClient;

namespace ConsoleApp3
{
    class Program
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindow(IntPtr HWnd, GetWindow_Cmd cmd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        [DllImport("user32.dll")]
        static extern int LoadKeyboardLayout(string pwszKLID, uint Flags);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        static extern int PostMessage(IntPtr hWnd, int uMsg, int wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern bool DestroyWindow(IntPtr hWnd);

        //[DllImport("user32.dll", CharSet = CharSet.Auto)]
        //private static extern IntPtr PostMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool SetWindowText(IntPtr hWnd, string text);

        [DllImport("user32.dll")]
        static extern bool SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int showWindowCommand);       

        [DllImport("user32.dll")]
        public static extern UInt32 GetWindowThreadProcessId(IntPtr hwnd, ref Int32 pid);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern void keybd_event(byte bvk, byte bscan, int dwflags, IntPtr dwExtraInfo);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);



        private const UInt32 WM_CLOSE = 0x0010;

        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        public enum GetWindow_Cmd : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6,
            WM_GETTEXT = 0x000D
        }

        public const int V = 0x56; // V key code
        public const int VK_CONTROL = 0x11; //Control key code
        public const int KEYEVENTF_KEYUP = 0x0002; //Key up flag

        const int VK_ENTER = 0x0D;

        const int WM_KEYDOWN = 0x100;

        const int wmChar = 0x0102;

        const uint SWP_NOZORDER = 0x0004;

        private static IntPtr FindGuard(IntPtr steamGuardWindow)
        {
            steamGuardWindow = FindWindow(null, "Steam Guard - Computer Authorization Required");
            if (steamGuardWindow.ToString() != "0")
            {
                Console.WriteLine("Steam Guard detected");
                return steamGuardWindow;
            }

            steamGuardWindow = FindWindow(null, "Steam Guard — Необходима авторизация компьютера");
            if (steamGuardWindow.ToString() != "0")
            {
                Console.WriteLine("Steam Guard detected");
                return steamGuardWindow;
            }

            return IntPtr.Zero;
        }

        private static void TypeText(IntPtr console,IntPtr steamGuardWindow,string str)
        {
            SetForegroundWindow(console);
            Thread.Sleep(50);
            SetForegroundWindow(steamGuardWindow);
            Thread.Sleep(100);
            foreach (char ch in str)
            {
                PostMessage(steamGuardWindow, wmChar, ch, 0);
                Thread.Sleep(100);
            }
            Thread.Sleep(50);
            PostMessage(steamGuardWindow, WM_KEYDOWN, VK_ENTER, 1);
        }

        static object k = new object();
        static int j=0;

        private static void Fo()
        {
            try
            {
                lock (k)
                {
                    Console.WriteLine("Начали");
                    try
                    {
                        j += 1;
                        Thread.Sleep(1000);

                        if (j == 3)
                        {
                            System.Timers.Timer steamGuardTimer = new System.Timers.Timer();
                            steamGuardTimer.Interval = 0;
                            steamGuardTimer.Elapsed += delegate
                            {
                                Console.WriteLine("abort!");
                                throw new Exception("Abort");
                            };
                        }

                        Console.WriteLine("проц " + j);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("vishe");
                    }
                    Console.WriteLine("Закончили");
                }               
            }
            catch
            {

            }                     
              
        }

        private static string GetGuardCode(string secretKey)
        {
            Thread.Sleep(10000);
            SteamGuardAccount acc = new SteamGuardAccount();
            acc.SharedSecret = secretKey;
            string codeGuard = acc.GenerateSteamGuardCode();
            Console.WriteLine(codeGuard);
            return codeGuard;
        }

        private static async Task<string> GetGuardCodeAsync(string secretKey)
        {
            await Task.Delay(3000);
            string s = await Task.Run(() => GetGuardCode(secretKey));            
            return s;
        }

        private static object f21 = new object();

        private static async Task f1()
        {
            await Task.Run(() => {
                lock (f21)
                {
                    DateTime now1 = DateTime.Now;
                    while (now1.AddSeconds(5) > DateTime.Now)
                    {
                        Console.WriteLine("f1");
                        Thread.Sleep(100);
                    }                    
                }                
            });           
        }

        private static async Task f2()
        {
            await Task.Run(() => {
                lock (f21)
                {
                    DateTime now1 = DateTime.Now;
                    while (now1.AddSeconds(5) > DateTime.Now)
                    {
                        Console.WriteLine("f2");
                        Thread.Sleep(100);
                    }
                }
            });
        }

        static UnmanagedMemoryStream GetResourceStream(string resName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var strResources = assembly.GetName().Name + ".g.resources";
            var rStream = assembly.GetManifestResourceStream(strResources);
            var resourceReader = new System.Resources.ResourceReader(rStream);
            var items = resourceReader.OfType<System.Collections.DictionaryEntry>();
            var stream = items.First(x => (x.Key as string) == resName.ToLower()).Value;
            return (UnmanagedMemoryStream)stream;
        }

        private static void SetOnlineZero()
        {
            try
            {
                MySqlConnection conn = DBUtils.GetDBConnection();
                 conn.Open();
                var com = new MySqlCommand("USE csgo; " +
                "Update accounts set isOnline = @online0 where isOnline = @online1", conn);
                com.Parameters.AddWithValue("@online0", 0);
                com.Parameters.AddWithValue("@online1", 1);

                com.ExecuteNonQuery();

                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }            
        }

        static void Main(string[] args)
        {
            //var task1 = f1();
            //var task2 = f2();
            //Console.WriteLine("main");

            //task.Wait();
            //Console.WriteLine(task.Result);
            //Console.WriteLine("da");

            SetOnlineZero();

            //  var file = GetResourceStream(resName);

            //List<Thread> listthread = new List<Thread>();

            //Console.Title = "con1";
            //for (int i = 0; i < 5; i++)
            //{

            //    Thread myThread = new Thread(new ThreadStart(Fo));
            //    myThread.Name = "Поток " + i.ToString();
            //    myThread.Start();


            //    //listthread.Add(thread);
            //}                

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}

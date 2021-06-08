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
using System.Timers;
using System.Management;
using Rijndael256;
using Microsoft.Win32;
using System.Drawing;
using WindowsInput;
using WindowsInput.Native;

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

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);



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
        public const int KEYEVENTF_EXTENDEDKEY = 1;
        private const int KEYEVENTF_KEYUP2 = 2;

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

        private static string GetHiddenConsoleInput()
        {
            StringBuilder input = new StringBuilder();
            while (true)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter) break;
                if (key.Key == ConsoleKey.Backspace && input.Length > 0) input.Remove(input.Length - 1, 1);
                else if (key.Key != ConsoleKey.Backspace) input.Append(key.KeyChar);
            }
            return input.ToString();
        }

        [DllImport("User32.dll")]
        static extern IntPtr GetDC(IntPtr hwnd);       

        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        private static string GetPassword()
        {
            StringBuilder input = new StringBuilder();
            while (true)
            {
                int x = Console.CursorLeft;
                int y = Console.CursorTop;
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                if (key.Key == ConsoleKey.Backspace && input.Length > 0)
                {
                    input.Remove(input.Length - 1, 1);
                    Console.SetCursorPosition(x - 1, y);
                    Console.Write(" ");
                    Console.SetCursorPosition(x - 1, y);
                }
                else if (key.Key != ConsoleKey.Backspace)
                {
                    input.Append(key.KeyChar);
                    Console.Write("*");
                }
            }
            return input.ToString();
        }

        static int i = 10;
        static System.Timers.Timer tmr = new System.Timers.Timer();
        static int timerDelay = 1;

        private static void TmrEvent2(ref bool k, System.Timers.Timer k2) //out сделать, ещё посмотреть как передаются параметры
        {
            k = false;
            k2.Enabled = false;            
        }

        private static void StartCsGo()
        {
            Process process2 = new Process(); //КАК НИ СТРАННО, НО УБИЙСТВО ПЛЕШИВОГО СТИМА РАБОТАЕТ ТОЛЬКО ИЗ НОВОГО ПРОЦЕССА
            ProcessStartInfo processStartInfo1 = new ProcessStartInfo();
            processStartInfo1.WindowStyle = ProcessWindowStyle.Hidden;
            processStartInfo1.FileName = "cmd.exe";
            processStartInfo1.Arguments = string.Format("/C \"{0}\"", new object[]
            {
                                 $@"C:\Program Files (x86)\Steam\Steam.exe"
            }); ;
            process2.StartInfo = processStartInfo1;
            process2.Start();
            Thread.Sleep(5000);
        }

        private static void Start()
        {
            int i = 0;
            while (true)
            {
                //тут синхронизация по mainObj что бы кски не запускались одновременно
                if (Process.GetProcessesByName("steam").Length < 5)
                {
                    StartCsGo();
                    i += 1;
                }
                Console.WriteLine($"Из асинка запущено {i} стимов");
                Thread.Sleep(10000);
            }
            
        }

        private static async Task StartAsync()
        {
            await Task.Run(() => Start());
        }
        
        private static IntPtr primary = GetDC(IntPtr.Zero);

        const int DESKTOPVERTRES = 117;

        const int DESKTOPHORZRES = 118;

        static uint MOUSEEVENTF_LEFTDOWN = 0x02;

        static uint MOUSEEVENTF_LEFTUP = 0x04;

        private static int monitorSizeX = GetDeviceCaps(primary, DESKTOPHORZRES);

        private static int monitorSizeY = GetDeviceCaps(primary, DESKTOPVERTRES);

        private static bool updatingWasFound = false;

        static void CopyDirectory(DirectoryInfo source, DirectoryInfo destination)
        {
            if (!destination.Exists)
            {
                destination.Create();
            }

            if (!source.Exists)
            {
                source.Create();
            }

            // Copy all files.
            FileInfo[] files = source.GetFiles();
            foreach (FileInfo file in files)
            {
                try //если он захочет поменять атрибут у файла которого нет, то и бог с ним
                {
                    DirectoryInfo d = new DirectoryInfo(Path.Combine(destination.FullName, file.Name));
                    if (d.Attributes != FileAttributes.Normal)
                    {
                        File.SetAttributes(d.ToString(), FileAttributes.Normal);
                    }
                }
                catch { }

                file.CopyTo(Path.Combine(destination.FullName,
                    file.Name), true);
            }

            // Process subdirectories.
            DirectoryInfo[] dirs = source.GetDirectories();
            foreach (DirectoryInfo dir in dirs)
            {
                // Get destination directory.
                string destinationDir = Path.Combine(destination.FullName, dir.Name);
                DirectoryInfo k = new DirectoryInfo(destinationDir);
                if (!k.Exists)
                {
                    destination.Create();
                }

                // Call CopyDirectory() recursively.
                CopyDirectory(dir, new DirectoryInfo(destinationDir));
            }
        }

        static void Main(string[] args)
        {
            //  Process.Start("demo.exe", "param1 param2");
            // Console.WriteLine(args[0]);



            //var task1 = f1();
            //var task2 = f2();
            //Console.WriteLine("main");

            //task.Wait();
            //Console.WriteLine(task.Result);
            //Console.WriteLine("da"); 



            Console.Title = "fd1";
            Thread.Sleep(3000);
            IntPtr console = FindWindow(null, "fd1");
            Console.WriteLine(console.ToString());
           // SetForegroundWindow(console);
            IntPtr cs = FindWindow(null, "Counter-Strike: Global Offensive"); //csgo_lwgxbf9lj
            Console.WriteLine(cs.ToString());
            //SetForegroundWindow(cs);
            Thread.Sleep(500);
            //PostMessage(cs, WM_KEYDOWN, 0xC0, 1); //0xC0 это тильда
            //SendKeys.SendWait("+{`}");
            //keybd_event(0xC0, 0, KEYEVENTF_EXTENDEDKEY, 0);
            //keybd_event(0xC0, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);

            SetForegroundWindow(cs);
            keybd_event(0xC0, 0, KEYEVENTF_EXTENDEDKEY, 0);
            Thread.Sleep(100);
            keybd_event(0xC0, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
            //var inputSimulator = new InputSimulator();
            //inputSimulator.Keyboard.KeyDown(VirtualKeyCode.F10);
            //inputSimulator.Keyboard.KeyDown(VirtualKeyCode.F10);

            //--------------------------------------------------------------------------------------------------------------ТУТ находим процесс перед килом
            //IntPtr cs = FindWindow(null, "Counter-Strike: Global Offensive");
            //int steamProcId = 0;
            //GetWindowThreadProcessId(cs, ref steamProcId);
            //Process steamProc = Process.GetProcessById(steamProcId);
            //steamProc.Kill();

            //var selectedTeams = from t in Process.GetProcesses() // определяем каждый объект из teams как t
            //                    where t.ProcessName.Contains("csgo") //фильтрация по критерию
            //                    select t; // выбираем объект
            //                              // where t.MainWindowTitle.Contains("csgo_gmyemqllylxr")


            //Process[] localByName = Process.GetProcessesByName("csgo");
            //foreach (var t in localByName)
            //{
            //    t.Kill();
            //}

            //DateTime waitFor = DateTime.Now.AddSeconds(50);
            //DateTime now = DateTime.Now;
            //--------------------------------------------------------------------------------------------------------------



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

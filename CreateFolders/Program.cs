using System;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SteamAuth;
using System.Threading;
using System.Timers;
using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Threading.Tasks;
using System.IO;
using System.Management;
using System.Security.Cryptography;
using System.Configuration;
using System.Drawing;

namespace CreateFolders
{
    class Program
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern UInt32 GetWindowThreadProcessId(IntPtr hwnd, ref Int32 pid);

        [DllImport("user32.dll")]
        static extern int LoadKeyboardLayout(string pwszKLID, uint Flags);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("User32.dll")]
        static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        static uint MOUSEEVENTF_LEFTDOWN = 0x02;

        static uint MOUSEEVENTF_LEFTUP = 0x04;

        const int VK_ENTER = 0x0D;

        const int WM_KEYDOWN = 0x100;

        const int wmChar = 0x0102;

        const int DESKTOPVERTRES = 117;

        const int DESKTOPHORZRES = 118;

        const uint SWP_NOZORDER = 0x0004;

        private static int consoleX = 380;

        private static int consoleY = 270;

        private static IntPtr primary = GetDC(IntPtr.Zero);

        private static double screenScalingFactor = GetWindowsScreenScalingFactor();

        private static int monitorSizeX = Convert.ToInt32(GetDeviceCaps(primary, DESKTOPHORZRES) / screenScalingFactor);

        private static int monitorSizeY = Convert.ToInt32(GetDeviceCaps(primary, DESKTOPVERTRES) / screenScalingFactor);

        private static int procCount = 0;

        const int VK_BACK = 0x08;

        const uint SWP_NOSIZE = 0x0001;

        public enum DeviceCap
        {
            VERTRES = 10,
            DESKTOPVERTRES = 117
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        public static string ShowSystemInfo()
        {

            string userName = Environment.UserName;
            string videoProcess = default;
            string procName = default;
            string procCores = default;
            string procId = default;

            String host = System.Net.Dns.GetHostName();
            System.Net.IPAddress ip = System.Net.Dns.GetHostByName(host).AddressList[0];
            string ipAdress = ip.ToString();

            ManagementObjectSearcher searcher11 = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_VideoController");

            foreach (ManagementObject queryObj in searcher11.Get())
            {
                videoProcess = queryObj["VideoProcessor"].ToString();
            }

            ManagementObjectSearcher searcher8 = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");

            foreach (ManagementObject queryObj in searcher8.Get())
            {
                procName = queryObj["Name"].ToString();
                procCores = queryObj["NumberOfCores"].ToString();
                procId = queryObj["ProcessorId"].ToString();
            }

            string s = ipAdress + userName + videoProcess + procName + procCores + procId;
            return s;
        }

        private static void LangToEn()
        {
            string lang = "00000409";
            int ret = LoadKeyboardLayout(lang, 1);
            PostMessage(GetForegroundWindow(), 0x50, 1, ret);
        }

        private static void TypeText(IntPtr console, IntPtr steamGuardWindow, string str)
        {
            LangToEn();
            //Thread.Sleep(500); //когда проц загружен нужен делей
            //SetForegroundWindow(console);
            //Thread.Sleep(100);
            SetForegroundWindow(steamGuardWindow);
            Thread.Sleep(1000);
            foreach (char ch in str)
            {
                PostMessage(steamGuardWindow, wmChar, ch, 0);
                Thread.Sleep(200);
            }
            Thread.Sleep(500);
            //PostMessage(targer, WM_KEYDOWN, VK_ENTER, 1);
            //Thread.Sleep(100);
            SetForegroundWindow(console);
        }

        private static string GetGuardCode(string secretKey)
        {
            SteamGuardAccount acc = new SteamGuardAccount();
            acc.SharedSecret = secretKey;
            string codeGuard = acc.GenerateSteamGuardCode();
            return codeGuard;
        }

        private static async Task<string> GetGuardCodeAsync(string secretKey)
        {
            string s = await Task.Run(() => GetGuardCode(secretKey));
            return s;
        }
        private static IntPtr FindGuard() //, System.Timers.Timer steamGuardTimer
        {
            IntPtr steamGuardWindow;
            steamGuardWindow = FindWindow(null, "Steam Guard - Computer Authorization Required");
            if (steamGuardWindow.ToString() != "0")
            {
                Console.WriteLine("[SYSTEM] Steam Guard detected");
                return steamGuardWindow;
            }

            steamGuardWindow = FindWindow(null, "Steam Guard — Необходима авторизация компьютера");
            if (steamGuardWindow.ToString() != "0")
            {
                Console.WriteLine("[SYSTEM] Steam Guard detected");
                return steamGuardWindow;
            }

            return IntPtr.Zero;
        }

        private static void CheckGuardClosed(IntPtr steamGuardWindow, Process steamProc, IntPtr console, string codeGuard)
        {
            Thread.Sleep(4000);
            IntPtr newGuard = FindGuard(); //новое окно с дефолтным именем, а если энтр не нажат, то имя гвар_айди и не детектит тогда
            if (newGuard.ToString() != "0")
            {
                Thread.Sleep(1000);
                lock (threadLockType)
                {
                    TypeText(console, newGuard, codeGuard);
                }

                Thread.Sleep(3000);
                IntPtr newGuard1 = FindGuard();
                if (newGuard1.ToString() != "0")
                {
                    Console.WriteLine("Steam Guard still on");
                    Console.WriteLine(new string('-', 25));
                    steamProc.Kill();
                    Thread.Sleep(1000);
                    throw new Exception("Abort");
                }
            }

            IntPtr steamWarning = FindWindow(null, "Steam - Warning");
            if (steamWarning.ToString() != "0")
            {
                Console.WriteLine("[SYSTEM] Steam Warning");
                Console.WriteLine(new string('-', 25));
                steamProc.Kill();
                Thread.Sleep(1000);
                throw new Exception("Abort");
            }
        }
        private static void CheckTimeSteam(ref bool k, System.Timers.Timer timer)
        {
            k = true;
            timer.Enabled = false;
        }

        private static void StartSteam(int currentCycle, int lastCycle)
        {
            try
            {
                string login = "";

                string password = "";

                string secretKey = "";
                int steamProcId = 0;

                int accid = 0;

                try
                {
                    conn.Open();
                    var com = new MySqlCommand("USE csgo; " +
                        "select * from accounts where  folderCreated = 0 limit 1", conn);

                    using (DbDataReader reader = com.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                accid = Convert.ToInt32(reader.GetString(0));
                                Console.WriteLine($"ID: {accid}");

                                login = reader.GetString(1);
                                Console.WriteLine($"Login: {login}");

                                password = reader.GetString(2);
                                secretKey = reader.GetString(3);
                            }
                        }
                        else
                        {
                            throw new Exception("[SYSTEM] No suitable data");
                        }
                    }
                    conn.Close();
                }
                catch (Exception ex)
                {
                    conn.Close();
                    Console.WriteLine(ex.Message);
                    if (ex.Message == "[SYSTEM] No suitable data")
                    {
                        Thread.Sleep(10000);
                        Environment.Exit(0);
                    }
                }
                Process steamProc = new Process();
                Process process = new Process();
                ProcessStartInfo processStartInfo = new ProcessStartInfo();

                processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processStartInfo.FileName = "cmd.exe";
                processStartInfo.Arguments = string.Format("/C \"{0}\" -noverifyfiles -noreactlogin -login {1} {2} ", new object[]
                {
                       @"C:\Program Files (x86)\Steam\steam.exe",
                       login,
                       password,

                });

                process.StartInfo = processStartInfo;
                process.Start();

                IntPtr steamWindow = new IntPtr();
                IntPtr console = FindWindow(null, "ConsoleCsgo");

                bool timeIsOverSteam = false;
                System.Timers.Timer tmrSteam = new System.Timers.Timer();
                tmrSteam.Interval = 1000 * 40;
                tmrSteam.Elapsed += (o, e) => CheckTimeSteam(ref timeIsOverSteam, tmrSteam);
                tmrSteam.Enabled = true;

                while (true)
                {
                    steamWindow = FindWindow(null, "Steam Sign In");
                    if (steamWindow.ToString() != "0")
                    {
                        Console.WriteLine("[SYSTEM] Steam detected");
                        Thread.Sleep(500);
                        GetWindowThreadProcessId(steamWindow, ref steamProcId);
                        steamProc = Process.GetProcessById(steamProcId);
                        break;
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }

                    if (timeIsOverSteam == true)
                    {
                        try
                        { //TODO: узнать тот ли стим убивает
                            int x = 0;
                            GetWindowThreadProcessId(steamWindow, ref x);
                            Process steamProcS = Process.GetProcessById(x);
                            //listSteamLogin.Remove(steamProcS.MainWindowHandle.ToString());
                            Console.WriteLine("[911] Error");
                            steamProcS.Kill();
                        }
                        catch
                        {
                            Console.WriteLine("[219][SYSTEM] Error");
                        }
                        Console.WriteLine("[SYSTEM] No Steam detected");
                        Thread.Sleep(1000);
                        throw new Exception("Abort");
                    }

                    Thread.Sleep(100);
                }

                //var codeGuardTask = GetGuardCodeAsync(secretKey); 
                //codeGuardTask.Wait();
                var codeGuardTask = GetGuardCode(secretKey);
                Console.WriteLine($"Guard code: {codeGuardTask}");


                bool guardWasDetected = false;
                DateTime now1 = DateTime.Now;
                //именно столько секунд даёт на прогрузку после гварда или когда ошибка expired. Из минусов столько ждать если неправильный пароль 
                while (now1.AddSeconds(60) > DateTime.Now)
                {
                    if (FindWindow(null, $"Steam Sign In").ToString() != "0") //FindWindow(null, $"steam_{login}").ToString() != "0"
                    {
                        guardWasDetected = true;
                        lock (threadLockType)
                        {
                            SetForegroundWindow(steamWindow);
                            Thread.Sleep(1000);
                            PostMessage(steamWindow, WM_KEYDOWN, VK_BACK, 1);
                            Thread.Sleep(200);
                            PostMessage(steamWindow, WM_KEYDOWN, VK_BACK, 1);
                            Thread.Sleep(200);
                            PostMessage(steamWindow, WM_KEYDOWN, VK_BACK, 1);
                            Thread.Sleep(200);
                            PostMessage(steamWindow, WM_KEYDOWN, VK_BACK, 1);
                            Thread.Sleep(200);
                            PostMessage(steamWindow, WM_KEYDOWN, VK_BACK, 1);
                            Thread.Sleep(200);
                            TypeText(console, steamWindow, codeGuardTask);
                        }
                    }
                    //если окно стим гварда(логина) было найдено и сейчас уже закрылось. Бывает ошибка и долго висит, пока не появится стим табличка с началом запуска кс
                    // так что надо ждать пока появится это окно, что бы ошибка закрылось и код пошёл дальше
                    if (FindWindow(null, $"Steam Sign In").ToString() == "0" && guardWasDetected == true) //FindWindow(null, $"steam_{login}").ToString()
                    {
                        Console.WriteLine("[SYSTEM] Guard was successfully completed");
                        Thread.Sleep(3000);
                        break;
                    }

                    Thread.Sleep(1000);
                }


                //ну тут понятно если не было найдено стима переименованное
                if (guardWasDetected == false)
                {
                    steamProc.Kill(); // если процесс подвисает на время загрузки гварда, никак не убить
                                      //listSteam.Remove(steamProc);
                    Console.WriteLine("[SYSTEM] Cant find Guard window");
                    Thread.Sleep(1000);
                    throw new Exception("Abort");
                }

                // тут если гвард был раньше найден, потом закрылся (условия прохождения while выше). А сейчас опять открыт
                if (guardWasDetected == true && FindWindow(null, $"Steam Sign In").ToString() != "0")
                {
                    steamProc.Kill(); // если процесс подвисает на время загрузки гварда, никак не убить
                                      //listSteam.Remove(steamProc);
                    Console.WriteLine("[SYSTEM] Cant skip Guard window");
                    Thread.Sleep(1000);
                    throw new Exception("Abort");
                }



                //CheckGuardClosed(steamWindow, steamProc, console, codeGuardTask.Result); // мб разделить проверку на стим гвард клозед и на стим варнинг, варнинг сделать таском и на секунд 15

                while (true)
                {
                    //таймер меняет переменную, тут делаем иф, если проходит внутрь, то аборт
                    steamWindow = FindWindow(null, "Steam");
                    if (steamWindow.ToString() != "0")
                    {
                        Thread.Sleep(1000);
                        IntPtr steamSubscriber = FindWindow(null, "Steam Subscriber Agreement");
                        if (steamSubscriber.ToString() != "0")
                        {
                            SetWindowPos(steamSubscriber, IntPtr.Zero, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
                            Thread.Sleep(500);
                            SetForegroundWindow(steamSubscriber);
                            Thread.Sleep(200);
                            int xSub = 295;
                            int ySub = 490;
                            SetCursorPos(xSub, ySub);
                            mouse_event(MOUSEEVENTF_LEFTDOWN, (uint)xSub, (uint)ySub, 0, 0);
                            mouse_event(MOUSEEVENTF_LEFTUP, (uint)xSub, (uint)ySub, 0, 0);
                            Console.WriteLine("[SYSTEM] Steam Subscriber Agreement detected");
                        }
                        Thread.Sleep(500);
                        Console.WriteLine("[SYSTEM] Steam detected");
                        Console.WriteLine(new string('-', 20) + $"Current window: {currentCycle}/{lastCycle}");
                        steamProc.Kill();
                        Thread.Sleep(500);
                        break;
                    }
                    Thread.Sleep(100);
                }

                try
                {
                    conn.Open();
                    var com = new MySqlCommand("USE csgo; " +
                    "Update accounts set folderCreated = @folderCreated where id = @id", conn);
                    com.Parameters.AddWithValue("@folderCreated", 1);
                    com.Parameters.AddWithValue("@id", accid);
                    int rowCount = com.ExecuteNonQuery();
                    procCount += 1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    conn.Close();
                }
                Thread.Sleep(1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(new string('-', 25));
            }
        }

        static double GetWindowsScreenScalingFactor(bool percentage = true)
        {
            //Create Graphics object from the current windows handle
            Graphics GraphicsObject = Graphics.FromHwnd(IntPtr.Zero);
            //Get Handle to the device context associated with this Graphics object
            IntPtr DeviceContextHandle = GraphicsObject.GetHdc();
            //Call GetDeviceCaps with the Handle to retrieve the Screen Height
            int LogicalScreenHeight = GetDeviceCaps(DeviceContextHandle, (int)DeviceCap.VERTRES);
            int PhysicalScreenHeight = GetDeviceCaps(DeviceContextHandle, (int)DeviceCap.DESKTOPVERTRES);
            //Divide the Screen Heights to get the scaling factor and round it to two decimals
            double ScreenScalingFactor = Math.Round((double)PhysicalScreenHeight / (double)LogicalScreenHeight, 2);
            //If requested as percentage - convert it
            if (percentage)
            {
                ScreenScalingFactor *= 100.0;
            }
            //Release the Handle and Dispose of the GraphicsObject object
            GraphicsObject.ReleaseHdc(DeviceContextHandle);
            GraphicsObject.Dispose();
            //Return the Scaling Factor
            return ScreenScalingFactor / 100;
        }

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

        private static MySqlConnection conn = DBUtils.GetDBConnection();

        private static object threadLockType = new object();

        static void Main(string[] args)
        {

            Console.Title = "Create CSGO Folders";
            Thread.Sleep(100);
            IntPtr conWindow = FindWindow(null, "Create CSGO Folders");
            SetWindowPos(conWindow, IntPtr.Zero, monitorSizeX - consoleX, monitorSizeY - consoleY - 40, consoleX, consoleY, SWP_NOZORDER); //вылазит за экран если размер элементов больше 100%	
            SetForegroundWindow(conWindow);

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

                        Console.WriteLine("How many Steam accounts to log in: ");
                        int count = Convert.ToInt32(Console.ReadLine());

                        while (procCount < count)
                        {
                            Thread myThread = new Thread(delegate () { StartSteam(procCount + 1, count); });
                            myThread.Start();
                            myThread.Join();
                        }
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }
    }
}
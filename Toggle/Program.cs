using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Toggle
{
    class Program
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [DllImport("User32.dll")]
        public static extern bool SetWindowText(IntPtr hwnd, string title);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        static uint MOUSEEVENTF_LEFTDOWN = 0x02;

        static uint MOUSEEVENTF_LEFTUP = 0x04;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern UInt32 GetWindowThreadProcessId(IntPtr hwnd, ref Int32 pid);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private static void CloseAllProcess()
        {
            foreach (Process processCS in from pr in Process.GetProcesses()
                                          where pr.ProcessName == "csgo"
                                          select pr)
            {
                processCS.Kill();
                LogAsync(processCS.MainWindowTitle + " csgo killed");
                Thread.Sleep(15000);
            }

            foreach (Process processSteam in from pr in Process.GetProcesses()
                                             where pr.ProcessName == "steam"
                                             select pr)
            {
                processSteam.Kill();
                LogAsync(processSteam.MainWindowTitle + " steam killed");
                Thread.Sleep(15000);
            }
        }

        /// <summary>
        /// меняет песок на ярылыке на обратный и запускает кс в зависимости от параметра кол-во
        /// </summary>
        /// <param name="count"></param>
        public static void StartCS(int count)
        {
            Thread.Sleep(60000);
            CloseAllProcess();
            Thread.Sleep(10000);

            string textFromFile = "";
            using (FileStream fstream = File.OpenRead($@"{AppDomain.CurrentDomain.BaseDirectory}\position.txt"))
            {
                // преобразуем строку в байты
                byte[] array = new byte[fstream.Length];
                // считываем данные
                fstream.Read(array, 0, array.Length);
                // декодируем байты в строку
                textFromFile = System.Text.Encoding.Default.GetString(array);
            }

            string[] subs = textFromFile.Split(' ');

            int xCs = Convert.ToInt32(subs[0]);
            int yCs = Convert.ToInt32(subs[1]);
            
            SetCursorPos(0, 0);
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);

            Thread.Sleep(100);
            SetCursorPos(xCs, yCs);
            mouse_event(MOUSEEVENTF_LEFTDOWN, (uint)xCs, (uint)yCs, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, (uint)xCs, (uint)yCs, 0, 0);

            Thread.Sleep(500);
            SendKeys.SendWait("{+}+{F10}");
            Thread.Sleep(1000);

            for (int i = 0; i < 10; i++)
            {
                SendKeys.SendWait("{UP}");
                Thread.Sleep(200);
            }
            Thread.Sleep(200);
            SendKeys.SendWait("{ENTER}");
            LogAsync("toggled sandbox");

            Thread.Sleep(1000);
            Process.Start("CSGO IDLE MACHINE.exe", count.ToString());
            LogAsync($"start idle machine({count})");
        }

        static bool updatingWasFound = false;

        static bool updateIsEnd = false;

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

        public static void UpdateServer()
        {
            Process processServerUpd = new Process();
            ProcessStartInfo processStartInfo = new ProcessStartInfo();

            processStartInfo.WindowStyle = ProcessWindowStyle.Normal;
            processStartInfo.FileName = "cmd.exe";
            processStartInfo.UseShellExecute = true;
            processStartInfo.Arguments = string.Format("/C \"{0}\"", new object[]
            {
                                        $@"C:\Users\{Environment.UserName}\Desktop\update_csgo.lnk"
            });

            processServerUpd.StartInfo = processStartInfo;
            processServerUpd.Start();
            Thread.Sleep(1000); //100мс хватает, но поставлю с запасом
            SetWindowText(processServerUpd.MainWindowHandle, "update_csgo");
            Thread.Sleep(100);

            bool updStarted = false;
            IntPtr updCS = FindWindow(null, "update_csgo");
            DateTime now = DateTime.Now;
            while (now.AddSeconds(10) > DateTime.Now)
            {
                if (updCS.ToString() != "0")
                {
                    updStarted = true;
                    break;
                }
            }
            Thread.Sleep(1000);

            if (updStarted == true)
            {
                while (true)
                {
                    IntPtr updCSEnd = FindWindow(null, "update_csgo");
                    if (updCSEnd.ToString() == "0")
                    {
                        break;
                    }
                    Thread.Sleep(1000);
                }
            }
            Thread.Sleep(1000);
        }
        public static void StartServer()
        {
            Process processServerUpd = new Process();
            ProcessStartInfo processStartInfo = new ProcessStartInfo();

            processStartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            processStartInfo.FileName = "cmd.exe";
            processStartInfo.UseShellExecute = true;
            processStartInfo.Arguments = string.Format("/C \"{0}\"", new object[]
            {
                                        $@"C:\Users\{Environment.UserName}\Desktop\start_server.lnk"
            });

            processServerUpd.StartInfo = processStartInfo;
            processServerUpd.Start();
            Thread.Sleep(1000); //100мс хватает, но поставлю с запасом
            SetWindowText(processServerUpd.MainWindowHandle, "start_server");
            Thread.Sleep(100);

            bool serverStarted = false;
            IntPtr server = FindWindow(null, "start_server");
            DateTime now = DateTime.Now;
            while (now.AddSeconds(10) > DateTime.Now)
            {
                if (server.ToString() != "0")
                {
                    serverStarted = true;
                    break;
                }
            }
            Thread.Sleep(1000);

            if (serverStarted == true)
            {
                while (true)
                {
                    IntPtr mainServer = FindWindow(null, "Counter-Strike: Global Offensive");
                    if (mainServer.ToString() != "0")
                    {
                        Thread.Sleep(10000);
                        ShowWindow(mainServer, 6);
                        break;
                    }
                    Thread.Sleep(1000);
                }
            }
        }

        public static void CloseOldServer()
        {
            try
            {
                while (FindWindow(null, "start_server").ToString() != "0")
                {
                    int idleInt = 0;
                    GetWindowThreadProcessId(FindWindow(null, "start_server"), ref idleInt);
                    Process idleProc = Process.GetProcessById(idleInt);                    
                    idleProc.Kill();
                    LogAsync("Old start window server killed");
                }
            }
            catch 
            {
                LogAsync("Exception while old start server killing");
            }

            try
            {
                while (FindWindow(null, "csgo_server").ToString() != "0")
                {
                    int idleInt = 0;
                    GetWindowThreadProcessId(FindWindow(null, "csgo_server"), ref idleInt);
                    Process idleProc = Process.GetProcessById(idleInt);
                    idleProc.Kill();
                    LogAsync("Old server killed");
                }
            }
            catch
            {
                LogAsync("Exception while old server killing");
            }
            Thread.Sleep(1000);
        }

        private static object logLocker = new object();

        private static void Log(string message)
        {
            try
            {
                lock (logLocker)
                {
                    try
                    {
                        StreamWriter connObj = new StreamWriter("log.txt", true);
                        connObj.WriteLine("[Toggle] " + message + " " + DateTime.Now);
                        connObj.Close();
                    }
                    catch
                    {
                        Thread.Sleep(1000);
                        StreamWriter connObj = new StreamWriter("log.txt", true);
                        connObj.WriteLine("[Toggle] "+message + " " + DateTime.Now);
                        connObj.Close();
                    }
                   
                }
            }
            catch { }
        }

        private static async Task LogAsync(string message)
        {
            await Task.Run(() => Log(message));
        }

        private static async Task LogAndConsoleWritelineAsync(string message)
        {
            Console.WriteLine(message);
            await Task.Run(() => Log(message));
        }

        static void Main(string[] args)
        {
            Console.Title = "Toggle";
            CloseOldServer();
            StartCS(1);
            int countDone = 0;
            try
            {
                countDone = Convert.ToInt32(args[0]);
            }
            catch
            {
                Environment.Exit(0);
            }
                    
            
            while (true)
            {
                Process[] csProcArray = Process.GetProcessesByName("csgo");
                IntPtr ready = FindWindow(null, $"Ready - Counter-Strike: Global Offensive");
                LogAsync("waiting cs ready window");
                if (ready.ToString() != "0" || csProcArray.Length != 0)
                {
                    Thread.Sleep(5000);
                    LogAsync("window cs ready or cs window found");
                    IntPtr idle = FindWindow(null, "CSGO_IDLE_MACHINE");
                    if (idle.ToString() != "0")
                    {
                        int idleInt = 0;
                        GetWindowThreadProcessId(idle, ref idleInt);
                        Process idleProc = Process.GetProcessById(idleInt);
                        try
                        {
                            idleProc.Kill();
                            LogAsync("idle machine killed");
                        }
                        catch { LogAsync("exception while idle machine killing"); }
                    }
                    Thread.Sleep(5000);

                    CloseAllProcess();

                    //тут замена папок панорамы 
                    string directoria = $@"{AppDomain.CurrentDomain.BaseDirectory}\csgoSettings\panorama\videos";
                    string kuda = @"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive\csgo\panorama\videos";
                    Directory.Delete(kuda, true); //true - если директория не пуста удаляем все ее содержимое
                    CopyDirectory(new DirectoryInfo(directoria), new DirectoryInfo(kuda));
                    LogAsync("panorama files changed");

                    try
                    {
                        LogAsync("Started server update");
                        Console.WriteLine("Started server update");
                        UpdateServer();
                    }
                    catch { }
                    try
                    {
                        LogAsync("Start server");
                        Console.WriteLine("Start server");
                        StartServer();
                    }
                    catch { }    

                    StartCS(countDone); //тут сразу открывается новый идл машин, а старый не успевает подождатьь 2 минуты
                    break;
                }

                Thread.Sleep(5000);
            }

            Environment.Exit(0);
        }
    }
}

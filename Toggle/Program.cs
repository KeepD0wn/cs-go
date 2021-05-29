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

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        static uint MOUSEEVENTF_LEFTDOWN = 0x02;

        static uint MOUSEEVENTF_LEFTUP = 0x04;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern UInt32 GetWindowThreadProcessId(IntPtr hwnd, ref Int32 pid);

        private static void CloseAllProcess()
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
        }

        /// <summary>
        /// меняет песок на ярылыке на обратный и запускает кс в зависимости от параметра кол-во
        /// </summary>
        /// <param name="count"></param>
        public static void StartCS(int count)
        {
            CloseAllProcess();

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

            SendKeys.SendWait("{+}+{F10}");

            for (int i = 0; i < 10; i++)
            {
                SendKeys.SendWait("{UP}");
                Thread.Sleep(50);
            }
            SendKeys.SendWait("{ENTER}");

            Thread.Sleep(5000);
            Process.Start("CSGO IDLE MACHINE.exe", count.ToString());
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

        static void Main(string[] args)
        {
            Console.Title = "Toggle";
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
                IntPtr ready = FindWindow(null, $"Ready - Counter-Strike: Global Offensive");
               // IntPtr ready = FindWindow(null, "Updating Counter-Strike: Global Offensive");
                if (ready.ToString() != "0")
                {
                    Thread.Sleep(5000);
                    IntPtr idle = FindWindow(null, "CSGO_IDLE_MACHINE");
                    if (idle.ToString() != "0")
                    {
                        int idleInt = 0;
                        GetWindowThreadProcessId(idle, ref idleInt);
                        Process idleProc = Process.GetProcessById(idleInt);
                        try
                        {
                            idleProc.Kill();
                        }
                        catch { }
                    }
                    Thread.Sleep(5000);

                    CloseAllProcess();

                    //тут замена папок панорамы 
                    string directoria = $@"{AppDomain.CurrentDomain.BaseDirectory}\csgoSettings\panorama\videos";
                    string kuda = @"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive\csgo\panorama\videos";
                    Directory.Delete(kuda, true); //true - если директория не пуста удаляем все ее содержимое
                    CopyDirectory(new DirectoryInfo(directoria), new DirectoryInfo(kuda));

                    StartCS(countDone); //тут сразу открывается новый идл машин, а старый не успевает подождатьь 2 минуты
                    break;
                }

                Thread.Sleep(5000);
            }

            Environment.Exit(0);
        }
    }
}

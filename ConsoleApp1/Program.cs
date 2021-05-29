using System;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SteamAuth;
using System.Threading;
using System.Windows.Forms;
using System.Timers;
using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Configuration;

namespace ConsoleApp1
{
	class Program
	{
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

		private static string def = "silent -nofriendsui -nochatui -single_core -novid -noshader -nofbo -nodcaudio -nomsaa -16bpp -nosound -high";
		
		private static string parsnew = "-silent -nofriendsui -nochatui -single_core -novid -noshader -nofbo -nodcaudio -nomsaa -16bpp -nosound -high";
		
		private static string V2 = "-window -32bit +mat_disable_bloom 1 +func_break_max_pieces 0 +r_drawparticles 0 -nosync -console -noipx -nojoy +exec autoexec.cfg -nocrashdialog -high -d3d9ex -noforcemparms -noaafonts" +
			" -noforcemaccel -limitvsconst +r_dynamic 0 -noforcemspd +fps_max 30 -nopreload -nopreloadmodels +cl_forcepreload 0 " +
			"-nosound -novid -w 640 -h 480 "; //крайне важен пробел в конце		меньше чеи 640х480 нельзя, иначе кску крашит

		private static string serverConnection = "";

		public static string csgopath = "D:\\Games\\steamapps\\common\\Counter-Strike Global Offensive";

		private static object connObj = new object();

		private static void SetOnline(int isOnline, int id)
		{
			lock (connObj)
			{
				try
				{
					conn.Open();

					var com = new MySqlCommand("USE csgo; " +
					"Update accounts set isOnline = @isOnline where id = @id", conn);
					com.Parameters.AddWithValue("@isOnline", isOnline);
					com.Parameters.AddWithValue("@id", id);
					int rowCount = com.ExecuteNonQuery();
					conn.Close();
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
				finally
				{
					conn.Close();
				}
			}
		}

		private static void langToEn()
		{
			string lang = "00000409";
			int ret = LoadKeyboardLayout(lang, 1);
			PostMessage(GetForegroundWindow(), 0x50, 1, ret);
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
		public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

		[System.Runtime.InteropServices.DllImport("user32.dll")]
		static extern bool SetCursorPos(int x, int y);

		[DllImport("user32.dll")]
		static extern int LoadKeyboardLayout(string pwszKLID, uint Flags);

		[DllImport("user32.dll")]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern bool PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);		

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("USER32.DLL")]
		public static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		static extern bool SetWindowText(IntPtr hWnd, string text);

		[DllImport("user32.dll")]
		public static extern UInt32 GetWindowThreadProcessId(IntPtr hwnd, ref Int32 pid);

		[DllImport("user32.dll", SetLastError = true)]
		static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

		[DllImport("User32.dll")]
		static extern IntPtr GetDC(IntPtr hwnd);

		[DllImport("gdi32.dll")]
		static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

		const int VK_ENTER = 0x0D;

		const int WM_KEYDOWN = 0x100;

		const int wmChar = 0x0102;

		const uint SWP_NOZORDER = 0x0004;

		const int DESKTOPVERTRES = 117;

		const int DESKTOPHORZRES = 118;

		const uint SWP_NOSIZE = 0x0001;

		private static object mainObj = new object();

		private static object threadLockType = new object();

		private static int windowCount = 0;

		private static int windowInARow = 0;

		private static int xOffset = 0;

		private static int yOffset = 0;

		private static int xSize = 160; //всё равно ставит своё разрешение

		private static int ySize = 160; //всё равно ставит своё разрешение

		private static MySqlConnection conn = DBUtils.GetDBConnection();

		private static int processStarted = 0;

		private static List<Process> listCsgo = new List<Process>();

		private static List<Process> listSteam = new List<Process>();

		private static int timeIdle = 12300000; //205 минут 12600000;

		private static int consoleX = 380;
		
		private static int consoleY = 270;

		private static int currentCycle = 0;

		private static IntPtr primary = GetDC(IntPtr.Zero);

		private static int monitorSizeX = GetDeviceCaps(primary, DESKTOPHORZRES);

		private static int monitorSizeY = GetDeviceCaps(primary, DESKTOPVERTRES);

		private static int maxWindowInARow = monitorSizeX / xSize;

		static int minToNewCycle = timeIdle / 60000;

		static uint MOUSEEVENTF_LEFTDOWN = 0x02;

		static uint MOUSEEVENTF_LEFTUP = 0x04;

		static System.Timers.Timer tmr = new System.Timers.Timer();

		static int timerDelayInMins = 1;

		static int timerDelayInSeconds = timerDelayInMins * 1000 * 60;

		static List<string> listSteamLogin = new List<string>();

		static int exceptionsInARow = 0;

		private static bool updatingWasFound = false;

		private static void TmrEvent(object sender, ElapsedEventArgs e)
		{
			minToNewCycle -= timerDelayInMins;
			Console.ForegroundColor = ConsoleColor.DarkGreen; // устанавливаем цвет	
			Console.WriteLine($"[SYSTEM] New cycle after: {minToNewCycle} minutes" +"   "); //пробелы что бы инфа от старой строки не осталось, но не слишком много, а то заедет некст строка		
			Console.ResetColor(); // сбрасываем в стандартный

			if (minToNewCycle <= 1)
			{
				tmr.Enabled = false;
			}
		}

		private static void CheckTime(ref bool k, System.Timers.Timer timer)
		{
			k = true;
			timer.Enabled = false;			
		}

		private static void CheckTimeSteam(ref bool k, System.Timers.Timer timer)
		{
			k = true;
			timer.Enabled = false;
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

		private static void CheckGuardClosed(IntPtr steamGuardWindow, Process steamProc, IntPtr console, int accid, string codeGuard)
		{
			Thread.Sleep(8000);
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
					Console.WriteLine(new string('-', 35));
					//try
					//{
					//	listSteamLogin.Remove(steamProc.MainWindowHandle.ToString());
					//}
					//catch { }
					try
                    {
						steamProc.Kill(); //TODO: проверять существует ли 
					}
                    catch 
					{
						Console.WriteLine("[201][SYSTEM] Error");
					}
					//listSteam.Remove(steamProc);
					exceptionsInARow += 1;
					Thread.Sleep(1000);
					throw new Exception("Abort");
				}
			}

			IntPtr steamWarning = FindWindow(null, "Steam-Warning"); //тут убрали пробелы, тк он не детектит
			if (steamWarning.ToString() != "0")
			{
				Console.WriteLine("[SYSTEM] Steam Warning");
				Console.WriteLine(new string('-', 35));
				//try
				//{
				//	listSteamLogin.Remove(steamProc.MainWindowHandle.ToString());
				//}
				//catch { }
				try
				{
					steamProc.Kill(); //TODO: проверять существует ли 
				}
				catch
				{
					Console.WriteLine("[202][SYSTEM] Error");
				}
				//listSteam.Remove(steamProc);
				exceptionsInARow += 1;
				Thread.Sleep(1000);
				throw new Exception("Abort");
			}
		}

		private static async Task CheckGuardClosedAsync(IntPtr steamGuardWindow, Process steamProc, IntPtr console, int accid, string codeGuard)
		{
			await Task.Run(() => CheckGuardClosed(steamGuardWindow, steamProc, console, accid, codeGuard));
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

		private static void SetCsgoPos(IntPtr csgoWindow, int xOffset, int yOffset,string login)
		{
			SetWindowPos(csgoWindow, IntPtr.Zero, xOffset, yOffset, xSize, ySize,SWP_NOSIZE | SWP_NOZORDER); //пусть сам выставляет свои 300 по высоте
		}

		private static async Task SetCsgoPosAsync(IntPtr csgoWindow, int xOffset1, int yOffset1, string login)
		{
			windowCount += 1;
			windowInARow += 1;
			if (windowInARow >= maxWindowInARow)
			{
				windowInARow = 0;
				yOffset += ySize;
			}
			xOffset = xSize * windowInARow;
			await Task.Run(() => SetCsgoPos(csgoWindow, xOffset1, yOffset1,login));
		}

		private static void TypeInCsgo(Process steamProc, string login, int accid, IntPtr console, int xOff, int yOff)
		{
			IntPtr csgoWin = FindWindow(null, $"csgo_{login}");
			if (csgoWin.ToString() != "0")
			{
				lock (threadLockType) //тут снимаем фокус с кски
				{					
					Thread.Sleep(100);
					//SetForegroundWindow(csgoWin);
					int xCs = xOff + 40; //+ отступ в зависимости от окна
					int yCs = yOff + 160;
					int x = monitorSizeX - 265;
					int y = monitorSizeY - 300;

					//клик по окну
					SetCursorPos(xCs, yCs);
					mouse_event(MOUSEEVENTF_LEFTDOWN, (uint)xCs, (uint)yCs, 0, 0);
					mouse_event(MOUSEEVENTF_LEFTUP, (uint)xCs, (uint)yCs, 0, 0);
					Thread.Sleep(500);

					//потом 1 по консоли
					SetForegroundWindow(console);
					SetCursorPos(x, y);
					mouse_event(MOUSEEVENTF_LEFTDOWN, (uint)x, (uint)y, 0, 0);
					mouse_event(MOUSEEVENTF_LEFTUP, (uint)x, (uint)y, 0, 0);
				
					//тут уже вписывает коннект в консоль
					Thread.Sleep(500);
					string setSize = $"mat_setvideomode {xSize} {ySize} 1";
					foreach (char ch in setSize)
					{
						PostMessage(csgoWin, wmChar, ch, 0);
						Thread.Sleep(50);
					}
					PostMessage(csgoWin, WM_KEYDOWN, VK_ENTER, 1);

					Thread.Sleep(500);
					langToEn();
					foreach (char ch in serverConnection)
					{
						PostMessage(csgoWin, wmChar, ch, 0);
						Thread.Sleep(50);
					}
					Thread.Sleep(500);
					PostMessage(csgoWin, WM_KEYDOWN, VK_ENTER, 1);
					Thread.Sleep(500);
				}				

			}
		}

		private static async Task TypeInCsgoAsync(Process steamProc, Process csgoProc, string login, int accid, IntPtr console, int xOff,int yOff)
		{
			await Task.Delay(100000);
			Task t = Task.Run(() => TypeInCsgo(steamProc, login, accid, console, xOff, yOff));
            System.Timers.Timer timer = new System.Timers.Timer(timeIdle);
			timer.Elapsed += (o, e) => KillCsSteam(steamProc, csgoProc, accid, login);
            timer.AutoReset = false;
            timer.Enabled = true;
        }

		private static void KillCsSteam(Process steamProc, Process csgoProc, int accid, string login)
		{
			try
			{
				bool csWasActiveBeforClosing = true;
                try //на всякий в трай, без обработки
                {
					csgoProc.Kill();
					//listCsgo.Remove(csgoProc);
					Thread.Sleep(2000);
				}
                catch
				{
					csWasActiveBeforClosing = false;
				}

				try
				{
					steamProc.Kill();
					//listSteam.Remove(steamProc);
				}
				catch { }
				
				//Console.WriteLine($"[{login}] was killed");
				SetOnline(0, accid);
				processStarted -= 1;

                if (csWasActiveBeforClosing)
                {
					try //сначала лок, потом трай
					{
						lock (connObj)
						{
							DateTime date = DateTime.Now;

							conn.Open();
							var com = new MySqlCommand("USE csgo; " +
							"Update accounts set canPlayDate = @canPlayDate where id = @id", conn);
							com.Parameters.AddWithValue("@canPlayDate", date.AddDays(7));
							com.Parameters.AddWithValue("@id", accid);
							com.ExecuteNonQuery();

							conn.Close();
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine($"[950][LOGIN: {login}]" + ex);
					}
					finally
					{
						conn.Close();
					}
				}				
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[901][LOGIN: {login}]" + ex);
			}
		}

		private static void SetOnlineZero()
		{
			try
			{
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
			finally
			{
				conn.Close();
			}
		}

		private static void TypeText(IntPtr console, IntPtr steamGuardWindow, string str)
		{
			langToEn();
			Thread.Sleep(500); //когда проц загружен нужен делей
			SetForegroundWindow(console);
			Thread.Sleep(100);
			SetForegroundWindow(steamGuardWindow);
			Thread.Sleep(1000);
			foreach (char ch in str)
			{
				PostMessage(steamGuardWindow, wmChar, ch, 0);
				Thread.Sleep(100);
			}
			Thread.Sleep(500);
			PostMessage(steamGuardWindow, WM_KEYDOWN, VK_ENTER, 1);
			Thread.Sleep(100);
			SetForegroundWindow(console);
		}

		private static void CloseSteamPlesh()
        {
            if(FindWindow(null, "Steam Login").ToString() != "0")
            {
				Console.WriteLine("Плешивый стим найден");
				Thread.Sleep(10000);				
			}			
		}

		private static void StartCsGo(int currentCycle, int lastCycle) //object state
		{
			int accid = 0;

			string login = "";

			string password = "";

			string secretKey = "";

			bool isOnline = false;

			int steamProcId = 0;

			int csProcId = 0;

			DateTime lastDateOnline = default;

			DateTime canPlayDate = default;

			try
			{
				lock (mainObj)
				{
					//записываем данные в переменные
					try
					{						
						conn.Open();
						var com = new MySqlCommand("USE csgo; " +
							"select * from accounts where isOnline = 0 AND NOW() > canPlayDate limit 1", conn);

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
									isOnline = Convert.ToBoolean(Convert.ToInt32(reader.GetString(4)));

									lastDateOnline = Convert.ToDateTime(reader.GetString(5));
									Console.WriteLine($"Last online: {lastDateOnline}");

									canPlayDate = Convert.ToDateTime(reader.GetString(6));
								}
							}
							else
							{
								throw new Exception("[902][SYSTEM] No suitable data");
							}
						}
						conn.Close();
					}
					catch (Exception ex)
					{
						conn.Close();
						Console.WriteLine(ex.Message);
						if (ex.Message == "[902][SYSTEM] No suitable data")
						{
							Console.WriteLine("Wait until all accounts are closed");
							Console.ReadLine();
							Environment.Exit(0);
						}
					}

					if(exceptionsInARow!=0)
					Console.WriteLine($"Exceptions in a row: {exceptionsInARow}");

					if (exceptionsInARow >= 3)
					{
						Console.WriteLine($"Габелла по ошибках их {exceptionsInARow}");
						try
						{
							DateTime date = DateTime.Now;
							conn.Open();
							var com1 = new MySqlCommand("USE csgo; " +
							"Update accounts set canPlayDate = @canPlayDate where id = @id", conn);
							com1.Parameters.AddWithValue("@canPlayDate", date.AddHours(2));
							com1.Parameters.AddWithValue("@id", accid);
							com1.ExecuteNonQuery();
							conn.Close();
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex.Message);
						}
						finally
						{
							exceptionsInARow = 0;
							conn.Close();
						}
						Console.WriteLine("[066][SYSTEM] Too much exceptions");
						throw new Exception("Abort");
					}

					Process csgoProc = new Process();
					Process steamProc = new Process();
					Process guardProc = new Process();
					Process process = new Process();
					ProcessStartInfo processStartInfo = new ProcessStartInfo();

					processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
					processStartInfo.FileName = "cmd.exe";					
                    processStartInfo.Arguments = string.Format("/C \"{0}\" -login {1} {2} -applaunch 730 -no-browser -language {3}{4}-x {5} -y {6} {7} {8}", new object[]
                    {
                       @"C:\Program Files (x86)\Steam\steam.exe",
                      login,
                       password,
                       accid,
                       Program.V2,
                       xSize * windowInARow,
                       Program.yOffset,
                       Program.parsnew,
                       Program.def
                    });

                    process.StartInfo = processStartInfo;
					process.Start();

					IntPtr steamWindow = new IntPtr();
					IntPtr csgoWindow = new IntPtr();
					IntPtr steamGuardWindow = new IntPtr();
					IntPtr console = FindWindow(null, "CSGO_IDLE_MACHINE");

					bool timeIsOverSteam = false;
					System.Timers.Timer tmrSteam = new System.Timers.Timer();
					tmrSteam.Interval = 1000 * 40;
					tmrSteam.Elapsed += (o, e) => CheckTimeSteam(ref timeIsOverSteam, tmrSteam);
					tmrSteam.Enabled = true;
					lock (threadLockType)
					{
						SetForegroundWindow(console);
					}						

					while (true)
					{
						steamWindow = FindWindow(null, "Вход в Steam");
						if (steamWindow.ToString() != "0" && !listSteamLogin.Contains(steamWindow.ToString())) //ищем стим, которого не было
						{
							Console.WriteLine("[SYSTEM] Steam detected");

							listSteamLogin.Add(steamWindow.ToString());

							Thread.Sleep(500);
							GetWindowThreadProcessId(steamWindow, ref steamProcId);
							steamProc = Process.GetProcessById(steamProcId);
							//listSteam.Add(steamProc);
							SetWindowText(steamProc.MainWindowHandle, $"steam_{login}");
							break;
						}

						steamWindow = FindWindow(null, "Steam Login");
						if (steamWindow.ToString() != "0" && !listSteamLogin.Contains(steamWindow.ToString()))
						{
							Console.WriteLine("[SYSTEM] Steam detected");

							listSteamLogin.Add(steamWindow.ToString());

							Thread.Sleep(500);
							GetWindowThreadProcessId(steamWindow, ref steamProcId);
							steamProc = Process.GetProcessById(steamProcId);
							//listSteam.Add(steamProc);
							SetWindowText(steamProc.MainWindowHandle, $"steam_{login}");
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
							exceptionsInARow += 1;
							Thread.Sleep(1000);
							throw new Exception("Abort");
						}

						Thread.Sleep(100);
					}					

					bool guardDetected1 = false;
					var codeGuardTask = GetGuardCodeAsync(secretKey);
					DateTime now1 = DateTime.Now;
					while (now1.AddSeconds(25) > DateTime.Now)
					{
						steamGuardWindow = FindWindow(null, "Steam Guard - Computer Authorization Required");
						if (steamGuardWindow.ToString() != "0")
						{
							Console.WriteLine("[SYSTEM] Steam Guard detected");
							guardDetected1 = true;
							break;
						}

						steamGuardWindow = FindWindow(null, "Steam Guard — Необходима авторизация компьютера");
						if (steamGuardWindow.ToString() != "0")
						{
							Console.WriteLine("[SYSTEM] Steam Guard detected");
							guardDetected1 = true;
							break;
						}

						Thread.Sleep(100);
					}					
					
					codeGuardTask.Wait();
					Console.WriteLine($"Guard code: {codeGuardTask.Result}");
					if (guardDetected1 == true && steamGuardWindow.ToString() != "0")
					{
						Thread.Sleep(3000); //с 1 секунды до 3 сек что бы на высоких кол-вах не захлёбывался
						lock (threadLockType)
						{
							TypeText(console, steamGuardWindow, codeGuardTask.Result);
						}
					}
					else
					{
						//try
						//{
						//	listSteamLogin.Remove(steamProc.MainWindowHandle.ToString());
						//}
						//catch { }

						steamProc.Kill(); //процесс подвисает на время загрузки гварда, никак не убить
						//listSteam.Remove(steamProc);
						Console.WriteLine("[SYSTEM] No steam Guard detected №2");
						exceptionsInARow += 1;
						Thread.Sleep(1000);
						throw new Exception("Abort");
					}
				
					var ts = new CancellationTokenSource(); //отменяемый таск
					CancellationToken ct = ts.Token;
					Task.Factory.StartNew(() =>
					{
						while (true)
						{
							IntPtr cs = FindWindow(null, "Updating Counter-Strike: Global Offensive");
							if (cs.ToString() != "0")
							{
								updatingWasFound = true;
								//Console.WriteLine("[SYSTEM] Updating...");
								//Thread.Sleep(5000);			

								IntPtr toggle = FindWindow(null, "Toggle"); 
								if (toggle.ToString() == "0")
								{
									Process processTog = new Process();
									ProcessStartInfo processStartInfoTog = new ProcessStartInfo();

									processStartInfoTog.WindowStyle = ProcessWindowStyle.Hidden;
									processStartInfoTog.FileName = "cmd.exe";
									processStartInfoTog.Arguments = string.Format("/C \"{0}\" {1}", new object[]
									{
										$@"{AppDomain.CurrentDomain.BaseDirectory}\Toggle.exe",
										lastCycle.ToString()
									});

									processTog.StartInfo = processStartInfoTog;
									processTog.Start();
									Environment.Exit(0);
								}
								else
								{
									Console.ReadLine(); //ждём пока тогл всё решит за нас
								}
							}

							Thread.Sleep(500);
							if (ct.IsCancellationRequested)
							{
								break;
							}
						}
					}, ct);					

					CheckGuardClosed(steamGuardWindow, steamProc, console, accid, codeGuardTask.Result); //тут должно отрабатывать но вообще неочаа

					bool timeIsOver = false;
					System.Timers.Timer tmr2 = new System.Timers.Timer();
                    tmr2.Interval = 1000*120;
                    tmr2.Elapsed += (o, e) => CheckTime(ref timeIsOver, tmr2);
					tmr2.Enabled = true;
					int xOffSave = xOffset;
					int yOffSave = yOffset;

					while (true)
					{
						csgoWindow = FindWindow(null, "Counter-Strike: Global Offensive");
						if (csgoWindow.ToString() != "0")
						{							
							Thread.Sleep(500);
							ts.Cancel();
							Console.WriteLine("[SYSTEM] CS:GO detected");
							Console.WriteLine(new string('-', 20)+$"Current window: {currentCycle}/{lastCycle}");
							GetWindowThreadProcessId(csgoWindow, ref csProcId);
							csgoProc = Process.GetProcessById(csProcId);
							//listCsgo.Add(csgoProc);
							SetWindowText(csgoWindow, $"csgo_{login}"); //ждёт подгруза кски, занимает се кунд 10. Но если не ждать, то слишком быстро всё
							SetCsgoPosAsync(csgoWindow, xOffset, yOffset,login);
							break;
						}

						if (timeIsOver == true)
                        {
                            //try
                            //{
                            //	listSteamLogin.Remove(steamProc.MainWindowHandle.ToString());
                            //}
                            //catch { }	

                            if (updatingWasFound == true)
                            {
								Console.ReadKey(); //ждём пока тогл сам закроет
                            }

							try
                            {
								steamProc.Kill();
							}
                            catch
                            {
								Console.WriteLine("[203][SYSTEM] Error");
							}
                            Console.WriteLine("[SYSTEM] No CSGO detected");
							exceptionsInARow += 1;
							Thread.Sleep(1000);
							throw new Exception("Abort");
						}
						Thread.Sleep(100);
					}
					processStarted += 1;
					SetOnline(1, accid);
					exceptionsInARow = 0;
					TypeInCsgoAsync(steamProc, csgoProc, login, accid, console, xOffSave, yOffSave);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine(new string('-', 35));
			}
		}

		private static void CloseAll()
        {
            try
            {
				foreach (var win in listCsgo)
				{
					win.Kill();
					Thread.Sleep(500);
				}
				listCsgo.Clear();

				Console.WriteLine("[SYSTEM] All CS:GO killed");
				foreach (var win in listSteam)
				{
					win.Kill();
					//listSteam.Remove(win);
					//ChangeOnline(0, win.);
					processStarted -= 1;
					Thread.Sleep(500);
				}
				//listSteam.Clear();
				Console.WriteLine("[SYSTEM] All Steam killed");
				Console.WriteLine(new string('-',35));
			}
            catch(Exception ex)
            {
				Console.WriteLine(ex);
            }			
		}

		private static void CheckSubscribe(string key)
        {
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

		private static void Start(int count)
		{
			int i = 0;
			listSteamLogin.Add("0"); //иногда ловил нолики при закрытии на всякий вставляю
			while (true)
            {				
				while (Process.GetProcessesByName("csgo").Length < count) //processStarted < count // 'ЭТА ВЕРСИЯ ДЛЯ ПОДДЕРЖАНИЯ ВСЕГДА N ПОТОКОВ и норм размещения окон
				{
					Thread myThread = new Thread(delegate () { StartCsGo(Process.GetProcessesByName("csgo").Length + 1, count); });
					myThread.Start();
					myThread.Join();
					i += 1;
				}
				Thread.Sleep(1000);
			}
		}

		private static async Task StartAsync(int count)
		{
			await Task.Run(() => Start(count));
		}

		static void Main(string[] args)
		{
			Console.Title = "CSGO_IDLE_MACHINE";
			Thread.Sleep(100);
			IntPtr conWindow = FindWindow(null, "CSGO_IDLE_MACHINE");			
			SetWindowPos(conWindow, IntPtr.Zero, monitorSizeX - consoleX, monitorSizeY - consoleY - 40, consoleX, consoleY, SWP_NOZORDER); //вылазит за экран если размер элементов больше 100%			
			SetForegroundWindow(conWindow);

			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("IDLE MACHINE v1.7.2");
			Console.WriteLine("discord.gg/nRrrpqhRtg");
			Console.ResetColor();

			if (File.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}\License.lic"))
			{
                string key = "";
                using (StreamReader sr = new StreamReader($@"{AppDomain.CurrentDomain.BaseDirectory}\License.lic"))
                {
                    key = sr.ReadToEnd();
                }
                key = key.Replace("\r\n", "");

				CheckSubscribe(key);

				if (PcInfo.GetCurrentPCInfo() == key)
				{	
					Console.WriteLine("[SYSTEM] License confirmed");

					SetOnlineZero(); 
					if (File.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}\connection.txt"))
					{
						 string connStr = "1";
                        using (StreamReader sr = new StreamReader($@"{AppDomain.CurrentDomain.BaseDirectory}\connection.txt"))
                        {
                            connStr = sr.ReadToEnd();
                        }
                        connStr = connStr.Replace("\r\n", "");

                        if (connStr != "")
						{
							serverConnection = connStr;
							Console.OutputEncoding = Encoding.UTF8;
							int count = 0;
							try
							{
								count = Convert.ToInt32(args[0]);
							}
							catch
							{
								Console.WriteLine("Write the number of windows csgo: ");
								count = Convert.ToInt32(Console.ReadLine());
							}
							int cycleCount = 0;
							tmr.Interval = timerDelayInSeconds;
							tmr.Elapsed += TmrEvent; //делаем за циклом что бы не стакались события
							bool hasStarted = false;

							while (true) 
                            {
                                CheckSubscribe(key);
                                if (hasStarted == false)
                                {
									StartAsync(count);
									hasStarted = true;
								}                               

                                cycleCount += 1;
								Console.WriteLine($"[SYSTEM] Current cycle №{cycleCount}");
								tmr.Enabled = true;

								Console.ForegroundColor = ConsoleColor.DarkGreen;
								Console.WriteLine($"[SYSTEM] New cycle after: {minToNewCycle} minutes" + "   "); //что бы показывало время сразу, а не через минуту
								Console.ResetColor();

								//тут асинхронность, которая открывает кс если что

								Thread.Sleep(timeIdle); 
								Console.ForegroundColor = ConsoleColor.Green; // устанавливаем цвет		
								Console.WriteLine();
								Console.WriteLine("[SYSTEM] New cycle");
								Console.ResetColor(); // сбрасываем в стандартный

								minToNewCycle = timeIdle / 60000;
								windowInARow = 0;
								windowCount = 0;
								xOffset = 0;
								yOffset = 0;

							}
						}
						else
						{
							Console.WriteLine("[600][SYSTEM] connection.txt is empty");
							Thread.Sleep(5000);
							Environment.Exit(0);
						}

					}
					else
					{
						Console.WriteLine("[601][SYSTEM] connection.txt not found");
						Thread.Sleep(5000);
						Environment.Exit(0);
					}

				}
				else
				{
					Console.WriteLine("[560][SYSTEM] License not found");
					Thread.Sleep(5000);
					Environment.Exit(0);
				}
			}
			else
			{
				Console.WriteLine("[561][SYSTEM] License not found");
				Thread.Sleep(5000);
				Environment.Exit(0);
			}

			Console.ReadKey();
		}
	}
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput;
using System.ComponentModel;
using SharpDX;
using Gma.System.MouseKeyHook;
using WindowsInput;
using System.Data;

namespace test
{
    internal class Program
    {
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetClientRect(IntPtr hWnd, out Rectangle lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(int nIndex);

        [DllImport("user32.dll")]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        const int SM_CXSCREEN = 0;
        const int SM_CYSCREEN = 1;

        const int MOUSEEVENTF_MOVE = 0x0001;
        const int MOUSEEVENTF_ABSOLUTE = 0x8000;


        const int INPUT_MOUSE = 0;

        struct INPUT
        {
            public InputType type;
            public MOUSEINPUT mi;
        }

        enum InputType : uint
        {
            Mouse = 0,
        }


        struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        struct POINT
        {
            public int X;
            public int Y;
        }

        const uint SWP_NOSIZE = 0x0001;

        const uint SWP_NOZORDER = 0x0004;

        [DllImport("user32.dll")]
        static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private const int WH_MOUSE_LL = 14;
        private const int WM_MOUSEMOVE = 0x0200;

        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private static LowLevelMouseProc _proc;
        private static IntPtr _hookID = IntPtr.Zero;
        private static POINT _lastCursorPosition;

        private static IntPtr SetHook(LowLevelMouseProc proc)
        {
            using var curProcess = System.Diagnostics.Process.GetCurrentProcess();
            using var curModule = curProcess.MainModule;
            return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_MOUSEMOVE)
            {
                var hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                Console.WriteLine($"X: {hookStruct.pt.X}, Y: {hookStruct.pt.Y}");
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINTAPI
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MouseHookStruct
        {
            public POINTAPI pt;
            public int hwnd;
            public int wHitTestCode;
            public int dwExtraInfo;
        }

        private static void Hook_MouseMove(object sender, MouseEventArgs e)
        {
            Console.WriteLine($"X: {e.X}, Y: {e.Y}");
        }

        private static IKeyboardMouseEvents _hook;


        public delegate void MouseMovedEvent();

        public class GlobalMouseHandler : IMessageFilter
        {
            private const int WM_MOUSEMOVE = 0x0200;

            public event MouseMovedEvent TheMouseMoved;

            #region IMessageFilter Members

            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg == WM_MOUSEMOVE)
                {
                    if (TheMouseMoved != null)
                    {
                        TheMouseMoved();
                    }
                }
                // Always allow message to continue to the next filter control
                return false;
            }

            #endregion
        }

        public partial class Form1 : Form
        {
            public Form1()
            {
                GlobalMouseHandler gmh = new GlobalMouseHandler();
                gmh.TheMouseMoved += new MouseMovedEvent(gmh_TheMouseMoved);
                Application.AddMessageFilter(gmh);

                
            }

            void gmh_TheMouseMoved()
            {
                Point cur_pos = System.Windows.Forms.Cursor.Position;
                System.Console.WriteLine(cur_pos);
            }
        }
        static private IKeyboardMouseEvents m_GlobalHook;

        [STAThread]      // I think this is necessary to ensure thread-safety
        static void PumpQueue()
        {
            Subscribe();

            Application.Run();
        }

        static public void Subscribe()
        {
            m_GlobalHook = Hook.GlobalEvents();

            m_GlobalHook.MouseDownExt += GlobalHookMouseDownExt;
        }

        static private void GlobalHookMouseDownExt(object sender, MouseEventExtArgs e)
        {
            Console.WriteLine("Mouse Click.");
        }

        private static void Events_MouseMove(object sender, MouseEventArgs e)
        {
            Console.WriteLine($"X: {e.X}, Y: {e.Y}");
        }

        static void Main(string[] args)
        {

            Hook.GlobalEvents().MouseMove += async (sender, e) =>
            {
                Console.WriteLine($"Mouse {e.X} {e.Y} Down");
            };
            //When a double click is made
            
            // Здесь может быть ваш код для работы с окном игры

            Console.ReadLine();
            //Thread.Sleep(2000);
            //Console.WriteLine("начали");

            //string logDirectory = "C:\\Users\\gvozd\\source\\repos\\ConsoleApp1\\test\\log";
            //string windowTitle = "Counter-Strike: Global Offensive - Direct3D 9";

            //Logger logger = new Logger(logDirectory, windowTitle);
            //logger.Start();

            //--------------------------------------------------------
            //Thread.Sleep(500);
            //IntPtr targetWindowHandle = FindWindow(null, "Counter-Strike: Global Offensive - Direct3D 9");
            //// SetWindowPos(targetWindowHandle, IntPtr.Zero, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
            //// Переключение фокуса на целевое окно
            //SetForegroundWindow(targetWindowHandle);
            //Thread.Sleep(1);

            //// Координаты, куда переместить мышь
            //int targetX = 100; // полный оборт через превое плечо по горизонтали 7438. 1 градус примерно 20.66 пикселей
            //// но если двигать по 1 пикселю 7438 раз, то будет только 180 поворот
            //int targetY = 0; // 3678 полностью опустить голову вниз из полностью вертикального положения. 1 градус примерно 20.43 пикселя

            //// Получение текущих координат мыши
            //POINT currentMousePosition;
            //GetCursorPos(out currentMousePosition);
            //Console.WriteLine(currentMousePosition.X + " " + currentMousePosition.Y);

            //// Вычисление изменения координаты X и Y
            ////int dx = targetX - currentMousePosition.X;
            ////int dy = targetY - currentMousePosition.Y;

            //int dx = targetX;
            //int dy = targetY;

            //Console.WriteLine(dx + " " + dy);

            //// Создание структуры INPUT для эмуляции перемещения мыши
            //INPUT[] inputs = new INPUT[1];
            //inputs[0].type = InputType.Mouse;
            //inputs[0].mi.dx = dx;
            //inputs[0].mi.dy = dy;
            //inputs[0].mi.mouseData = 0;
            //inputs[0].mi.dwFlags = MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE;

            //for (int i = 0; i < 74; i++)
            //{
            //    SetForegroundWindow(targetWindowHandle);
            //    Thread.Sleep(10);
            //    // Вызов SendInput для эмуляции перемещения мыши
            //    if (SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT))) == 0)
            //    {
            //        Console.WriteLine("Ошибка при вызове SendInput: " + Marshal.GetLastWin32Error());
            //    }
            //    i++;
            //}



            Console.ReadLine();
        }
    }
}

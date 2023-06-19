using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

public class Logger
{
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_MOUSEMOVE = 0x0200;

    private string logDirectory;
    private string windowTitle;
    private IntPtr windowHandle;
    public static int counterX = 0;
    public static int counterY = 0;
    public static int counterScreen = 0;

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    public Logger(string logDirectory, string windowTitle)
    {
        this.logDirectory = logDirectory;
        this.windowTitle = windowTitle;
    }

    public void Start()
    {       
        windowHandle = FindWindow(null, "Counter-Strike: Global Offensive - Direct3D 9");

        // Создаем директорию для логов, если она не существует
        Directory.CreateDirectory(logDirectory);

        // Запускаем потоки для логирования
        Thread screenshotThread = new Thread(CaptureScreenshot);
        screenshotThread.Priority = ThreadPriority.Highest;
        screenshotThread.Start();

        Thread keyboardThread = new Thread(LogKeyboardInput);
        keyboardThread.Start();

        Thread mouseThread = new Thread(LogMouseInput);
        mouseThread.Start();
    }

    private void CaptureScreenshot()
    {
        int counterScreen = 0; // Счетчик скриншотов
        Stopwatch stopwatch = new Stopwatch(); // Таймер для измерения времени выполнения

        while (true)
        {
            try
            {
                // Получаем размеры окна
                RECT rect;
                GetWindowRect(windowHandle, out rect);

                // Масштабируем размеры окна
                rect.left = Convert.ToInt32(rect.left * 1.25);
                rect.right = Convert.ToInt32(rect.right * 1.25);
                rect.top = Convert.ToInt32(rect.top * 1.25);
                rect.bottom = Convert.ToInt32(rect.bottom * 1.25);

                int width = rect.right - rect.left;
                int height = rect.bottom - rect.top;

                // Создаем битмап и делаем скриншот окна
                using (var bitmap = new System.Drawing.Bitmap(width, height))
                {
                    using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
                    {
                        graphics.CopyFromScreen(rect.left, rect.top, 0, 0, bitmap.Size);
                    }

                    // Сохраняем скриншот в файл
                    counterScreen++;
                    string fileName = Path.Combine(logDirectory, $"screenshot_{DateTime.Now:yyyyMMddHHmmssfffff}-{counterScreen}.png");
                    bitmap.Save(fileName, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок при сохранении скриншота
                Console.WriteLine($"Ошибка при сохранении скриншота: {ex.Message}");
            }

            // Задержка перед следующим скриншотом
            //stopwatch.Restart(); // Запуск таймера
            //while (stopwatch.ElapsedMilliseconds < 50) { } // Ожидание 50 мс
            //stopwatch.Stop(); // Остановка таймера
        }
    }

    private void LogKeyboardInput()
    {
        StringBuilder sb = new StringBuilder();
        while (true)
        {
            if (windowHandle == GetForegroundWindow())
            {
                for (int i = 0; i < 256; i++)
                {
                    bool isKeyDown = ((GetAsyncKeyState(i) & 0x8000) != 0);
                    if (isKeyDown)
                    {
                        sb.Append((Keys)i + " ");
                    }
                }

                if (sb.Length > 0)
                {
                    string log = sb.ToString().Trim();
                    LogToFile(log);
                    sb.Clear();
                }
            }

            // Задержка перед следующей проверкой нажатий клавиш
            Thread.Sleep(50); // 20 раз в секунду (1000 / 20 = 50 мс)
        }
    }




    private void LogMouseInput()
    {
        
        StringBuilder sb = new StringBuilder();
        while (true)
        {
            if (windowHandle == GetForegroundWindow())
            {
                POINT point;
                GetCursorPos(out point);
                counterX = counterX + point.X - 1024;
                counterY = counterX + point.Y - 576;

                sb.Append($"X: {point.X-1024}, Y: {point.Y}. counterX {counterX} ; counterY {counterY}");

                if (sb.Length > 0)
                {
                    string log = sb.ToString();
                    LogToFile(log);
                    sb.Clear();
                }
            }

            // Задержка перед следующей проверкой позиции мыши
            Thread.Sleep(1); // 20 раз в секунду (1000 / 20 = 50 мс)
        }
    }

    private void LogToFile(string log)
    {
        try
        {
            string fileName = Path.Combine(logDirectory, "log.txt");
            using (StreamWriter writer = new StreamWriter(fileName, true))
            {
                writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss:fffff}: {log}");
            }
        }
        catch (Exception ex)
        {
            // Обработка ошибок при записи лога в файл
            Console.WriteLine($"Ошибка при записи лога: {ex.Message}");
        }
    }

    // Импорт функций WinAPI для работы с окнами и мышью
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern int GetWindowRect(IntPtr hWnd, out RECT rect);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }
}

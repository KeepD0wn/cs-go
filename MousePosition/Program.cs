using System;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace MousePosition
{
    class Program
    {
        static void Main(string[] args)
        {
            for (int i = 10; i > 0; i--)
            {
                Console.WriteLine($"Наведите курсок мыши на ярлык стима. Осталось {i} секунд");
                Thread.Sleep(1000);
            }

            int CursorX = Cursor.Position.X;
            int CursorY = Cursor.Position.Y;
            using (FileStream fstream = new FileStream($@"{AppDomain.CurrentDomain.BaseDirectory}\position.txt", FileMode.Create))
            {
                // преобразуем строку в байты
                byte[] array = System.Text.Encoding.Default.GetBytes($"{CursorX} {CursorY}");
                // запись массива байтов в файл
                fstream.Write(array, 0, array.Length);
                Console.WriteLine($"{CursorX} {CursorY}");
            }

            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}

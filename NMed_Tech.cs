using System;
using System.IO;

namespace task_A
{
    class Program
    {
        static void Main(string[] args)
        {
            int A, B;
            string s;

            // РЕШЕНИЕ В КОНСОЛИ
            //s = Console.ReadLine();

            //try
            //{
            //    A = int.Parse(s.Substring(0, s.IndexOf(" ")));
            //    B = int.Parse(s.Substring(s.IndexOf(" ") + 1));
            //    Console.WriteLine(A + B);
            //}
            //catch (FormatException e)
            //{
            //    Console.WriteLine(e.Message);
            //}

            //РЕШЕНИЕ ЧЕРЕЗ ФАЙЛ
            StreamReader reader = new StreamReader("input.txt");
            StreamWriter writer = new StreamWriter("output.txt");

            s = reader.ReadLine();

            try
            {
                A = int.Parse(s.Substring(0, s.IndexOf(" ")));
                B = int.Parse(s.Substring(s.IndexOf(" ") + 1));
                writer.WriteLine(A + B);
                Console.WriteLine("Записано в файл.");
            }
            catch (FormatException e)
            {
                Console.WriteLine(e.Message);
            }

            reader.Close(); writer.Close();

            Console.ReadKey();
        }
    }
}

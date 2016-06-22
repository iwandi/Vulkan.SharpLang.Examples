using System;
using System.IO;

namespace Vulkaninfo
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            if (!Console.IsOutputRedirected)
            {
                Console.WindowWidth = 130;
                Console.WindowHeight = 50;
                Console.BufferWidth = 130;
                Console.BufferHeight = 20000;
            }

            InfoGenerator gen = new InfoGenerator();
                           
            StreamWriter sw = new StreamWriter(Console.OpenStandardOutput());
            gen.DumpInfo(sw);
            sw.Close();

            if (!Console.IsOutputRedirected)
            {
                Console.ReadLine();
            }
        }
    }
}

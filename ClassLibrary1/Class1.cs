using System;
using System.Threading;

namespace ClassLibrary1
{
    public class NombrePremier
    {
        public static void Premier()
        {
            for (int p = 1; p < 1000000; p++)
            {
                int i = 2;
                while ((p % i) != 0 && i < p)
                {
                    i++;
                }
                if (i == p)
                    Console.WriteLine(p.ToString());
                Thread.Sleep(50);

            }
        }
    }
}

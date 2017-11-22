using System;
using System.Threading;

namespace ClassLibraryPremier
{
    public class NombrePremier
    {
        public static void Premier(int number)
        {
            for (int p = 1; p < 1000000; p++)
            {
                int i = 2;
                while ((p % i) != 0 && i < p)
                {
                    i++;
                }
                if (i == p)
                    Console.WriteLine("Thread <{0}> = {1}", number.ToString(), p.ToString());
                Thread.Sleep(50);

            }
        }
    }
}

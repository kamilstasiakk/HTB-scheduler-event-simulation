using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueueSystemSimulator
{
    class Program
    {
        static void Main(string[] args)
        {
            SystemKolejkowy system = new SystemKolejkowy();
            system.start();
        }
    }
}

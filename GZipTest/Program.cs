using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZipTest
{
    class Program
    {
        static int Main(string[] args)
        {
            var processing = new Processing();
            var result = processing.Start();
            // processing.Dispose();

            return result;
        }
    }
}

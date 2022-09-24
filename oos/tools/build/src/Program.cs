using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Build
{
    class Program
    { 
        static void Main(string[] args)
        {
            MachinePara.Init(args[0],args[1],args[2],args[3]); // c.img boot.bin kernel.bin programs
            Run r = new Run();
            r.Begin();
        }
    }
}

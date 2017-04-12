using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace OSBIDE.ConsoleSetup
{
    class Program
    {
        static void Main(string[] args)
        {
            using (FileStream fs = File.OpenWrite("log.txt"))
            {
                using (StreamWriter writer = new StreamWriter(fs))
                {
                    Process proc1 = new Process();
                    proc1.StartInfo.FileName = "OSBIDE.vsix";
                    proc1.Start();
                    proc1.WaitForExit();
                    proc1.Close();
                }
            }
        }
    }
}

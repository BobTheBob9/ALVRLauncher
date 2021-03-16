using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace ALVRLauncher
{
    public static class ApplicationLauncher
    {
        public static Process LaunchApplication()
        {
            // honestly this is weird but this doesn't get called enough to require caching 
            // and it's easier to do this in a file than to implement any sort of actual settings window for now lmao
            string filepath = File.ReadAllText("alvrpath.txt");

            Console.WriteLine("starting alvr...");
            return Process.Start(filepath);
        }
    }
}

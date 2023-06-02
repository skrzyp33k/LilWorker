using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LilWorker.Essentials
{
    public class LWPFile
    {
        public string Name { get; private set; }

        public string Path { get; private set; }

        public LWPFile(string fileName)
        {
            Name = fileName;
            Path = Directory.GetCurrentDirectory() + "\\" + fileName + ".lwp";
        }

        //TODO:
        //load
        //recalculate coordinates
        //send to ESP32
        //debug print
    }
}

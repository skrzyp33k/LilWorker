using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LilWorker.Essentials
{
    public class LWIFile
    {
        public string Name { get; private set; }

        public string Path { get; private set; }

        private List<string> _instructions = new List<string>();
        public LWIFile(string fileName)
        {
            Name = fileName;
            Path = Directory.GetCurrentDirectory() + "\\" + fileName + ".lwi";
            _loadFile();
        }

        private void _loadFile()
        {
            _instructions = File.ReadAllLines(Path).ToList();
        }

        public void DebugPrint()
        {
            Console.WriteLine("_instructions");
            foreach(string line in _instructions)
            {
                Console.WriteLine($"{line}");
            }
        }
        public void SendTo(string workerIP)
        {
            TCPSocket.SendFile(workerIP, 60001, Path);
        }
    }
}

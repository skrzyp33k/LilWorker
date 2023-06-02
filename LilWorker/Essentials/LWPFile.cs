﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LilWorker.Essentials
{
    public class LWPFile
    {
        public string Name { get; private set; }

        public string Path { get; private set; }

        private string _pathWOExt = "";

        private string[] _rawLines = { "" };
        private List<string> _rebuildLines = new List<string>();
        private List<string> _instructions = new List<string>();

        private int middleX = 0;
        private int middleY = 0;

        public LWPFile(string fileName)
        {
            Name = fileName;
            Path = Directory.GetCurrentDirectory() + "\\" + fileName + ".lwp";
            _pathWOExt = Directory.GetCurrentDirectory() + "\\" + fileName;
            _loadFile();
            _rebuildFile();
            _searchMidpoint();
            _recalculateFile();
        }

        private void _loadFile()
        {
            _rawLines = File.ReadAllLines(Path);
        }

        private void _rebuildFile()
        {
            List<string> dividedCoords = new List<string>();

            for (int i = 0; i < _rawLines.Length; i++)
            {
                if (_rawLines[i].StartsWith("@"))
                {
                    dividedCoords.Add(_rawLines[i]);
                    continue;
                }
                string[] line = _rawLines[i].Split(";");

                string coords1 = line[0] + ";" + line[1];
                string coords2 = line[2] + ";" + line[3];

                dividedCoords.Add(coords1);
                dividedCoords.Add(coords2);
            }

            string prev = "";

            foreach (string coords in dividedCoords)
            {
                if (prev != coords)
                {
                    _rebuildLines.Add(coords);
                    prev = coords;
                }
            }
        }

        private void _searchMidpoint()
        {
            int minX = 0;
            int maxX = 0;

            int minY = 0;
            int maxY = 0;

            foreach (string line in _rebuildLines)
            {
                if(line.StartsWith("@"))
                {
                    continue;
                }
                string[] coords = line.Split(";");
                int x = int.Parse(coords[0]);
                int y = int.Parse(coords[1]);

                if (x > maxX) maxX = x;
                if (y > maxY) maxY = y;

                if(x < minX) minX = x;
                if(y < minY) minY = y;
            }

            middleX = (int)Math.Round((double)(maxX - minX) / 2.0);
            middleY = (int)Math.Round((double)(maxY - minY) / 2.0);
        }

        private string _recalculateCoords(string line)
        {
            string[] coords = line.Split(";");
            int x = int.Parse(coords[0]) - middleX;
            int y = int.Parse(coords[1]) - middleY;
            return $"{x};{y}";
        }

        private void _recalculateFile()
        {
            for(int i = 0; i < _rebuildLines.Count; i++)
            {
                string line = _rebuildLines[i];
                if(line.StartsWith("@"))
                {
                    if(line == "@next")
                    {
                        if(i+1 == _rebuildLines.Count)
                        {
                            _instructions.Add("end");
                            break;
                        }
                        _instructions.Add("fly:up");
                        _instructions.Add($"fly:{_recalculateCoords(_rebuildLines[i+1])}");
                        _instructions.Add("fly:down");
                    }
                    continue;
                }
                _instructions.Add($"fly:{_recalculateCoords(line)}");
            }
        }

        private void _writeLWIFile(string path)
        {
            try
            {
                File.WriteAllLines(path, _instructions);
                Console.WriteLine($"File {path} saved.");
            }
            catch (IOException ex)
            {
                Console.WriteLine("Error while file writing: " + ex.Message);
            }
        }
        public void SaveLWIFile()
        {
            string path = _pathWOExt + ".lwi";
            _writeLWIFile(path);
        }

        public void SaveLWIFile(string name)
        {
            string path = Directory.GetCurrentDirectory() + "\\" + name + ".lwi";
            _writeLWIFile(path);
        }

        public void DebugPrint()
        {
            Console.WriteLine("_rawLines");
            foreach (string line in _rawLines)
            {
                Console.WriteLine(line);
            }
            Console.WriteLine("_rebuildLines");
            foreach (string line in _rebuildLines)
            {
                Console.WriteLine(line);
            }
            Console.WriteLine("_instructions");
            foreach (string line in _instructions)
            {
                Console.WriteLine(line);
            }
        }
    }
}

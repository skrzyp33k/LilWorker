using LilWorker;
using LilWorker.Essentials;
using System.Net.NetworkInformation;

string prefix = "LilWorker";

Dictionary<int, NetworkInterface> networkInterfaces = new Dictionary<int, NetworkInterface>();
int networkInterfacesKey = -1;

List<WorkerController> workers = new List<WorkerController>();
int workerIndex = -1;

LWPFile lwpFile = null;
LWIFile lwiFile = null;

void Help()
{
    Console.WriteLine();
    Console.WriteLine("Instructions tree:");
    Console.WriteLine("interface - command for inet operations");
    Console.WriteLine("\t list - list available interfaces");
    Console.WriteLine("\t select {NUMBER} - selects interface");
    Console.WriteLine("\t scan - scans for LilWorkers");
    Console.WriteLine("file - command for file operations");
    Console.WriteLine("\t lwp - command for lwp file operations");
    Console.WriteLine("\t\t list - list *.lwp files near this executable");
    Console.WriteLine("\t\t load {NAME} - loads NAME.lwp file");
    Console.WriteLine("\t\t save {NAME}  - save NAME.lwi file");
    Console.WriteLine("\t lwi - command for lwi file operations");
    Console.WriteLine("\t\t list - list *.lwi files near this executable");
    Console.WriteLine("\t\t load {NAME} - loads NAME.lwi file");
    Console.WriteLine("\t\t send {WORKER ID} - send loaded file to WORKER");
    Console.WriteLine();
}

async Task CommandProcessorAsync()
{
    Console.Write($"[{prefix}] ");
    string input = Console.ReadLine()!;
    if (input == "help")
    {
        Help();
        return;
    }

    string[] args = input.Split(" ");

    if (args[0] == "interface")
    {
        if (args[1] == "list")
        {
            networkInterfaces = EthernetInterface.GetInterfaces();
            foreach (var inter in networkInterfaces)
            {
                Console.WriteLine($"{inter.Key} - {inter.Value.Name} - {inter.Value.GetIPProperties().UnicastAddresses[1].Address}");
            }
        }
        else if (args[1] == "select")
        {
            networkInterfacesKey = int.Parse(args[2]);
        }
        else if (args[1] == "scan")
        {
            if (networkInterfacesKey >= 0)
            {
                workers = await WorkerController.ScanForWorkers(networkInterfaces[networkInterfacesKey], 60000, TimeSpan.FromSeconds(5));
                Console.WriteLine("ID\tNAME\t\t\tIP");
                for (int i = 0; i < workers.Count; i++)
                {
                    Console.WriteLine($"{i}\t{workers[i].WorkerName}\t{workers[i].WorkerIP}");
                }
            }
        }
        else
        {
            Console.WriteLine("you idiot");
        }
    }
    else if (args[0] == "file")
    {
        if (args[1] == "lwp")
        {
            if (args[2] == "list")
            {
                string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.lwp");
                Console.WriteLine($"Files in {Directory.GetCurrentDirectory()}:");
                foreach(string file in files)
                {
                    string name = file.Split(@"\").Last().Split(".").First();
                    Console.WriteLine($"\t- {name}");
                }
            }
            else if (args[2] == "load")
            {
                try
                {
                    lwpFile = new LWPFile(args[3]);
                }
                catch (Exception ex)
                {
                    Console.Write(ex.ToString());
                }
            }
            else if (args[2] == "save")
            {
                if(lwpFile != null)
                {
                    lwpFile.SaveLWIFile();
                }
            }
            else
            {
                Console.WriteLine("you idiot");
            }
        }
        else if (args[1] == "lwi")
        {
            if (args[2] == "list")
            {
                string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.lwi");
                Console.WriteLine($"Files in {Directory.GetCurrentDirectory()}:");
                foreach (string file in files)
                {
                    string name = file.Split(@"\").Last().Split(".").First();
                    Console.WriteLine($"\t- {name}");
                }
            }
            else if (args[2] == "load")
            {
                try
                {
                    lwiFile = new LWIFile(args[3]);
                }
                catch (Exception ex)
                {
                    Console.Write(ex.ToString());
                }
            }
            else if (args[2] == "send")
            {
                try
                {
                    if(lwiFile != null)
                    {
                        lwiFile.SendTo(workers[int.Parse(args[3])].WorkerIP);
                    }
                }
                catch (Exception ex)
                {
                    Console.Write(ex.ToString());
                }
            }
            else
            {
                Console.WriteLine("you idiot");
            }
        }
        else
        {
            Console.WriteLine("you idiot");
        }
    }
    else
    {
        Console.WriteLine("you idiot");
    }
}

// Main loop
while (true)
{
    await CommandProcessorAsync();
}
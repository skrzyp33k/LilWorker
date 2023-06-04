using LilWorker;
using LilWorker.Essentials;
using System.Net.NetworkInformation;

string prefix = "LilWorker";

Dictionary<int, NetworkInterface> networkInterfaces = new Dictionary<int, NetworkInterface>();
int networkInterfacesKey = -1;

void Help()
{
    Console.WriteLine();
    Console.WriteLine("Instructions tree:");
    Console.WriteLine("interface - command for inet operations");
    Console.WriteLine("\t list - list available interfaces");
    Console.WriteLine("\t select {NUMBER} - selects interface");
    Console.WriteLine("\t scan - scans for LilWorkers");
    Console.WriteLine();
}

async Task CommandProcessorAsync()
{
    Console.Write($"[{prefix}] ");
    string input = Console.ReadLine();
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
            if(networkInterfacesKey >= 0)
            {
                List<WorkerController> workers = await WorkerController.ScanForWorkers(networkInterfaces[networkInterfacesKey], 60000, TimeSpan.FromSeconds(5));
            }
        }
    }
}

// Main loop
while(true)
{
    await CommandProcessorAsync();
}
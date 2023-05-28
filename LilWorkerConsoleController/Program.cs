using LilWorker;

var interfaces = LilWorker.Essentials.EthernetInterface.GetInterfaces();

foreach(var inter in interfaces)
{
    Console.WriteLine($"{inter.Key} - {inter.Value.Name}");
}

int networkInterface = -1;

Console.Write("Interfejs: ");

networkInterface = int.Parse(Console.ReadLine());

List<WorkerController> workers = await WorkerController.ScanForWorkers(interfaces[networkInterface], 60000, TimeSpan.FromSeconds(5));

foreach(var worker in workers)
{
    Console.WriteLine($"{worker.WorkerIP}\t{worker.WorkerName}");
}

Console.ReadKey();
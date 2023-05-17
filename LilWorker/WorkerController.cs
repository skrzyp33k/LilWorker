using System.Net;
using System.Net.NetworkInformation;
using LilWorker.Essentials;

namespace LilWorker
{
    public class WorkerController
    {
        public string WorkerIP { get; private set; }
        public string WorkerID { get; private set; }

        public WorkerController(string workerIP, string workerID)
        {
            this.WorkerIP = workerIP;
            this.WorkerID = "LilWorker-" + workerID;
        }

        public static async Task<List<WorkerController>> ScanForWorkers(NetworkInterface networkInterface, int udpPort, TimeSpan timeSpan)
        {
            List<WorkerController> workerControllers = new List<WorkerController>();

            IPAddress broadcastAddress = EthernetInterface.GetBroadcastAddress(networkInterface);

            UDPSocket socket = new UDPSocket(broadcastAddress, 60000);

            List<(string,string)> response = await socket.SendAndReceiveWithTimeoutAsync("LilWorker?", timeSpan);

            foreach((string ip, string id) in response)
            {
                workerControllers.Add(new WorkerController(ip, id));
            }

            return workerControllers;
        }
    }
}
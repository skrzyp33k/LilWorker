using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class UdpListener
{
    private int port;
    private UdpClient udpClient;

    public UdpListener(int port)
    {
        this.port = port;
        udpClient = new UdpClient(port);
    }

    public void StartListening()
    {
        Console.WriteLine("LilWorker emulator running on port {0}...", this.port);

        while (true)
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] receivedBytes = udpClient.Receive(ref remoteEndPoint);
            string receivedMessage = Encoding.ASCII.GetString(receivedBytes);

            if (receivedMessage == "LilWorker?")
            {
                string responseMessage = "LilWorker-EMULATOR";
                byte[] responseBytes = Encoding.ASCII.GetBytes(responseMessage);

                udpClient.Send(responseBytes, responseBytes.Length, remoteEndPoint);
                Console.WriteLine("Presented to address: {0}", remoteEndPoint.Address.ToString());
                break;
            }
        }
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        UdpListener udpListener = new UdpListener(60000);
        udpListener.StartListening();
    }
}
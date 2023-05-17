using System;
using System.Net;
using System.Net.NetworkInformation;

namespace LilWorker.Essentials
{
    public class EthernetInterface
    {
        public static Dictionary<int, NetworkInterface> GetInterfaces()
        {
            var result = new Dictionary<int, NetworkInterface>();
            int counter = 0;

            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface networkInterface in interfaces)
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    result.Add(counter, networkInterface);
                    counter++;
                }
            }

            return result;
        }

        public static IPAddress GetBroadcastAddress(NetworkInterface networkInterface)
        {
            IPAddress broadcast = IPAddress.Parse("255.255.255.255");

            IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();
            foreach (UnicastIPAddressInformation addressInfo in ipProperties.UnicastAddresses)
            {
                if (addressInfo.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    IPAddress ipAddress = addressInfo.Address;
                    IPAddress subnetMask = addressInfo.IPv4Mask;
                    
                    broadcast = calculateBroadcast(ipAddress, subnetMask);
                }
            }

            return broadcast;
        }

        private static IPAddress calculateBroadcast(IPAddress ipAddress, IPAddress subnetMask)
        {
            byte[] ipBytes = ipAddress.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("The length of the IP address and subnet mask must be the same!");

            byte[] broadcastBytes = new byte[ipBytes.Length];

            for (int i = 0; i < broadcastBytes.Length; i++)
            {
                broadcastBytes[i] = (byte)(ipBytes[i] | (subnetMaskBytes[i] ^ 255));
            }

            return new IPAddress(broadcastBytes);
        }
    }
}

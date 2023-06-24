using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LilWorker.Essentials
{
    internal class TCPSocket
    {
        public static void SendFile(string ipAddress, int port, string filePath)
        {
            byte[] fileData = File.ReadAllBytes(filePath);

            try
            {
                using (TcpClient client = new TcpClient(ipAddress, port))
                {
                    NetworkStream stream = client.GetStream();

                    stream.Write(fileData, 0, fileData.Length);

                    Console.WriteLine("File transfer successfully completed!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while uploading the file: " + ex.Message);
            }
        }
    }
}

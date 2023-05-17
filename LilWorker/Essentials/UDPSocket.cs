using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LilWorker.Essentials
{
    public static class TaskExtensions
    {
        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
            {
                if (task != await Task.WhenAny(task, tcs.Task))
                    throw new OperationCanceledException(cancellationToken);
            }

            return await task;
        }
    }
    internal class UDPSocket
    {
        private UdpClient udpClient;
        private IPAddress address;
        private int port;
        public UDPSocket(IPAddress address, int port)
        {
            this.address = address;
            this.port = port;
            this.udpClient = new UdpClient();
        }

        public async Task<List<(string, string)>> SendAndReceiveWithTimeoutAsync(string message, TimeSpan timeout)
        {
            var responses = new List<(string, string)>();

            byte[] messageBytes = Encoding.ASCII.GetBytes(message);

            await udpClient.SendAsync(messageBytes, messageBytes.Length, new IPEndPoint(this.address, this.port));

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(timeout);

            while (!cts.Token.IsCancellationRequested)
            {
                try
                {
                    UdpReceiveResult receiveResult = await udpClient.ReceiveAsync().WithCancellation(cts.Token);
                    string ip = receiveResult.RemoteEndPoint.Address.ToString();
                    string id = Encoding.ASCII.GetString(receiveResult.Buffer);
                    responses.Add((ip,id));
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            return responses;
        }
    }
}

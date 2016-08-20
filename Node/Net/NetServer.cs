using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace Node.Net
{
    public sealed class NetServer
    {
        public readonly ConcurrentQueue<Node> Connections = new ConcurrentQueue<Node>();
        private readonly X509Certificate2 certificate;
        private readonly IPEndPoint endPoint;
        private bool active;

        public NetServer(IPEndPoint endPoint, X509Certificate2 cert = null)
        {
            this.certificate = cert;
            this.endPoint = endPoint;
        }

        public bool Send(IPEndPoint endPoint, byte[] data)
        {
            var client = Connections.FirstOrDefault(x => x.EndPoint.Address.Equals(endPoint.Address) && x.Connected);
            if (client != null)
            {
                client.Send(data);
                return true;
            }
            return false;
        }

        public bool IsConnected(IPEndPoint endPoint)
        {
            return Connections.Any(a => a.EndPoint.Address.Equals(endPoint.Address) && a.Connected);
        }

        public async void Start()
        {
            active = true;
            var listener = new TcpListener(endPoint);
            try
            {
                listener.Start();
                Started();
                while (active)
                {
                    var client =  new Node(await listener.AcceptTcpClientAsync(), certificate);
                    client.Received += Received;
                    client.Disconnected += Disconnect;
                    client.Start();
                    Connections.Enqueue(client);
                    Connected();
                }
            }
            catch (IOException ex)
            {
                Error(ex);
            }
            finally
            {
                listener.Stop();
                Stop();
            }
        }

        public void Stop()
        {
            active = false;
            Connections.TakeWhile(x => true);
            Stopped();
        }

        private void Disconnect(Node client)
        {
            Connections.TryDequeue(out client);
            Disconnected();
        }

        public event Action<byte[]> Received = data => { };
        public event Action Disconnected = () => { };
        public event Action Connected = () => { };
        public event Action Started = () => { };
        public event Action Stopped = () => { };
        public event Action<Exception> Error = ex => { };
    }
}
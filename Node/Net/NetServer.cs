using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Node.Net
{
    public sealed class NetServer
    {
        public readonly ConcurrentQueue<Node> Connections = new ConcurrentQueue<Node>();
        private readonly X509Certificate2 certificate;
        private readonly TcpListener listener;

        private bool active;

        public NetServer(IPEndPoint endpoint, X509Certificate2 cert = null)
        {
            listener = new TcpListener(endpoint);
            this.certificate = cert;
        }

        public bool Send(IPEndPoint endpoint, Message msg)
        {
            var client = Connections.FirstOrDefault(x => x.EndPoint.Address.Equals(endpoint.Address) && x.Connected);
            if (client != null)
            {
                client.Send(msg);
                return true;
            }
            return false;
        }

        public bool IsConnected(IPEndPoint endpoint)
        {
            return Connections.Any(a => a.EndPoint.Address.Equals(endpoint.Address) && a.Connected);
        }

        public async Task Start()
        {
            active = true;
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

        public event Action<Message> Received = msg => { };
        public event Action Disconnected = () => { };
        public event Action Connected = () => { };
        public event Action Started = () => { };
        public event Action Stopped = () => { };
        public event Action<Exception> Error = ex => { };
    }
}
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Node
{
    public sealed class Host
    {
        public readonly ConcurrentQueue<Node> Connections = new ConcurrentQueue<Node>();
        private readonly X509Certificate2 certificate;
        private readonly TcpListener listener;

        private bool active;

        public Host(IPEndPoint endpoint, X509Certificate2 cert = null)
        {
            listener = new TcpListener(endpoint);
            certificate = cert;
        }

        public void Publish<T>(IPEndPoint endpoint, T instance)
        {
            var client = GetClient(endpoint);
            client?.Publish(instance);
        }

        private Node GetClient(IPEndPoint endpoint)
        {
            return Connections.FirstOrDefault(x => x.EndPoint.Address.Equals(endpoint.Address) && x.Connected);
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
                    client.Subscribe<object>(msg => Received(msg));
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

        public void Subscribe<T>(Action<T> action)
        {
            Received += msg =>
            {
                if (msg is T)
                {
                    action((T)msg);
                }
            };
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

        private event Action<object> Received = msg => { };
        public event Action Disconnected = () => { };
        public event Action Connected = () => { };
        public event Action Started = () => { };
        public event Action Stopped = () => { };
        public event Action<Exception> Error = ex => { };
    }
}
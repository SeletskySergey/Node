using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Node
{
    public sealed class Host
    {
        private readonly ConcurrentDictionary<IPAddress, Node> nodes = new ConcurrentDictionary<IPAddress, Node>();
        private readonly TcpListener listener;

        private bool active;

        public Host(IPEndPoint endpoint)
        {
            listener = new TcpListener(endpoint);
        }

        public void Publish<T>(IPAddress address, T instance)
        {
            nodes[address]?.Publish(instance);
        }

        public void Start()
        {
            active = true;
            try
            {
                listener.Start();
                Started();
                while (active)
                {
                    var client =  new Node(listener.AcceptTcpClient());
                    client.Subscribe<object>(msg => Received(msg));
                    client.Disconnected += Disconnect;
                    client.Start();
                    var added = nodes.TryAdd(client.EndPoint.Address, client);
                    if (added)
                    {
                        Connected();
                    }
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
            nodes.Clear();
            Stopped();
        }

        private void Disconnect(Node client)
        {
            var removed = nodes.TryRemove(client.EndPoint.Address, out client);
            if (removed)
            {
                Disconnected();
            }
        }

        private event Action<object> Received = msg => { };
        public event Action Disconnected = () => { };
        public event Action Connected = () => { };
        public event Action Started = () => { };
        public event Action Stopped = () => { };
        public event Action<Exception> Error = ex => { };
    }
}
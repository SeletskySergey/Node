using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Node
{
    public sealed class Node
    {
        private bool active;
        private TcpClient client;
        private Task receiver;
        private Stream stream;

        public IPEndPoint EndPoint { get; set; }

        public Node(IPEndPoint endPoint)
        {
            EndPoint = endPoint;
            client = new TcpClient();
            receiver = new Task(Receiver);
        }

        public Node(TcpClient client)
        {
            this.client = client;
            EndPoint = (IPEndPoint)client.Client.RemoteEndPoint;
            receiver = new Task(Receiver);
        }

        public bool Connected => active && client.Connected;

        private void Send(Message msg)
        {
            try
            {
                var data = msg.Serialize();
                stream.Write(data, 0, data.Length);
                stream.Flush();
            }
            catch (IOException)
            {
                Disconnect();
            }   
        }

        private void Receiver()
        {
            while (active)
            {
                try
                {
                    var data = Message.Get(stream);
                    var msg = Message.Deserialize(data);
                    Received(msg);
                }
                catch (IOException)
                {
                    Disconnect();
                }
            }
        }

        public void Start()
        {
            try
            {
                active = true;
                if (!Connected)
                {
                    client = new TcpClient();
                    client.Connect(EndPoint);
                }

                stream = CreateNetworkStream();

                if (receiver.Status == TaskStatus.Created)
                {
                    receiver = new Task(Receiver);
                    receiver.Start();
                }
            }
            catch (SocketException)
            {
                Disconnect();
            }
        }

        private Stream CreateNetworkStream()
        {
            return client.GetStream();
        }

        public void Publish<T>(T instance)
        {
            var msg = new Message(0, 0, 0) { Contract = instance };
            Send(msg);
        }

        public void Subscribe<T>(Action<T> action)
        {
            Received += msg =>
            {
                if (msg.Contract is T)
                {
                    Task.Run(() => action((T)msg.Contract));
                }
            };
        }

        private void Disconnect()
        {
            if (active)
            {
                active = false;
                Disconnected(this);
            }
        }

        private event Action<Message> Received = msg => { };
        public event Action<Node> Disconnected = client => { };
    }
}


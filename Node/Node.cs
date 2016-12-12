using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Node
{
    public sealed class Node
    {
        private readonly X509Certificate2 certificate;
        private bool active;
        private TcpClient client;
        private Task receiver;
        private Stream stream;

        public IPEndPoint EndPoint { get; set; }

        public Node(IPEndPoint endPoint, X509Certificate2 cert = null)
        {
            certificate = cert;
            EndPoint = endPoint;
            client = new TcpClient();
            receiver = new Task(Receiver);
        }

        public Node(TcpClient client, X509Certificate2 cert = null)
        {
            certificate = cert;
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
                    if (certificate != null)
                    {
                        stream = CreateClientNetworkStream();
                    }
                }
                else
                {
                    if (certificate != null)
                    {
                        stream = CreateServerNetworkStream();
                    }
                }

                if (certificate == null)
                {
                    stream = CreateNetworkStream();
                }

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

        private Stream CreateClientNetworkStream()
        {
            var stream = new SslStream(client.GetStream(), false, RemoteCertificateValidation, LocalCertificateSelection, EncryptionPolicy.RequireEncryption);
            stream.AuthenticateAsClient(EndPoint.Address.ToString(), new X509CertificateCollection(new[] { certificate }), SslProtocols.Default, true);
            stream.DisplaySecurityLevel();
            stream.DisplaySecurityServices();
            stream.DisplayCertificateInformation();
            stream.DisplayStreamProperties();
            return stream;
        }

        private Stream CreateServerNetworkStream()
        {
            var stream = new SslStream(client.GetStream(), false, RemoteCertificateValidation, LocalCertificateSelection, EncryptionPolicy.RequireEncryption);
            stream.AuthenticateAsServer(certificate, true, SslProtocols.Default, true);
            stream.DisplaySecurityLevel();
            stream.DisplaySecurityServices();
            stream.DisplayCertificateInformation();
            stream.DisplayStreamProperties();
            return stream;
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

        public event RemoteCertificateValidationCallback RemoteCertificateValidation = (sender, certificate, chain, sslPolicyErrors) =>
        {
            return true;
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

            // Do not allow this client to communicate with unauthenticated servers.
            return false;
        };

        public event LocalCertificateSelectionCallback LocalCertificateSelection = (sender, targetHost, localCertificates, remoteCertificate, acceptableIssuers) =>
        {
            return localCertificates.Cast<X509Certificate>().FirstOrDefault();
        };
    }
}


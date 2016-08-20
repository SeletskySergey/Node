using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Node.Net;

namespace Node
{
    internal class App
    {
        private static void Main()
        {
            Task.Factory.StartNew(Server);
            Task.Factory.StartNew(Client);
            Console.ReadLine();
        }

        private static void Server()
        {
            var count = 0;
            var watch = new Stopwatch();

            var cert = new X509Certificate2("cert.pfx", "Gagarin77$");
   
            var dns = Dns.GetHostEntry("127.0.0.1").AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
            var endPoint = new IPEndPoint(dns, 17777);
            var server = new NetServer(endPoint);

            server.Received += d =>
            {
                if (count == 0)
                {
                    watch.Start();
                }
                Interlocked.Increment(ref count);
                if (count >= 100000)
                {
                    watch.Stop();
                    Console.WriteLine("Server {0:0 000} Msg / {2}Kb : {1} ms.", count, watch.ElapsedMilliseconds, d.Length/1024.0);
                    watch.Reset();
                    count = 0;
                }
            };

            server.Disconnected += () => Console.WriteLine("Client is disconnected");
            server.Connected += () => Console.WriteLine("Client accepted!");
            server.Started += () => Console.WriteLine("Waiting for a connection...");
            server.Error += ex => Console.WriteLine(ex.Message);
            server.Stopped += () => Console.WriteLine("Server stoped !");
            server.Start();

            var msg = new Message(0, 0, 0) {Data = File.ReadAllBytes("Node.exe")};
            var data = Message.Serialize(msg);

            try
            {
                while (true)
                {
                    if (server.IsConnected(endPoint))
                    {
                        server.Send(endPoint, data);
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey();
        }

        private static void Client()
        {
            var count = 0;
            var watch = new Stopwatch();

            Console.ForegroundColor = ConsoleColor.Green;
            var dns = Dns.GetHostEntry("127.0.0.1").AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
            var endPoint = new IPEndPoint(dns, 17777);

            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            X509CertificateCollection certs = store.Certificates.Find(X509FindType.FindBySubjectName, "Sergey Seletsky", true);
            var cert = certs.Cast<X509Certificate2>().FirstOrDefault();

            var client = new Node(endPoint);

            client.Received += d =>
            {
                if (count == 0)
                {
                    watch.Start();
                }
                Interlocked.Increment(ref count);
                if (count >= 100000)
                {
                    watch.Stop();
                    Console.WriteLine("Client {0:0 000} Msg / {2}Kb : {1} ms.", count, watch.ElapsedMilliseconds, d.Length/1024.0);
                    watch.Reset();
                    count = 0;
                }
            };

            client.Disconnected += c =>
            {
                if (c.Connected == false)
                {
                    Console.WriteLine("Check connecting...");
                    Thread.Sleep(5000);
                    c.Start();
                }
            };

            client.Start();

            var msg = new Message(0, 0, 0) { Data = File.ReadAllBytes("Node.exe") };

            var data = Message.Serialize(msg);

            while (true)
            {
                if (client.Connected)
                {
                    client.Send(data);
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }
    }
}
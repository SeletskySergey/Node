using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Node
{
    internal class App
    {
        private static readonly IPEndPoint local;

        static App()
        {
            var ip = Dns.GetHostEntry("127.0.0.1").AddressList.First(x => x.AddressFamily == AddressFamily.InterNetwork);
            local = new IPEndPoint(ip, 17777);
        }

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

            var server = new Host(local);

            server.Subscribe<TestContractClass>(msg =>
            {
                if (count == 0)
                {
                    watch.Start();
                }
                Interlocked.Increment(ref count);
                if (count >= 10000)
                {
                    watch.Stop();
                    Console.WriteLine("Server {0:0 000} Msg / {2}Kb : {1} ms.", count, watch.ElapsedMilliseconds, msg.Three.Length / 1024.0);
                    watch.Reset();
                    count = 0;
                }
                server.Publish(local, msg);
            });

            server.Disconnected += () => Console.WriteLine("Client is disconnected");
            server.Connected += () => Console.WriteLine("Client accepted!");
            server.Started += () => Console.WriteLine("Waiting for a connection...");
            server.Error += ex => Console.WriteLine(ex.Message);
            server.Stopped += () => Console.WriteLine("Server stoped !");
            server.Start();

            server.Publish(local, new TestContractClass()); //Init recursive sending
            Console.ReadKey();
        }

        private static void Client()
        {
            var count = 0;
            var watch = new Stopwatch();

            Console.ForegroundColor = ConsoleColor.Green;

            var client = new Node(local);

            client.Subscribe<TestContractClass>(msg =>
            {
                if (count == 0)
                {
                    watch.Start();
                }
                Interlocked.Increment(ref count);
                if (count >= 10000)
                {
                    watch.Stop();
                    Console.WriteLine("Client {0:0 000} Msg / {2}Kb : {1} ms.", count, watch.ElapsedMilliseconds, msg.Three.Length / 1024.0);
                    watch.Reset();
                    count = 0;
                }
                client.Publish(msg);
            });

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
            client.Publish(new TestContractClass()); // Init recursive sending
        }
    }
}
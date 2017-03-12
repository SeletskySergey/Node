using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
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

        const int globalCount = 10000;

        static TestContract instance = new TestContract()
        {
            One = "sdfsdfsdfsdfsdfsdfsdfsdfsdfsdfsdfsdfsdfsdfsdfsdfsdfsdfsdfsdfsdfsdfsdfsdf",
            Count = 35346457567,
            Three = new byte[400],
            Two = DateTime.Now,
            Active = true,
            Strings = new List<string> { "", "one", "two", "one", "two", "one", "two", "one", "two" },
            TestEnum = TestContract.Test.Three
        };

        private static void Main()
        {
            RuntimeTypeModel.Default.Add(typeof(TestContract), true);
            RuntimeTypeModel.Default.CompileInPlace();

            Task.Factory.StartNew(Server);
            Task.Factory.StartNew(Client);
            Console.ReadLine();
        }

        private static void Server()
        {
            var count = 0;
            var sw = new Stopwatch();

            var server = new Host(local);

            server.Subscribe<TestContract>(msg =>
            {
                if (count == 0)
                {
                    sw.Start();
                }
                Interlocked.Increment(ref count);
                if (count >= globalCount)
                {
                    sw.Stop();
                    Console.WriteLine($"Server {count} Msg / {msg.Three.Length / 1024.0}Kb : {sw.ElapsedMilliseconds} ms.");
                    sw.Reset();
                    count = 0;
                }
                server.Publish(local.Address, msg);
            });

            server.Disconnected += () => Console.WriteLine("Client is disconnected");
            server.Connected += () => Console.WriteLine("Client accepted!");
            server.Started += () => Console.WriteLine("Waiting for a connection...");
            server.Error += ex => Console.WriteLine(ex.Message);
            server.Stopped += () => Console.WriteLine("Server stoped !");
            server.Start();

            server.Publish(local.Address, instance); //Init recursive sending
            Console.ReadKey();
        }

        private static void Client()
        {
            var count = 0;
            var sw = new Stopwatch();

            Console.ForegroundColor = ConsoleColor.Green;

            var client = new Node(local);

            client.Subscribe<TestContract>(msg =>
            {
                if (count == 0)
                {
                    sw.Start();
                }
                Interlocked.Increment(ref count);
                if (count >= globalCount)
                {
                    sw.Stop();
                    Console.WriteLine($"Client {count} Msg / {msg.Three.Length / 1024.0}Kb : {sw.ElapsedMilliseconds} ms.");
                    sw.Reset();
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
            client.Publish(instance); // Init recursive sending
        }
    }
}
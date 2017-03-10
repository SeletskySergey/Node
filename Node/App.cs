using Newtonsoft.Json;
using ProtoBuf;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Node
{
    internal class App
    {
        public static void JsonTest(int count)
        {
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < count; i++)
            {
                var x = JsonConvert.SerializeObject(instance);
                var bytes = Encoding.UTF8.GetBytes(x);
            }
            sw.Stop();
            Console.WriteLine($"JSON Serialize: {sw.ElapsedMilliseconds} ms.");

            var json = JsonConvert.SerializeObject(instance);
            var l1 = Encoding.UTF8.GetBytes(json);
            Console.WriteLine($"JSON Size: {l1.Length} bytes.");

            sw.Restart();
            for (var i = 0; i < count; i++)
            {
                var x = JsonConvert.DeserializeObject(json, typeof(TestContractClass));
            }
            sw.Stop();
            Console.WriteLine($"JSON Deserialize: {sw.ElapsedMilliseconds} ms.\n");
        }

        public static void ProtoTest(int count)
        {
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < count; i++)
            {
                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, instance);
                    var bytes = ms.ToArray();
                }
            }
            sw.Stop();
            Console.WriteLine($"PROTO Serialize: {sw.ElapsedMilliseconds} ms.");


            byte[] data;
            using (var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, instance);
                data = ms.ToArray();
            }
            Console.WriteLine($"PROTO Size: {data.Length} bytes.");

            sw.Start();
            for (var i = 0; i < count; i++)
            {
                using (var ms = new MemoryStream(data))
                {
                    var o = Serializer.Deserialize(typeof(TestContractClass), ms);
                }
            }
            sw.Stop();
            Console.WriteLine($"PROTO Deserialize: {sw.ElapsedMilliseconds} ms.\n");
        }

        public static void NanoTest(int count)
        {
            var serializer = NanoSerializer.Build<TestContractClass>();

            var sw = Stopwatch.StartNew();
            for (var i = 0; i < count; i++)
            {
                var bytes = serializer.Serialize(instance);
            }
            sw.Stop();
            Console.WriteLine($"NANO Serialize: {sw.ElapsedMilliseconds} ms.");

            byte[] data5 = serializer.Serialize(instance);

            Console.WriteLine($"NANO Size: {data5.Length} bytes.");
            sw.Start();
            for (var i = 0; i < count; i++)
            {
                var cls = serializer.Deserialize(data5);
            }
            sw.Stop();
            Console.WriteLine($"NANO Deserialize: {sw.ElapsedMilliseconds} ms.\n");
        }

        private static readonly IPEndPoint local;

        static App()
        {
            var ip = Dns.GetHostEntry("127.0.0.1").AddressList.First(x => x.AddressFamily == AddressFamily.InterNetwork);
            local = new IPEndPoint(ip, 17777);
        }

        const int globalCount = 10000;

        static TestContractClass instance = new TestContractClass()
        {
            One = "sdfsdfsdfsdfsdfsdfsdfsdfsdfsdfsdfsdfsdfsdfsdfsdfsdfsdfsdfsdfsdfsdfsdfsdf",
            Count = 35346457567,
            Three = new byte[400],
            Two = DateTime.Now,
            Active = true,
            Strings = new List<string> { "", "one", "two", "one", "two", "one", "two", "one", "two" },
            TestEnum = TestEnum.Three
        };

        private static void Main()
        {
            //RuntimeTypeModel.Default.Add(typeof(TestContractClass), true);
            //RuntimeTypeModel.Default.CompileInPlace();

            var count = 1000000;

            NanoTest(count);
            //ProtoTest(count);
            //JsonTest(count);


            //Task.Factory.StartNew(Server);
            //Task.Factory.StartNew(Client);
            Console.ReadLine();
        }

        private static void Server()
        {
            var count = 0;
            var sw = new Stopwatch();

            var server = new Host(local);

            server.Subscribe<TestContractClass>(msg =>
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

            client.Subscribe<TestContractClass>(msg =>
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
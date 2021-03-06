﻿using Consul;
using MachinaAurum.Artemis.Server;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace FakeServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Annourcer.UseConsul("/api/api1", 8081, 18081, new[] { "artemis-gateway" });
            OpenFakeHttpServer(8081);
            Annourcer.UseConsul("/api/api2", 8082, 18082, new[] { "artemis-gateway" });
            OpenFakeHttpServer(8082);

            Console.ReadLine();
        }

        private static void OpenFakeHttpServer(int port)
        {
            Console.WriteLine($"Openning port {port}");
            int i = 0;
            var li = new HttpListener();
            li.Prefixes.Add($"http://+:{port}/api/");
            li.Start();
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    var ctx = li.GetContext();
                    using (ctx.Response)
                    {
                        ctx.Response.SendChunked = false;
                        ctx.Response.StatusCode = 200;

                        Console.WriteLine($"at {port} Correlation {ctx.Request.Headers["X-CORRELATION"]}");

                        using (var sw = new StreamWriter(ctx.Response.OutputStream))
                        {
                            ++i;
                            var str = $"{i}!!!";
                            ctx.Response.ContentLength64 = str.Length;
                            sw.Write(str);
                        }
                    }
                }
            });
        }
    }
}

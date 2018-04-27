using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Diagnostics;

namespace IdEpi.WebClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = $"Web Client {Process.GetCurrentProcess().Id}";

            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}

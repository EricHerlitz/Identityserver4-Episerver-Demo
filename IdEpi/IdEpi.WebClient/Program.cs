using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;

namespace IdEpi.WebClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "Web Client";

            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}

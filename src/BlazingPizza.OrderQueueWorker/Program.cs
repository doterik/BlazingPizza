using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazingPizza.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BlazingPizza.OrderQueueWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddDbContext<PizzaStoreContext>(options => 
                    {
                        options.UseSqlServer(
                            Environment.GetEnvironmentVariable("SQL_SERVER")
                        );
                    });
                    services.AddHostedService<Worker>();
                });
    }
}

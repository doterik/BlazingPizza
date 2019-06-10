using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlazingPizza.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace BlazingPizza.OrderQueueWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private IServiceProvider _serviceProvider;

        public CloudStorageAccount Account { get; set; }
        public CloudQueue Queue { get; set; }

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public override async Task StartAsync(CancellationToken stoppingToken)
        {
            var str = Environment.GetEnvironmentVariable("STORAGE_ACCOUNT");
            CloudStorageAccount account;
            if(CloudStorageAccount.TryParse(str, out account))
            {
                Account = account;
                Queue = Account.CreateCloudQueueClient().GetQueueReference("incomingorders");
                if(!await Queue.ExistsAsync())
                    await Queue.CreateIfNotExistsAsync();
            }
            await base.StartAsync(stoppingToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                try
                {
                    var msg = await Queue.GetMessageAsync();
                    if(msg != null)
                    {
                        var order = JsonConvert.DeserializeObject<Order>(msg.AsString);

                        using (IServiceScope scope = _serviceProvider.CreateScope())
                        {
                            var context = scope.ServiceProvider.GetRequiredService<PizzaStoreContext>();
                            context.Orders.Attach(order);
                            await context.SaveChangesAsync();
                            await Queue.DeleteMessageAsync(msg);
                        }
                    }
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Error during dequeue");
                }
                
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}

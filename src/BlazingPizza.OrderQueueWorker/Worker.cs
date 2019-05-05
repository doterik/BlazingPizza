using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        public CloudStorageAccount Account { get; set; }
        public CloudQueue Queue { get; set; }
        private PizzaStoreContext _db;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
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
                var msg = await Queue.GetMessageAsync();
                if(msg != null)
                {
                    var order = JsonConvert.DeserializeObject<Order>(msg.AsString);
                    _db.Orders.Attach(order);
                    await _db.SaveChangesAsync();
                }
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}

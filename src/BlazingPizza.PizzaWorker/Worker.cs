using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlazingPizza.Shared;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using BlazingPizza.OrderStatusUpdates;
using static BlazingPizza.OrderStatusUpdates.PizzaOrderStatus;
using BlazingPizza.OrderStatusClient;

namespace BlazingPizza.PizzaWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private IServiceProvider _serviceProvider;

        public CloudStorageAccount Account { get; set; }
        public CloudQueue PizzaOrdersQueue { get; set; }
        public CloudQueue PizzaDeliveryQueue { get; set; }
        public IConfiguration Configuration { get; }

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            Configuration = configuration;
        }

        public override async Task StartAsync(CancellationToken stoppingToken)
        {
            var str = Configuration["Azure:Storage:ConnectionString"];
            CloudStorageAccount account;
            if (CloudStorageAccount.TryParse(str, out account))
            {
                Account = account;
                PizzaOrdersQueue = Account.CreateCloudQueueClient().GetQueueReference("pizzaorders");
                if (!await PizzaOrdersQueue.ExistsAsync())
                    await PizzaOrdersQueue.CreateIfNotExistsAsync();
                PizzaDeliveryQueue = Account.CreateCloudQueueClient().GetQueueReference("pizzadeliveries");
                if (!await PizzaDeliveryQueue.ExistsAsync())
                    await PizzaDeliveryQueue.CreateIfNotExistsAsync();
            }
            await base.StartAsync(stoppingToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Pizza worker running at: {time}", DateTimeOffset.Now);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var msg = await PizzaOrdersQueue.GetMessageAsync();
                    if (msg != null)
                    {
                        var order = JsonConvert.DeserializeObject<Order>(msg.AsString);

                        using (IServiceScope scope = _serviceProvider.CreateScope())
                        {
                            // Save the order to the DB
                            var context = scope.ServiceProvider.GetRequiredService<PizzaStoreContext>();
                            order.Status = "Preparing";
                            context.Orders.Attach(order);
                            await context.SaveChangesAsync();
                            await PizzaOrdersQueue.DeleteMessageAsync(msg);
                            _logger.LogInformation($"Preparing order {order.OrderId}");

                            // Send Preparing status
                            var channel = new Channel("localhost:50051", ChannelCredentials.Insecure);
                            var client = new PizzaOrderStatusClient(channel);
                            var ack = await client.SendStatusAsync(order.ToStatusUpdate());
                            _logger.LogInformation($"Status update ack: {ack.Message}");

                            await Task.Delay(10000, stoppingToken); // Prepare the pizza

                            // Put the pizza order on the delivery queue
                            await PizzaDeliveryQueue.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(order)));
                            _logger.LogInformation($"Order {order.OrderId} sent for delivery");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error preparing pizza order: {ex.Message}");
                }

                // Mandatory Pizza Worker break
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}

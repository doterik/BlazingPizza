using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlazingPizza.OrderStatusClient;
using BlazingPizza.OrderStatusUpdates;
using BlazingPizza.Shared;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using static BlazingPizza.OrderStatusUpdates.PizzaOrderStatus;

namespace BlazingPizza.PizzaDeliveryWorker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private IServiceProvider _serviceProvider;

        public CloudStorageAccount Account { get; set; }
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
                PizzaDeliveryQueue = Account.CreateCloudQueueClient().GetQueueReference("pizzadeliveries");
                if (!await PizzaDeliveryQueue.ExistsAsync())
                    await PizzaDeliveryQueue.CreateIfNotExistsAsync();
            }
            await base.StartAsync(stoppingToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Pizza delivery worker running at: {time}", DateTimeOffset.Now);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var msg = await PizzaDeliveryQueue.GetMessageAsync();
                    if (msg != null)
                    {
                        var order = JsonConvert.DeserializeObject<Order>(msg.AsString);

                        using (IServiceScope scope = _serviceProvider.CreateScope())
                        {
                            // Save the order to the DB
                            var context = scope.ServiceProvider.GetRequiredService<PizzaStoreContext>();
                            order.Status = "Out for delivery";
                            context.Orders.Attach(order);
                            await context.SaveChangesAsync();
                            await PizzaDeliveryQueue.DeleteMessageAsync(msg);
                            _logger.LogInformation($"Order {order.OrderId} out for delivery");

                            // Send delivery status 
                            var startPosition = order.CurrentLocation;
                            var dispatchTime = DateTime.Now;
                            var deliveryDuration = TimeSpan.FromMinutes(1);
                            var proportionOfDeliveryCompleted = 0d;
                            var channel = new Channel("localhost:50051", ChannelCredentials.Insecure);
                            var client = new PizzaOrderStatusClient(channel);
                            Ack ack;

                            while (proportionOfDeliveryCompleted < 1)
                            {
                                proportionOfDeliveryCompleted = Math.Min(1, (DateTime.Now - dispatchTime).TotalMilliseconds / deliveryDuration.TotalMilliseconds);
                                _logger.LogInformation($"Proportion complete for order {order.OrderId}: {proportionOfDeliveryCompleted}");
                                order.CurrentLocation = LatLong.Interpolate(startPosition, order.DeliveryLocation, proportionOfDeliveryCompleted);

                                ack = await client.SendStatusAsync(order.ToStatusUpdate());
                                _logger.LogInformation($"Status update ack: {ack.Message}");

                                await Task.Delay(1000, stoppingToken);
                            }

                            order.Status = "Delivered";
                            context.Orders.Attach(order);
                            await context.SaveChangesAsync();
                            _logger.LogInformation($"Order {order.OrderId} delivered");

                            ack = await client.SendStatusAsync(order.ToStatusUpdate());
                            _logger.LogInformation($"Status update ack: {ack.Message}");

                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error deliverying pizza order: {ex.Message}");
                }

                // Mandatory Pizza delivery worker break
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlazingPizza.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Grpc.Core;
using PizzaStatusClient = Pizza.PizzaStatus.PizzaStatusClient;
using PizzaStatusReport = Pizza.PizzaStatusReport;

namespace BlazingPizza.OrderStatusTracker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private IServiceProvider _serviceProvider;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<PizzaStoreContext>();
                    var subDate = DateTime.Now.Subtract(TimeSpan.FromMinutes(2));
                    var activeOrders = context.Orders.Where(x => 
                        x.CreatedTime > subDate).ToList();

                    foreach (var order in activeOrders)
                    {
                        var statusText = string.Empty;
                        var orderId = order.OrderId;
                        var userId = order.UserId;
                        var dispatchTime = order.CreatedTime.AddSeconds(10);
                        var deliveryDuration = TimeSpan.FromMinutes(1);

                        if (DateTime.Now < dispatchTime)
                        {
                            statusText = "Preparing";
                        }
                        else if (DateTime.Now < dispatchTime + deliveryDuration)
                        {
                            statusText = "Out for delivery";
                        }
                        else
                        {
                            statusText = "Delivered";
                        }

                        var channel = new Channel("localhost:50051", ChannelCredentials.Insecure);
                        var client = new PizzaStatusClient(channel);

                        try
                        {
                            var ack = await client.SendStatusAsync(new PizzaStatusReport
                            {
                                UserId = userId,
                                OrderId = orderId,
                                StatusText = statusText
                            });

                            _logger.LogInformation($"-----------------------");
                            _logger.LogInformation(ack.Message);
                            _logger.LogInformation($"-----------------------");
                        }
                        catch(Exception ex)
                        {
                            _logger.LogError(ex, "Error during gRPC send");
                        }
                        
                        await channel.ShutdownAsync();
                    }
                }
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}

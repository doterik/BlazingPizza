using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using BlazingPizza.OrderStatusUpdates;

namespace BlazingPizza.Server.Services
{
    public class PizzaOrderStatusService : OrderStatusUpdates.PizzaOrderStatus.PizzaOrderStatusBase
    {
        private readonly OrderStatusUpdater _orderStatusUpdater;

        public PizzaOrderStatusService(OrderStatusUpdater orderStatusUpdater)
        {
            _orderStatusUpdater = orderStatusUpdater;
        }

        public override Task<Ack> SendStatus(StatusUpdate update, ServerCallContext context)
        {
            _orderStatusUpdater.UpdateOrderStatus(new OrderStatus()
            {
                OrderId = update.OrderId,
                UserId = update.UserId,
                Status = update.StatusText,
                CurrentLocation = new LatLong(update.Lat, update.Long)
            });

            return Task.FromResult(new Ack
            {
                Message = string.Format($"Received status {update.StatusText} for order {update.OrderId}.")
            });
        }
    }
}

using BlazingPizza.OrderStatusUpdates;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlazingPizza.OrderStatusClient
{
    public static class OrderExtensions
    {
        public static StatusUpdate ToStatusUpdate(this Order order)
        {
            return new StatusUpdate
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                StatusText = order.Status,
                Lat = order.CurrentLocation.Latitude,
                Long = order.CurrentLocation.Longitude
            };
        }
    }
}

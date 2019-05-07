using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazingPizza
{
    public class OrderStatusUpdater
    {
        public event EventHandler<OrderStatus> OrderStatusChanged;

        public void UpdateOrderStatus(OrderStatus orderStatus)
        {
            Console.WriteLine("PizzaStore.UpdateOrderStatus");
            Console.WriteLine($"OrderStatusChanged == null: {OrderStatusChanged == null}");
            OrderStatusChanged?.Invoke(this, orderStatus);
        }
    }
}

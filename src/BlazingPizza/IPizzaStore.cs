using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazingPizza
{
    public interface IPizzaStore
    {
        Task<List<Order>> GetOrders(string userId);
        Task<Order> GetOrder(int orderId, string userId);
        Task<List<PizzaSpecial>> GetSpecials();
        Task<List<Topping>> GetToppings();
        Task PlaceOrder(Order order, string userId);
    }
}
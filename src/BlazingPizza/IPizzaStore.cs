using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazingPizza
{
    public interface IPizzaStore
    {
        Task<List<OrderWithStatus>> GetOrders(string userId);
        Task<OrderWithStatus> GetOrderWithStatus(int orderId, string userId);
        Task<List<PizzaSpecial>> GetSpecials();
        Task<List<Topping>> GetToppings();
        Task PlaceOrder(Order order, string userId);
    }
}
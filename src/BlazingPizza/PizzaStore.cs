using BlazingPizza.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazingPizza
{
    public class PizzaStore : IPizzaStore
    {
        private PizzaStoreContext _db;

        public PizzaStore(PizzaStoreContext db)
        {
            _db = db;
        }

        public async Task<List<PizzaSpecial>> GetSpecials()
        {
            return await _db.Specials.OrderByDescending(s => s.BasePrice).ToListAsync();
        }

        public async Task<List<Topping>> GetToppings()
        {
            return await _db.Toppings.OrderBy(t => t.Name).ToListAsync();
        }

        public async Task<List<OrderWithStatus>> GetOrders(string userId)
        {
            var orders = await _db.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Pizzas).ThenInclude(p => p.Special)
                .Include(o => o.Pizzas).ThenInclude(p => p.Toppings).ThenInclude(t => t.Topping)
                .OrderByDescending(o => o.CreatedTime)
                .ToListAsync();

            return orders.Select(o => OrderWithStatus.FromOrder(o)).ToList();
        }

        public async Task<OrderWithStatus> GetOrderWithStatus(int orderId, string userId)
        {
            var order = await _db.Orders
                .Where(o => o.OrderId == orderId)
                .Where(o => o.UserId == userId)
                .Include(o => o.Pizzas).ThenInclude(p => p.Special)
                .Include(o => o.Pizzas).ThenInclude(p => p.Toppings).ThenInclude(t => t.Topping)
                .SingleOrDefaultAsync();

            if (order == null) return null;

            return OrderWithStatus.FromOrder(order);
        }

        public async Task PlaceOrder(Order order, string userId)
        {
            order.CreatedTime = DateTime.Now;
            order.DeliveryLocation = new LatLong(51.5001, -0.1239);
            order.UserId = userId;

            _db.Orders.Attach(order);
            await _db.SaveChangesAsync();
        }
    }
}

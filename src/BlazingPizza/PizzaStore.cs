using BlazingPizza.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazingPizza
{
    public class PizzaStore : IPizzaStore
    {
        private PizzaStoreContext _db;

        public IConfiguration Configuration { get; }

        public PizzaStore(PizzaStoreContext db, IConfiguration configuration)
        {
            _db = db;
            Configuration = configuration;
        }

        public async Task<List<PizzaSpecial>> GetSpecials()
        {
            return await _db.Specials.OrderByDescending(s => s.BasePrice).ToListAsync();
        }

        public async Task<List<Topping>> GetToppings()
        {
            return await _db.Toppings.OrderBy(t => t.Name).ToListAsync();
        }

        public async Task<List<Order>> GetOrders(string userId)
        {
            var orders = await _db.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Pizzas).ThenInclude(p => p.Special)
                .Include(o => o.Pizzas).ThenInclude(p => p.Toppings).ThenInclude(t => t.Topping)
                .OrderByDescending(o => o.CreatedTime)
                .ToListAsync();

            return orders;
        }

        public async Task<Order> GetOrder(int orderId, string userId)
        {
            var order = await _db.Orders
                .Where(o => o.OrderId == orderId)
                .Where(o => o.UserId == userId)
                .Include(o => o.Pizzas).ThenInclude(p => p.Special)
                .Include(o => o.Pizzas).ThenInclude(p => p.Toppings).ThenInclude(t => t.Topping)
                .SingleOrDefaultAsync();

            if (order == null) return null;

            return order;
        }

        public async Task PlaceOrder(Order order, string userId)
        {
            // TODO: change to be an enqueue process here
            var str = Configuration["Azure:Storage:ConnectionString"];
            CloudStorageAccount account;
            if(CloudStorageAccount.TryParse(str, out account) == true)
            {
                order.CreatedTime = DateTime.Now;
                order.DeliveryLocation = new LatLong(47.611860, -122.331926);
                order.CurrentLocation = new LatLong(47.610110, -122.342250);
                order.Status = "Ordered";
                order.UserId = userId;

                // queue the order
                var qName = "pizzaorders";
                await account.CreateCloudQueueClient()
                    .GetQueueReference(qName).CreateIfNotExistsAsync();

                var queue = account.CreateCloudQueueClient()
                    .GetQueueReference(qName);

                await queue.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(order)));
            }
        }
    }
}

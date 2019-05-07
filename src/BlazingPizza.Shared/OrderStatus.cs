using System;
using System.Collections.Generic;
using System.Text;

namespace BlazingPizza
{
    public class OrderStatus
    {
        public int OrderId { get; set; }
        public string UserId { get; set; }
        public string Status { get; set; }
        public LatLong CurrentLocation { get; set; }
    }
}

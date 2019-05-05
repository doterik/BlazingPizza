using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Pizza;

namespace BlazingPizza.Server.Services
{
    public class PizzaStatusService : PizzaStatus.PizzaStatusBase
    {
        public override Task<Ack> SendStatus(PizzaStatusReport request, ServerCallContext context)
        {
            return Task.FromResult(new Ack
            {
                Message = string.Format($"Received status {request.StatusText} for order {request.OrderId} from user {request.UserId}.")
            });
        }
    }
}

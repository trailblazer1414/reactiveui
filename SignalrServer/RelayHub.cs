using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Connections.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace SignalrServer
{
    public interface IRelayHubClient
    {
        Task SendOrder(Order ord);
        Task SendInitOrders(List<Order> ord);
        Task SendOrderUpd(OrderUpd ord);
        Task SendOrderOtherUpd(OrderOtherUpd order);
    }

    [Authorize]
    public class RelayHub : Hub<IRelayHubClient>
    {
        private ILogger<RelayHub> _logger;
        private CacheContainer _cache;

        public RelayHub(ILogger<RelayHub> logger, CacheContainer cache)
        {
            _logger = logger;
            _cache = cache;
        }

        private string UserId
        {
            get
            {
                return Context.User.Identity.Name;
            }
        }
        private string ConnectionId
        {
            get
            {
                return Context.ConnectionId;
            }
        }


        public override async Task OnConnectedAsync()
        {
            try
            {
                _logger.LogInformation("Connected : " + Context.ConnectionId + " User :" + UserId + " On :" + Context.Features.Get<IHttpTransportFeature>().TransportType);
                await base.OnConnectedAsync();
                await Clients.Caller.SendInitOrders(_cache.OrderCollection.Values.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError("Error connecting: " + ex.Message);
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                if (exception != null)
                {
                    _logger.LogInformation("Error:" + exception);
                }

                _logger.LogInformation("Disconnected :" + Context.ConnectionId + " User :" + UserId + " On :" + Context.Features.Get<IHttpTransportFeature>().TransportType);
                await base.OnDisconnectedAsync(exception);

            }
            catch (Exception ex)
            {
                _logger.LogError("Error disconnecting: " + ex.Message);
            }
        }

    }
    
}

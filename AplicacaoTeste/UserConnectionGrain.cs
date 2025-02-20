using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace AplicacaoTeste;

public class UserConnectionGrain : Grain, IUserConnectionGrain
{
    private readonly IPersistentState<List<string>> _connectedUsers;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly ILogger<UserConnectionGrain> _logger;

    public UserConnectionGrain(
        [PersistentState("connectedUsers", "RedisStore")] 
        IPersistentState<List<string>> connectedUsers,
        IHubContext<ChatHub> hubContext, ILogger<UserConnectionGrain> logger)
    {
        _connectedUsers = connectedUsers;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task UserConnected(string userName)
    {
        if (!_connectedUsers.State.Contains(userName))
        {
            _connectedUsers.State.Add(userName);
            await _connectedUsers.WriteStateAsync();
            await _hubContext.Clients.All.SendAsync("ConnectedUsersUpdated", _connectedUsers.State);
        }
    }

    public async Task UserDisconnected(string userName)
    {
        if (_connectedUsers.State.Contains(userName))
        {
            _connectedUsers.State.Remove(userName);
            await _connectedUsers.WriteStateAsync();
            await _hubContext.Clients.All.SendAsync("ConnectedUsersUpdated", _connectedUsers.State);
        }
    }

    public async Task<List<string>> GetConnectedUsers()
    {
        return _connectedUsers.State;
    }
    
    
    public async ValueTask<string> SayHello(string greeting)
    {
        UserConnected(greeting);
        _logger.LogInformation("SayHello message received: greeting is {greeting}", greeting);
        return $"\n Client said: {greeting}, so HelloGrain says: Hello back! Value in state is: {this.GetPrimaryKey()}";
    }
}
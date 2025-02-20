namespace AplicacaoTeste;

public interface IUserConnectionGrain : IGrainWithGuidKey
{
    Task UserConnected(string userName);
    Task UserDisconnected(string userName);
    Task<List<string>> GetConnectedUsers();
    
    ValueTask<string> SayHello(string greeting);
}
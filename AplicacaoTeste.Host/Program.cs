using AplicacaoTeste;
using Orleans.Hosting;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


builder.Host.UseOrleans(siloBuilder =>
{
    siloBuilder.UseLocalhostClustering();


    siloBuilder.AddRedisGrainStorage("RedisStore",
        configureOptions: options =>
        {
            options.ConfigurationOptions = new ConfigurationOptions();
            options.ConfigurationOptions.EndPoints.Add("redis:6379"); // Configure a conexão com o Redis
            // Outras opções de configuração do Redis podem ser adicionadas aqui 
        });

    
    // Add Orleans Dashboard
    siloBuilder.UseDashboard(options =>
    {
        /* Dashboard options can be configured here */
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR(); // Adiciona o serviço SignalR

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast");

app.MapGet("/hello/{name}", async (string name, IClusterClient client) =>
{
    var grain = client.GetGrain<IUserConnectionGrain>(Guid.NewGuid());
    return await grain.SayHello($"Hello from {name}!");
});


app.MapGet("/Teste", async ( IClusterClient client) =>
{
    var ids = new List<Guid>();
    for (int j = 0; j < 5; j++)
    {
        var id = Guid.NewGuid();
        var grain = client.GetGrain<IUserConnectionGrain>(id);
        for (int i = 0; i < 5000; i++)
        {
            await grain.SayHello($"Hello player {i} to room {j}!");
        }
        ids.Add(id);
    }

    return ids;
});

app.MapGet("/Teste/{id}", async (Guid id, IClusterClient client) =>
{
    var grain = client.GetGrain<IUserConnectionGrain>(id);
    return await grain.GetConnectedUsers();
});

app.UseSwagger();
app.UseSwaggerUI();

app.MapHub<ChatHub>("/chathub"); // Mapeia o Hub SignalR


app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
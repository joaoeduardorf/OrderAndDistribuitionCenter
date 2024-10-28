using Microsoft.EntityFrameworkCore;
using OrderSolution.Domain.Repositories;
using OrderSolution.Infrastructure.Data;
using OrderSolution.Infrastructure.Integrations;
using OrderSolution.Infrastructure.Repositories;
using OrdersSolution.RetryConsumer;

var builder = Host.CreateApplicationBuilder(args);


// Configuração do banco de dados (PostgreSQL)
builder.Services.AddDbContext<OrderContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

builder.Services.AddHttpClient<IDistributionCenterAPI, DistributionCenterAPI>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7078");
});

// Registra os repositórios necessários para o reprocessamento
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();

// Registra o serviço do consumidor como um BackgroundService
builder.Services.AddHostedService<OrderRetryConsumerService>();

// Configuração de logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();
app.Run();
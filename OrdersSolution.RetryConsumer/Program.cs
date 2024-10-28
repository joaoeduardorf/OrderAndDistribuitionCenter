using Microsoft.EntityFrameworkCore;
using OrderSolution.Domain.Repositories;
using OrderSolution.Infrastructure.Data;
using OrderSolution.Infrastructure.Integrations;
using OrderSolution.Infrastructure.Repositories;
using OrdersSolution.RetryConsumer;

var builder = Host.CreateApplicationBuilder(args);


// Configura��o do banco de dados (PostgreSQL)
builder.Services.AddDbContext<OrderContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

builder.Services.AddHttpClient<IDistributionCenterAPI, DistributionCenterAPI>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7078");
});

// Registra os reposit�rios necess�rios para o reprocessamento
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();

// Registra o servi�o do consumidor como um BackgroundService
builder.Services.AddHostedService<OrderRetryConsumerService>();

// Configura��o de logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();
app.Run();
using Microsoft.EntityFrameworkCore;
using OrderSolution.Application.Interfaces;
using OrderSolution.Application;
using OrderSolution.Domain.Notifications;
using OrderSolution.Domain.Repositories;
using OrderSolution.Domain.Validation.Interfaces;
using OrderSolution.Domain.Validation;
using OrderSolution.Infrastructure.Data;
using OrderSolution.Infrastructure.Integrations;
using OrderSolution.Infrastructure.Repositories;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddOpenTelemetry().
    WithTracing(tracerProviderBuilder =>
{
    tracerProviderBuilder
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("OrderSolution.API"))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddJaegerExporter(options =>
        {
            options.AgentHost = "localhost"; // Altere para o host do Jaeger, se necessário
            options.AgentPort = 6831;        // Porta padrão do Jaeger
        });
}); ;
// Configure OpenTelemetry and Jaeger
//builder.Services.AddOpenTelemetryTracing(tracerProviderBuilder =>
//{
//    tracerProviderBuilder
//        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("OrderService"))
//        .AddAspNetCoreInstrumentation()
//        .AddHttpClientInstrumentation()
//        .AddJaegerExporter(options =>
//        {
//            options.AgentHost = builder.Configuration["Jaeger:Host"];
//            options.AgentPort = builder.Configuration.GetValue<int>("Jaeger:Port");
//        });
//});

builder.Services.AddDbContext<OrderContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IKafkaProducerService, KafkaProducerService>();
builder.Services.AddScoped<IOrdersService, OrdersService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>(); // Novo repositório de OrderItem
builder.Services.AddScoped<IDistributionCenterAPI, DistributionCenterAPI>();
builder.Services.AddScoped<IOrderValidationService, OrderValidationService>();
builder.Services.AddSingleton<DomainNotificationHandler>();

builder.Services.AddHttpClient<IDistributionCenterAPI, DistributionCenterAPI>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7078");
})
.AddPolicyHandler(GetRetryPolicy())          // Retry policy
.AddPolicyHandler(GetTimeoutPolicy())         // Timeout policy
.AddPolicyHandler(GetCircuitBreakerPolicy()); // Circuit breaker policy

var app = builder.Build();

// Middleware for Correlation ID
app.Use(async (context, next) =>
{
    if (!context.Request.Headers.ContainsKey("X-Correlation-ID"))
        context.Request.Headers["X-Correlation-ID"] = Guid.NewGuid().ToString();

    await next();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();



static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError() // Lida com erros 5xx e 408 (timeout)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))); // Tentativas com tempo exponencial
}

static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
{
    return Policy.TimeoutAsync<HttpResponseMessage>(5); // Define um timeout de 5 segundos
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)); // Circuito abre após 5 falhas consecutivas e fecha após 30 segundos
}
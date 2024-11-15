using DistribuitionCenter.API.Middlewares;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .WithMetrics( metrics => {
        metrics.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("DistribuitionCenter.API"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            ;
    })
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("DistribuitionCenter.API"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddJaegerExporter(options =>
            {
                options.AgentHost = "jaeger"; // Altere para o host do Jaeger, se necess�rio
                options.AgentPort = 6831;        // Porta padr�o do Jaeger
            });
    });

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();
app.MapHealthChecks("/health");

app.UseAuthorization();

app.MapControllers();

app.Run();

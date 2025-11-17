using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using StackExchange.Redis;
using System.Text;
using Venice.Orders.Application.Interfaces;
using Venice.Orders.Application.Services;
using Venice.Orders.Infrastructure.Cache;
using Venice.Orders.Infrastructure.Messaging;
using Venice.Orders.Infrastructure.Mongo;
using Venice.Orders.Infrastructure.Persistence;
using Venice.Orders.Infrastructure.Repositories;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

string sqlConn = Environment.GetEnvironmentVariable("SQLSERVER_CONNECTIONSTRING")
                    ?? throw new Exception("SQLSERVER_CONNECTIONSTRING not found.");

string mongoConn = Environment.GetEnvironmentVariable("MONGO_CONNECTIONSTRING")
                    ?? throw new Exception("MONGO_CONNECTIONSTRING not found.");

string mongoDbName = Environment.GetEnvironmentVariable("MONGO_DATABASE")
                    ?? "venice";

string redisConf = Environment.GetEnvironmentVariable("REDIS_CONFIGURATION")
                    ?? throw new Exception("REDIS_CONFIGURATION not found.");

string rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST")
                    ?? throw new Exception("RABBITMQ_HOST not found.");

string jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
                    ?? throw new Exception("JWT_SECRET not found.");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Venice Orders",
        Version = "v1",
        Description = "API de pedidos da Venice com autenticação JWT, cache, mensageria e persistência híbrida."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira o token JWT neste formato: **Bearer {seu_token}**",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<OrdersDbContext>(opts =>
    opts.UseSqlServer(sqlConn));

builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConn));
builder.Services.AddSingleton(provider =>
{
    var client = provider.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoDbName);
});

builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConf));
builder.Services.AddSingleton<RedisCacheService>();

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderItemRepository, OrderItemRepository>();
builder.Services.AddScoped<IEventPublisher>(provider =>
{
    var config = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "RabbitMQ:HostName", rabbitHost }
        })
        .Build();

    return new RabbitMqPublisher(config);
});

builder.Services.AddScoped<OrderService>();

var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

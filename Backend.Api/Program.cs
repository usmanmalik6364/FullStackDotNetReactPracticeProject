using Backend.Api.Caching;
using Backend.Api.Data;
using Backend.Api.Middleware;
using Backend.Api.Services;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration =
        builder.Configuration.GetConnectionString("Redis");

    options.InstanceName = "BackendApi:";
});
builder.Services.AddScoped<IContractorService, ContractorService>();
builder.Services.AddScoped<ICacheService, RedisCacheService>();
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration =
        builder.Configuration.GetConnectionString("Redis");

    return ConnectionMultiplexer.Connect(configuration!);
});
builder.Services.AddSingleton<
    IDistributedLockService,
    RedisDistributedLockService>();
//we want all the requests inside this API Process to share the same lock dictionary for cache lock provider
builder.Services.AddSingleton<CacheLockProvider>();
var app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();

app.Run();
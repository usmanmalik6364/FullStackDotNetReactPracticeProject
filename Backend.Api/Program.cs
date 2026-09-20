using Backend.Api.Data;
using Backend.Api.Middleware;
using Backend.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<IContractorService, ContractorService>();

var app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapControllers();

app.Run();
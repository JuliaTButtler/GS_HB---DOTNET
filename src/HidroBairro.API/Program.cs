using HidroBairro.Application;
using HidroBairro.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    service = "HidroBairro.API",
    boundedContext = "Zeladoria",
    status = "ok"
}));

app.Run();

public partial class Program;

using HidroBairro.API.Endpoints;
using HidroBairro.API.Extensions;
using HidroBairro.Application;
using HidroBairro.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHidroBairroSwagger();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseHidroBairroSwagger();
}

app.MapGet("/", () => Results.Ok(new
{
    service = "HidroBairro.API",
    boundedContext = "Zeladoria",
    status = "ok",
    swagger = "/swagger"
}));

app.MapCategoriaEndpoints();
app.MapSolicitacaoEndpoints();

app.Run();

public partial class Program;

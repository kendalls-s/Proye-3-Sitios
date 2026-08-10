using Core2.OferentesAptos;
using Core2.OferentesAptos.Repository;
using Core2.OferentesAptos.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("ClientDev", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<OferenteRepository>();
builder.Services.AddScoped<IBitacoraRepository, BitacoraRepository>();
builder.Services.AddScoped<IOferentesAptosService, OferentesAptosService>();

var app = builder.Build();

app.UseCors("ClientDev");
app.MapOferentesAptosEndpoints();

app.Run();

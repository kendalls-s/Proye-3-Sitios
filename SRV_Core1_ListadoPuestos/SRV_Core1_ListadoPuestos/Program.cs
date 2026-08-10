using Core1.ListadoPuestos;
using Core1.ListadoPuestos.Repository;
using Core1.ListadoPuestos.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("ClientDev", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<PuestoRepository>();
builder.Services.AddScoped<IBitacoraRepository, BitacoraRepository>();
builder.Services.AddScoped<IListadoPuestosService, ListadoPuestosService>();

var app = builder.Build();

app.UseCors("ClientDev");
app.MapListadoPuestosEndpoints();

app.Run();

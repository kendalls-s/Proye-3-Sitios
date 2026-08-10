using Core3.CreacionEmpleados;
using Core3.CreacionEmpleados.Repository;
using Core3.CreacionEmpleados.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("ClientDev", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<EmpleadoRepository>();
builder.Services.AddScoped<IBitacoraRepository, BitacoraRepository>();
builder.Services.AddScoped<ICreacionEmpleadosService, CreacionEmpleadosService>();

var app = builder.Build();

app.UseCors("ClientDev");
app.MapCreacionEmpleadosEndpoints();

app.Run();

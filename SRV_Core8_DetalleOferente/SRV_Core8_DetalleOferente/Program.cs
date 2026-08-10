using Core8.DetalleOferente;
using Core8.DetalleOferente.Repository;
using Core8.DetalleOferente.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("ClientDev", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<OferenteRepository>();
builder.Services.AddScoped<IBitacoraRepository, BitacoraRepository>();
builder.Services.AddScoped<IDetalleOferenteService, DetalleOferenteService>();

var app = builder.Build();

app.UseCors("ClientDev");
app.MapDetalleOferenteEndpoints();

app.Run();

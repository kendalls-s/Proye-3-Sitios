using Core7.ListadoOferentes;
using Core7.ListadoOferentes.Repository;
using Core7.ListadoOferentes.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("ClientDev", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddScoped<OferenteRepository>();
builder.Services.AddScoped<IBitacoraRepository, BitacoraRepository>();
builder.Services.AddScoped<IListadoOferentesService, ListadoOferentesService>();

var app = builder.Build();

app.UseCors("ClientDev");
app.MapListadoOferentesEndpoints();

app.Run();

using ApiElTiempo.Extensions;
using Finbuckle.MultiTenant;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors();
builder.Services.AddControllers();

builder.Services.AddMultiTenant<TenantInfo>()
            .WithHeaderStrategy("Client")
            .WithInMemoryStore(options =>
            {
                options.Tenants.Add(new TenantInfo { Id = "1", Identifier = "ElTiempo1", Name = "ElTiempo1", ConnectionString = "Server=DESARROLLODL;Database=DATASYSTEM;User Id=sa;Password=25741842; Encrypt=False;" });
                options.Tenants.Add(new TenantInfo { Id = "2", Identifier = "ElTiempo2", Name = "ElTiempo2", ConnectionString = "Server=DESARROLLODL;Database=DATASYSTEM2;User Id=sa;Password=25741842; Encrypt=False;" });
                options.Tenants.Add(new TenantInfo { Id = "3", Identifier = "ElTiempo3", Name = "ElTiempo3", ConnectionString = "Server=DESARROLLODL;Database=DATASYSTEM3;User Id=sa;Password=25741842; Encrypt=False;" });
            });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API ElTiempo",
        Version = "v1",
        Description = "# API ElTiempo\r\n\r\n" +
    "Integración API REST basada en JSON para el consumo de servicios\r\n\r\n" +
    "## PETICIÓN\r\n\r\n" +
    "Se deberán realizar a la misma url base de la instancia.\r\n\r\n" +
    "Las peticiones pueden ser tipo POST, GET, PUT o DELETE según definición del endpoint.\r\n\r\n" +
    "## RESPUESTA\r\n\r\n" +
    "Si es recibida correctamente, cada petición devolverá un objeto con la estructura, ok, message, data:\r\n " +
    "* ok: representa la bandera de validación, exitosa o no, de la respuesta.\r\n" +
    "* message: contendrá el mensaje de error o exito de la solicitud.\r\n" +
    "* data: contendra, si aplica, información de la respuesta. Su estructura se indicará de forma individual en cada endpoint\r\n" +
    "\t``` {\r\n  \"ok\": bool,\r\n  \"message\": string,\r\n  \"data\": object\r\n ```",
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.configureTrasient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api ElTiempo v1");
    c.RoutePrefix = string.Empty;
});

app.UseMultiTenant();
app.UseCors(options =>
{
    options.WithOrigins("*");
    options.AllowAnyMethod();
    options.AllowAnyHeader();
});

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

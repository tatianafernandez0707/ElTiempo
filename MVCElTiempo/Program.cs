using Microsoft.EntityFrameworkCore;
using MVCElTiempo.Extensions;
using MVCElTiempo.Infraestructure;
using MVCElTiempo.Models.ContextEntityFramework;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.UseConnectionPerTenant(builder.Configuration);

builder.Services.ConfigureTransient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseMiddleware<TenantInfoMiddleware>();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Guardar}/{id?}");

app.Run();

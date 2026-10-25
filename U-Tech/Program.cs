using U_Tech.Filters; // 👈 1. Importa el namespace del filtro

var builder = WebApplication.CreateBuilder(args);

// Configuración de servicios
builder.Services.AddControllersWithViews(options =>
{
    // 👈 2. Registra el filtro NoCacheFilter para que se aplique a todos los controladores
    options.Filters.Add(typeof(NoCacheFilter));
});
builder.Services.AddSession();
builder.Services.AddSingleton<U_Tech.Data.Conexion>();

var app = builder.Build();

// Configuración del middleware (orden correcto)
app.UseStaticFiles();
app.UseSession();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
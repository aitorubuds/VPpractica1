using Datos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// Instancia única en memoria de la capa de datos, cargada con los datos de prueba
builder.Services.AddSingleton<IDatos>(DatosPrueba.Crear());

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(opciones =>
{
    opciones.IdleTimeout = TimeSpan.FromMinutes(30);
    opciones.Cookie.HttpOnly = true;
    opciones.Cookie.IsEssential = true;
});

var app = builder.Build();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.MapRazorPages();

app.Run();

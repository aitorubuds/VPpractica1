using Datos;
using Nucleo.Seguridad;
using www.Utilidades;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// Clave para cifrar datos sensibles: de la configuración o, si no hay, una nueva en cada arranque
// (vale porque la base de datos también se crea de nuevo en cada arranque)
string claveCifrado = builder.Configuration["Cifrado:Clave"] ?? ServicioCifrado.GenerarClave();
builder.Services.AddSingleton(new ConfiguracionCifrado(claveCifrado));

// Instancia única en memoria de la capa de datos, cargada con los datos de prueba
builder.Services.AddSingleton<IDatos>(DatosPrueba.Crear(claveCifrado));

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

// Para que las pruebas de integración puedan arrancar la web (WebApplicationFactory<Program>)
public partial class Program { }

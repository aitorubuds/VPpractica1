using Datos;
using Nucleo.Modelos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// Instancia única en memoria de la capa de datos
var capaDatos = new Datos.Datos();

// Usuario de prueba para iniciar sesión
var usuarioInicial = new Usuario
{
    Nombre = "Alex",
    Apellidos = "Alumno",
    EMail = "admin@ubu.es",
    Cuenta = "admin",
    Activo = true
};
usuarioInicial.EstablecerContrasena("Password123!");
capaDatos.GuardarUsuario(usuarioInicial);

// Proyecto inicial de prueba
capaDatos.GuardarProyecto(new Proyecto
{
    Nombre = "Plataforma UBU TPMP",
    Siglas = "UBU-TPMP",
    Descripcion = "Proyecto de gestión de pruebas y validación.",
    Activo = true
});

builder.Services.AddSingleton<IDatos>(capaDatos);

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

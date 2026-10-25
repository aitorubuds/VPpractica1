using Datos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System.Net;
using System.Net.Sockets;

namespace Web.UITests;

// Arranca la web en un puerto libre (con sus datos de prueba recién creados)
// y abre el navegador indicado. Cada prueba tiene la suya, así no se pisan.
public sealed class SesionNavegador : IDisposable
{
    private readonly WebApplicationFactory<Program> _fabrica;

    public IWebDriver Driver { get; }
    public WebDriverWait Espera { get; }
    public string UrlBase { get; }

    public IDatos Datos => _fabrica.Services.GetRequiredService<IDatos>();

    public SesionNavegador(string navegador)
    {
        int puerto = PuertoLibre();
        UrlBase = $"http://localhost:{puerto}";

        _fabrica = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(b => b.ConfigureLogging(l => l.ClearProviders()));
        _fabrica.UseKestrel(o => o.ListenLocalhost(puerto));
        _fabrica.StartServer();

        Driver = CrearDriver(navegador);
        Driver.Manage().Window.Size = new System.Drawing.Size(1280, 900);
        Espera = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));

        // Mientras la página se recarga los elementos se quedan "viejos": se reintenta
        Espera.IgnoreExceptionTypes(typeof(StaleElementReferenceException), typeof(NoSuchElementException));
    }

    // Con la variable de entorno VER_NAVEGADOR=1 se ve el navegador; si no, va oculto (headless)
    private static IWebDriver CrearDriver(string navegador)
    {
        bool oculto = Environment.GetEnvironmentVariable("VER_NAVEGADOR") != "1";

        switch (navegador)
        {
            case "chrome":
                ChromeOptions chrome = new ChromeOptions();
                if (oculto) chrome.AddArgument("--headless=new");
                return new ChromeDriver(chrome);

            case "edge":
                EdgeOptions edge = new EdgeOptions();
                if (oculto) edge.AddArgument("--headless=new");
                return new EdgeDriver(edge);

            case "firefox":
                FirefoxOptions firefox = new FirefoxOptions();
                if (oculto) firefox.AddArgument("-headless");
                return new FirefoxDriver(firefox);

            default:
                throw new ArgumentException($"Navegador no soportado: {navegador}");
        }
    }

    private static int PuertoLibre()
    {
        TcpListener l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        int puerto = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        return puerto;
    }

    // ---- Acciones que se repiten en las pruebas ----

    public void Ir(string ruta) => Driver.Navigate().GoToUrl(UrlBase + ruta);

    public IWebElement Elemento(string id) => Espera.Until(d => d.FindElement(By.Id(id)));

    public bool Existe(string id) => Driver.FindElements(By.Id(id)).Count > 0;

    public void Escribir(string id, string texto)
    {
        IWebElement campo = Elemento(id);
        campo.Clear();
        campo.SendKeys(texto);
    }

    // Espera a que la página actual sea la indicada (sin mirar mayúsculas ni parámetros)
    public void EsperarPagina(string ruta) =>
        Espera.Until(d => new Uri(d.Url).AbsolutePath.Equals(ruta, StringComparison.OrdinalIgnoreCase));

    public string RutaActual => new Uri(Driver.Url).AbsolutePath;

    public string TextoPagina => Driver.FindElement(By.TagName("body")).Text;

    // Login en los dos pasos, como lo haría el usuario
    public void Login(string email, string clave)
    {
        Ir("/Login");
        Escribir("txtUsuario", email);
        Elemento("btnSiguiente").Click();
        EsperarPagina("/Login/Index2");

        Escribir("txtContraseña", clave);
        Elemento("btnAceptar").Click();
    }

    // Abre el menú del usuario (desplegable de Bootstrap) y pulsa una opción
    public void Menu(string idOpcion)
    {
        Elemento("btnMenuUsuario").Click();
        IWebElement opcion = Elemento(idOpcion);
        Espera.Until(_ => opcion.Displayed);
        opcion.Click();
    }

    public void Dispose()
    {
        Driver.Quit();
        _fabrica.Dispose();
    }
}

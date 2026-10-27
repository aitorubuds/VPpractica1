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

// Arranca la web en un puerto libre con sus datos de prueba nuevos y abre el navegador
// que le digamos. Cada prueba tiene la suya, así no se molestan entre ellas
public class SesionNavegador : IDisposable
{
    // Ponlo a true para ver el navegador mientras se hacen las pruebas
    private static readonly bool VerNavegador = false;

    private readonly WebApplicationFactory<Program> _fabrica;

    public IWebDriver Driver { get; }
    public WebDriverWait Espera { get; }
    public string UrlBase { get; }

    // La base de datos de esta web
    public IDatos Datos
    {
        get { return _fabrica.Services.GetRequiredService<IDatos>(); }
    }

    public SesionNavegador(string navegador)
    {
        // 1. Arrancamos la web en un puerto libre
        int puerto = BuscarPuertoLibre();
        UrlBase = "http://localhost:" + puerto;

        _fabrica = new WebApplicationFactory<Program>().WithWebHostBuilder(ConfigurarWeb);
        _fabrica.UseKestrel(opciones => opciones.ListenLocalhost(puerto));
        _fabrica.StartServer();

        // 2. Abrimos el navegador
        Driver = CrearDriver(navegador);
        Driver.Manage().Window.Size = new System.Drawing.Size(1280, 900);

        // 3. Esperamos hasta 10 segundos a que aparezcan las cosas en la página
        Espera = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));

        // Si la página se está recargando y no encuentra el elemento, lo vuelve a intentar
        Espera.IgnoreExceptionTypes(typeof(StaleElementReferenceException), typeof(NoSuchElementException));
    }

    // Quitamos los logs de la web para que la salida de las pruebas se lea mejor
    private static void ConfigurarWeb(IWebHostBuilder web)
    {
        web.ConfigureLogging(QuitarLogs);
    }

    private static void QuitarLogs(ILoggingBuilder logs)
    {
        logs.ClearProviders();
    }

    // Crear el navegador, si no lo queremos ver va oculto
    private static IWebDriver CrearDriver(string navegador)
    {
        if (navegador == "chrome")
        {
            ChromeOptions opciones = new ChromeOptions();
            if (!VerNavegador)
            {
                opciones.AddArgument("--headless=new");
            }
            return new ChromeDriver(opciones);
        }

        if (navegador == "edge")
        {
            EdgeOptions opciones = new EdgeOptions();
            if (!VerNavegador)
            {
                opciones.AddArgument("--headless=new");
            }
            return new EdgeDriver(opciones);
        }

        if (navegador == "firefox")
        {
            FirefoxOptions opciones = new FirefoxOptions();
            if (!VerNavegador)
            {
                opciones.AddArgument("-headless");
            }
            return new FirefoxDriver(opciones);
        }

        throw new ArgumentException("Navegador no soportado: " + navegador);
    }

    // Pedimos al sistema un puerto que no esté usando nadie y lo soltamos para la web
    private static int BuscarPuertoLibre()
    {
        TcpListener escucha = new TcpListener(IPAddress.Loopback, 0);
        escucha.Start();
        int puerto = ((IPEndPoint)escucha.LocalEndpoint).Port;
        escucha.Stop();
        return puerto;
    }

    // Acciones que se repiten en las pruebas

    // Ir a una página de la web
    public void Ir(string ruta)
    {
        Driver.Navigate().GoToUrl(UrlBase + ruta);
    }

    // Buscar un elemento por su id esperando a que aparezca
    public IWebElement Elemento(string id)
    {
        return Espera.Until(driver => driver.FindElement(By.Id(id)));
    }

    // Mirar si un elemento está en la página, sin esperar
    public bool Existe(string id)
    {
        return Driver.FindElements(By.Id(id)).Count > 0;
    }

    // Borrar lo que haya en un campo y escribir el texto
    public void Escribir(string id, string texto)
    {
        IWebElement campo = Elemento(id);
        campo.Clear();
        campo.SendKeys(texto);
    }

    // Esperar a que estemos en esa página, sin mirar mayúsculas
    public void EsperarPagina(string ruta)
    {
        Espera.Until(driver => string.Equals(RutaActual, ruta, StringComparison.OrdinalIgnoreCase));
    }

    // Esperar a que salga un texto en la página
    public void EsperarTexto(string texto)
    {
        Espera.Until(driver => TextoPagina.Contains(texto));
    }

    // La ruta de la página en la que estamos, sin el principio ni los parámetros
    public string RutaActual
    {
        get { return new Uri(Driver.Url).AbsolutePath; }
    }

    // Todo el texto que se ve en la página
    public string TextoPagina
    {
        get { return Driver.FindElement(By.TagName("body")).Text; }
    }

    // Login en los dos pasos, como lo haría el usuario
    public void Login(string email, string clave)
    {
        // 1. Ponemos el email
        Ir("/Login");
        Escribir("txtUsuario", email);
        Elemento("btnSiguiente").Click();
        EsperarPagina("/Login/Index2");

        // 2. Ponemos la contraseña
        Escribir("txtContraseña", clave);
        Elemento("btnAceptar").Click();
    }

    // Abrir el menú del usuario y pulsar una opción
    public void Menu(string idOpcion)
    {
        Elemento("btnMenuUsuario").Click();
        IWebElement opcion = Elemento(idOpcion);

        // Esperamos a que el menú se haya desplegado
        Espera.Until(driver => opcion.Displayed);
        opcion.Click();
    }

    public void Dispose()
    {
        Driver.Quit();
        _fabrica.Dispose();
    }
}

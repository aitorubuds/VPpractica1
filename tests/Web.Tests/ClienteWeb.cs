using Datos;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Web.Tests;

// Arranca la web en memoria sin navegador y hace las peticiones igual que el navegador,
// guardando las cookies de la sesión y mandando el token de cada formulario
public class ClienteWeb : IDisposable
{
    // Clave fija para poder comprobar desde la prueba el dato cifrado
    public const string ClaveCifrado = "AAECAwQFBgcICQoLDA0ODxAREhMUFRYXGBkaGxwdHh8=";

    private readonly WebApplicationFactory<Program> _fabrica;

    public HttpClient Http { get; }

    // La base de datos de esta web, cada ClienteWeb tiene la suya
    public IDatos Datos
    {
        get { return _fabrica.Services.GetRequiredService<IDatos>(); }
    }

    public ClienteWeb()
    {
        _fabrica = new WebApplicationFactory<Program>().WithWebHostBuilder(ConfigurarWeb);

        // No seguimos las redirecciones para poder comprobar a dónde manda cada página
        WebApplicationFactoryClientOptions opciones = new WebApplicationFactoryClientOptions();
        opciones.AllowAutoRedirect = false;
        Http = _fabrica.CreateClient(opciones);
    }

    // Le ponemos la clave de cifrado fija y quitamos los logs de la web
    private static void ConfigurarWeb(IWebHostBuilder web)
    {
        web.UseSetting("Cifrado:Clave", ClaveCifrado);
        web.ConfigureLogging(QuitarLogs);
    }

    // Sin logs la salida de las pruebas se lee mejor
    private static void QuitarLogs(ILoggingBuilder logs)
    {
        logs.ClearProviders();
    }

    // Pedir una página
    public async Task<HttpResponseMessage> Get(string url)
    {
        return await Http.GetAsync(url);
    }

    // Pedir una página y devolver su HTML con los acentos bien puestos
    public async Task<string> GetHtml(string url)
    {
        HttpResponseMessage respuesta = await Http.GetAsync(url);
        return await Html(respuesta);
    }

    // Enviar un formulario, primero cargamos la página para sacar el token
    public async Task<HttpResponseMessage> EnviarFormulario(string pagina, string? handler, Dictionary<string, string> campos)
    {
        HttpResponseMessage paginaCargada = await Http.GetAsync(pagina);
        string html = await paginaCargada.Content.ReadAsStringAsync();
        campos["__RequestVerificationToken"] = SacarToken(html);

        string url = pagina;
        if (handler != null)
        {
            url = pagina + "?handler=" + handler;
        }

        return await Http.PostAsync(url, new FormUrlEncodedContent(campos));
    }

    // Login en los dos pasos de la web, primero el email y luego la contraseña
    public async Task<HttpResponseMessage> Login(string email, string clave)
    {
        Dictionary<string, string> paso1 = new Dictionary<string, string>();
        paso1.Add("NombreUsuario", email);
        await EnviarFormulario("/Login", null, paso1);

        Dictionary<string, string> paso2 = new Dictionary<string, string>();
        paso2.Add("Contraseña", clave);
        return await EnviarFormulario("/Login/Index2", "Aceptar", paso2);
    }

    // A dónde nos manda la página, vacío si no nos manda a ninguna
    public static string Destino(HttpResponseMessage respuesta)
    {
        if (respuesta.Headers.Location == null)
        {
            return string.Empty;
        }

        return respuesta.Headers.Location.OriginalString;
    }

    // El HTML de una respuesta con los acentos ya pasados a letra
    public static async Task<string> Html(HttpResponseMessage respuesta)
    {
        string html = await respuesta.Content.ReadAsStringAsync();
        return WebUtility.HtmlDecode(html);
    }

    // Buscamos el campo oculto del token en el HTML y sacamos su valor
    private static string SacarToken(string html)
    {
        string inicio = "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"";
        int posicion = html.IndexOf(inicio);

        if (posicion < 0)
        {
            return string.Empty;
        }

        posicion = posicion + inicio.Length;
        int fin = html.IndexOf("\"", posicion);
        return html.Substring(posicion, fin - posicion);
    }

    public void Dispose()
    {
        Http.Dispose();
        _fabrica.Dispose();
    }
}

using Datos;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.RegularExpressions;

namespace Web.Tests;

// Arranca la web en memoria (sin navegador) y hace las peticiones como lo haría el navegador:
// guarda las cookies de sesión y envía el token antiforgery en cada formulario.
public sealed partial class ClienteWeb : IDisposable
{
    // Clave fija para poder comprobar desde la prueba el dato cifrado
    public const string ClaveCifrado = "AAECAwQFBgcICQoLDA0ODxAREhMUFRYXGBkaGxwdHh8=";

    private readonly WebApplicationFactory<Program> _fabrica;

    public HttpClient Http { get; }

    // La base de datos de esta instancia de la web (cada ClienteWeb tiene la suya)
    public IDatos Datos => _fabrica.Services.GetRequiredService<IDatos>();

    public ClienteWeb()
    {
        _fabrica = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(b => b
                .UseSetting("Cifrado:Clave", ClaveCifrado)
                // Sin los logs de ASP.NET para que la salida de las pruebas se lea bien
                .ConfigureLogging(l => l.ClearProviders()));

        // Sin seguir redirecciones para poder comprobar a dónde manda cada página
        Http = _fabrica.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    public async Task<HttpResponseMessage> Get(string url) => await Http.GetAsync(url);

    // Devuelve el HTML ya decodificado (los acentos vienen como &#xF3;)
    public async Task<string> GetHtml(string url)
    {
        HttpResponseMessage respuesta = await Http.GetAsync(url);
        return WebUtility.HtmlDecode(await respuesta.Content.ReadAsStringAsync());
    }

    // Envía un formulario de la página: primero la carga para sacar el token antiforgery
    public async Task<HttpResponseMessage> EnviarFormulario(
        string pagina,
        string? handler,
        Dictionary<string, string> campos)
    {
        string html = await (await Http.GetAsync(pagina)).Content.ReadAsStringAsync();
        campos["__RequestVerificationToken"] = SacarToken(html);

        string url = handler is null ? pagina : $"{pagina}?handler={handler}";
        return await Http.PostAsync(url, new FormUrlEncodedContent(campos));
    }

    // Login en los dos pasos de la web (email y luego contraseña)
    public async Task<HttpResponseMessage> Login(string email, string clave)
    {
        await EnviarFormulario("/Login", null, new() { ["NombreUsuario"] = email });

        return await EnviarFormulario("/Login/Index2", "Aceptar", new() { ["Contraseña"] = clave });
    }

    public static string Destino(HttpResponseMessage respuesta) =>
        respuesta.Headers.Location?.OriginalString ?? string.Empty;

    public static async Task<string> Html(HttpResponseMessage respuesta) =>
        WebUtility.HtmlDecode(await respuesta.Content.ReadAsStringAsync());

    private static string SacarToken(string html)
    {
        Match m = RegexToken().Match(html);
        return m.Success ? m.Groups[1].Value : string.Empty;
    }

    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")]
    private static partial Regex RegexToken();

    public void Dispose()
    {
        Http.Dispose();
        _fabrica.Dispose();
    }
}

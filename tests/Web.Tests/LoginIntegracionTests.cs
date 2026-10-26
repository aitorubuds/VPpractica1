using Datos;
using Nucleo.Modelos;
using System.Net;

namespace Web.Tests;

// Integración Web + Nucleo + Datos: login, auditoría y seguridad básica, por HTTP
[TestClass]
public class LoginIntegracionTests
{
    private ClienteWeb _web = null!;

    [TestInitialize]
    public void Inicializar() => _web = new ClienteWeb();

    [TestCleanup]
    public void Limpiar() => _web.Dispose();

    private EntradaLog? UltimoLog(string accion) =>
        _web.Datos.Logs.LastOrDefault(l => l.Accion == accion);

    // IT-01: sin sesión, todas las páginas protegidas mandan al login
    [TestMethod]
    [DataRow("/")]
    [DataRow("/Principal")]
    [DataRow("/Proyectos/Seleccionar")]
    [DataRow("/Log")]
    [DataRow("/Usuarios/Perfil")]
    [DataRow("/Login/Index2")]
    public async Task IT01_SinSesion_RedirigeALogin(string pagina)
    {
        HttpResponseMessage respuesta = await _web.Get(pagina);

        Assert.AreEqual(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.StartsWith("/Login", ClienteWeb.Destino(respuesta));
    }

    // IT-02: login correcto -> página principal y queda auditado
    [TestMethod]
    [DataRow(DatosPrueba.EmailAdmin, DatosPrueba.ClaveAdmin)]
    [DataRow(DatosPrueba.EmailTester, DatosPrueba.ClaveTester)]
    [DataRow(DatosPrueba.EmailInvitado, DatosPrueba.ClaveInvitado)]
    [DataRow("ADMIN@UBU.ES", DatosPrueba.ClaveAdmin)]
    public async Task IT02_LoginCorrecto_EntraYAudita(string email, string clave)
    {
        HttpResponseMessage respuesta = await _web.Login(email, clave);

        Assert.AreEqual(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.AreEqual("/Principal", ClienteWeb.Destino(respuesta));
        Assert.AreEqual(HttpStatusCode.OK, (await _web.Get("/Principal")).StatusCode);

        EntradaLog? log = UltimoLog(Auditoria.LoginCorrecto);
        Assert.IsNotNull(log);
        Assert.AreEqual(email, log.NombreUsuario, ignoreCase: true);
        Assert.AreEqual(Auditoria.NivelInfo, log.Nivel);
    }

    // IT-03: contraseña mala, email que no existe o usuario de baja -> mismo mensaje y auditado
    [TestMethod]
    [DataRow(DatosPrueba.EmailAdmin, "Mala1234!", Auditoria.LoginFallido, DisplayName = "IT-03 contraseña incorrecta")]
    [DataRow("nadie@ubu.es", "Algo1234!", Auditoria.LoginFallido, DisplayName = "IT-03 email inexistente")]
    [DataRow(DatosPrueba.EmailBaja, DatosPrueba.ClaveBaja, Auditoria.CuentaInactiva, DisplayName = "IT-03 usuario de baja")]
    public async Task IT03_LoginIncorrecto_NoEntraYAudita(string email, string clave, string accionEsperada)
    {
        HttpResponseMessage respuesta = await _web.Login(email, clave);
        string html = await ClienteWeb.Html(respuesta);

        Assert.AreEqual(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Contains("Usuario o contraseña incorrectos.", html);
        Assert.AreEqual(HttpStatusCode.Redirect, (await _web.Get("/Principal")).StatusCode);

        EntradaLog? log = UltimoLog(accionEsperada);
        Assert.IsNotNull(log);
        Assert.AreEqual(email, log.NombreUsuario);
        Assert.AreEqual(Auditoria.NivelAviso, log.Nivel);
    }

    // IT-04: tres fallos bloquean la cuenta; luego ni con la buena entra
    [TestMethod]
    public async Task IT04_TresFallos_BloqueaCuenta()
    {
        int fallidosAntes = _web.Datos.Logs.Count(l => l.Accion == Auditoria.LoginFallido);

        for (int i = 0; i < 3; i++)
        {
            await _web.Login(DatosPrueba.EmailTester, "Mala1234!");
        }

        HttpResponseMessage respuesta = await _web.Login(DatosPrueba.EmailTester, DatosPrueba.ClaveTester);

        Assert.AreEqual(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.IsTrue(_web.Datos.LeerUsuario(DatosPrueba.EmailTester)!.Bloqueado);
        Assert.AreEqual(fallidosAntes + 3, _web.Datos.Logs.Count(l => l.Accion == Auditoria.LoginFallido));
        Assert.IsTrue(_web.Datos.Logs.Any(l => l.Accion == Auditoria.CuentaBloqueada && l.Nivel == Auditoria.NivelError));
        Assert.IsTrue(_web.Datos.Logs.Any(l => l.Accion == Auditoria.CuentaBloqueada && l.Nivel == Auditoria.NivelAviso));
    }

    // IT-05: validación del formulario de email (vacío o mal escrito no pasa al paso 2)
    [TestMethod]
    [DataRow("", "Debe indicar un correo electrónico.")]
    [DataRow("hola", "El formato del correo electrónico no es válido.")]
    public async Task IT05_EmailNoValido_MuestraError(string email, string mensaje)
    {
        HttpResponseMessage respuesta = await _web.EnviarFormulario("/Login", null, new() { ["NombreUsuario"] = email });

        Assert.AreEqual(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Contains(mensaje, await ClienteWeb.Html(respuesta));
    }

    // IT-06: cerrar sesión la borra y queda auditado
    [TestMethod]
    public async Task IT06_CerrarSesion_BorraSesionYAudita()
    {
        await _web.Login(DatosPrueba.EmailAdmin, DatosPrueba.ClaveAdmin);

        HttpResponseMessage respuesta = await _web.EnviarFormulario("/Principal", "CerrarSesion", new());

        Assert.AreEqual("/Login", ClienteWeb.Destino(respuesta));
        Assert.AreEqual(HttpStatusCode.Redirect, (await _web.Get("/Principal")).StatusCode);
        Assert.IsNotNull(UltimoLog(Auditoria.CierreSesion));
    }

    // IT-07 (seguridad): un POST sin token se rechaza
    [TestMethod]
    [DataRow("/Login")]
    [DataRow("/Login/Index2?handler=Aceptar")]
    [DataRow("/Principal?handler=CerrarSesion")]
    public async Task IT07_PostSinToken_Rechazado(string url)
    {
        HttpResponseMessage respuesta = await _web.Http.PostAsync(url,
            new FormUrlEncodedContent(new Dictionary<string, string> { ["NombreUsuario"] = DatosPrueba.EmailAdmin }));

        Assert.AreEqual(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    // IT-08 (seguridad): la cookie de sesión es HttpOnly 
    [TestMethod]
    public async Task IT08_CookieSesion_HttpOnly()
    {
        // La sesión se crea al enviar el email (primer paso del login)
        HttpResponseMessage respuesta = await _web.EnviarFormulario("/Login", null,
            new() { ["NombreUsuario"] = DatosPrueba.EmailAdmin });
        string cookie = string.Join(";", respuesta.Headers.GetValues("Set-Cookie")).ToLowerInvariant();

        Assert.Contains(".aspnetcore.session", cookie);
        Assert.Contains("httponly", cookie);
    }

    // IT-09
    [TestMethod]
    [DataRow("/lib/bootstrap/dist/css/bootstrap.css")]
    [DataRow("/css/site.css")]
    [DataRow("/css/login.css")]
    public async Task IT09_RecursosEstaticos_Disponibles(string url)
    {
        Assert.AreEqual(HttpStatusCode.OK, (await _web.Get(url)).StatusCode);
    }
}

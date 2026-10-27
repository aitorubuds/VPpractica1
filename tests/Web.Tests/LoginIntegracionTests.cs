using Datos;
using Nucleo.Modelos;
using System.Net;

namespace Web.Tests;

// Pruebas de integración de la web con Nucleo y Datos para el login, el log y la seguridad
[TestClass]
public class LoginIntegracionTests
{
    private ClienteWeb _web = null!;

    // Arrancamos una web nueva antes de cada test
    [TestInitialize]
    public void Inicializar()
    {
        _web = new ClienteWeb();
    }

    // Y la cerramos al terminar
    [TestCleanup]
    public void Limpiar()
    {
        _web.Dispose();
    }

    // Función auxiliar para sacar la última entrada del log con esa acción, o null si no hay
    private EntradaLog? UltimoLog(string accion)
    {
        EntradaLog? ultimo = null;

        foreach (EntradaLog log in _web.Datos.Logs)
        {
            if (log.Accion == accion)
            {
                ultimo = log;
            }
        }

        return ultimo;
    }

    // Función auxiliar para contar las entradas del log con esa acción
    private int ContarLogs(string accion)
    {
        int total = 0;

        foreach (EntradaLog log in _web.Datos.Logs)
        {
            if (log.Accion == accion)
            {
                total++;
            }
        }

        return total;
    }

    // Función auxiliar para mirar si hay alguna entrada del log con esa acción y ese nivel
    private bool HayLog(string accion, string nivel)
    {
        foreach (EntradaLog log in _web.Datos.Logs)
        {
            if (log.Accion == accion && log.Nivel == nivel)
            {
                return true;
            }
        }

        return false;
    }

    // Sin sesión, pedimos la página y nos tiene que mandar al login
    private async Task ComprobarQueMandaAlLogin(string pagina)
    {
        HttpResponseMessage respuesta = await _web.Get(pagina);

        Assert.AreEqual(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.IsTrue(ClienteWeb.Destino(respuesta).StartsWith("/Login"));
    }

    // Sin sesión, la página de inicio manda al login
    [TestMethod]
    public async Task IT01_SinSesion_Inicio()
    {
        await ComprobarQueMandaAlLogin("/");
    }

    // Sin sesión, la página principal manda al login
    [TestMethod]
    public async Task IT01_SinSesion_Principal()
    {
        await ComprobarQueMandaAlLogin("/Principal");
    }

    // Sin sesión, la página de proyectos manda al login
    [TestMethod]
    public async Task IT01_SinSesion_Proyectos()
    {
        await ComprobarQueMandaAlLogin("/Proyectos/Seleccionar");
    }

    // Sin sesión, la página del log manda al login
    [TestMethod]
    public async Task IT01_SinSesion_Log()
    {
        await ComprobarQueMandaAlLogin("/Log");
    }

    // Sin sesión, la página del perfil manda al login
    [TestMethod]
    public async Task IT01_SinSesion_Perfil()
    {
        await ComprobarQueMandaAlLogin("/Usuarios/Perfil");
    }

    // Sin sesión, la página de la contraseña manda al login
    [TestMethod]
    public async Task IT01_SinSesion_PasoContrasena()
    {
        await ComprobarQueMandaAlLogin("/Login/Index2");
    }

    // Hacemos login bien y tiene que ir a la principal y quedar apuntado en el log
    private async Task ComprobarLoginCorrecto(string email, string clave)
    {
        // 1. Hacemos login
        HttpResponseMessage respuesta = await _web.Login(email, clave);

        // 2. Nos manda a la principal y ya podemos entrar en ella
        Assert.AreEqual(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.AreEqual("/Principal", ClienteWeb.Destino(respuesta));

        HttpResponseMessage principal = await _web.Get("/Principal");
        Assert.AreEqual(HttpStatusCode.OK, principal.StatusCode);

        // 3. En el log queda el login correcto con su email
        EntradaLog? log = UltimoLog(Auditoria.LoginCorrecto);
        Assert.IsNotNull(log);
        Assert.AreEqual(email.ToLower(), log.NombreUsuario.ToLower());
        Assert.AreEqual(Auditoria.NivelInfo, log.Nivel);
    }

    // El administrador entra con su clave
    [TestMethod]
    public async Task IT02_LoginCorrecto_Admin()
    {
        await ComprobarLoginCorrecto(DatosPrueba.EmailAdmin, DatosPrueba.ClaveAdmin);
    }

    // El tester entra con su clave
    [TestMethod]
    public async Task IT02_LoginCorrecto_Tester()
    {
        await ComprobarLoginCorrecto(DatosPrueba.EmailTester, DatosPrueba.ClaveTester);
    }

    // El invitado entra con su clave
    [TestMethod]
    public async Task IT02_LoginCorrecto_Invitado()
    {
        await ComprobarLoginCorrecto(DatosPrueba.EmailInvitado, DatosPrueba.ClaveInvitado);
    }

    // También entra si escribe el email en mayúsculas
    [TestMethod]
    public async Task IT02_LoginCorrecto_EmailEnMayusculas()
    {
        await ComprobarLoginCorrecto("ADMIN@UBU.ES", DatosPrueba.ClaveAdmin);
    }

    // Hacemos login mal, sale el mensaje de error, no entra y queda apuntado en el log
    private async Task ComprobarLoginIncorrecto(string email, string clave, string accionEsperada)
    {
        // 1. Hacemos login
        HttpResponseMessage respuesta = await _web.Login(email, clave);
        string html = await ClienteWeb.Html(respuesta);

        // 2. Se queda en la página con el mensaje de siempre
        Assert.AreEqual(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.IsTrue(html.Contains("Usuario o contraseña incorrectos."));

        // 3. No puede entrar en la principal
        HttpResponseMessage principal = await _web.Get("/Principal");
        Assert.AreEqual(HttpStatusCode.Redirect, principal.StatusCode);

        // 4. En el log queda el intento con el email que puso
        EntradaLog? log = UltimoLog(accionEsperada);
        Assert.IsNotNull(log);
        Assert.AreEqual(email, log.NombreUsuario);
        Assert.AreEqual(Auditoria.NivelAviso, log.Nivel);
    }

    // Con la contraseña mal no entra
    [TestMethod]
    public async Task IT03_LoginIncorrecto_ClaveMala()
    {
        await ComprobarLoginIncorrecto(DatosPrueba.EmailAdmin, "Mala1234!", Auditoria.LoginFallido);
    }

    // Con un email que no existe no entra, y el mensaje es el mismo
    [TestMethod]
    public async Task IT03_LoginIncorrecto_EmailQueNoExiste()
    {
        await ComprobarLoginIncorrecto("nadie@ubu.es", "Algo1234!", Auditoria.LoginFallido);
    }

    // Un usuario dado de baja no entra aunque ponga bien la clave
    [TestMethod]
    public async Task IT03_LoginIncorrecto_UsuarioDeBaja()
    {
        await ComprobarLoginIncorrecto(DatosPrueba.EmailBaja, DatosPrueba.ClaveBaja, Auditoria.CuentaInactiva);
    }

    // Tres fallos bloquean la cuenta y luego ni con la clave buena entra
    [TestMethod]
    public async Task IT04_TresFallos_BloqueaCuenta()
    {
        int fallidosAntes = ContarLogs(Auditoria.LoginFallido);

        // 1. Fallamos 3 veces
        await _web.Login(DatosPrueba.EmailTester, "Mala1234!");
        await _web.Login(DatosPrueba.EmailTester, "Mala1234!");
        await _web.Login(DatosPrueba.EmailTester, "Mala1234!");

        // 2. Probamos con la clave buena
        HttpResponseMessage respuesta = await _web.Login(DatosPrueba.EmailTester, DatosPrueba.ClaveTester);

        // 3. No entra y la cuenta está bloqueada
        Assert.AreEqual(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.IsTrue(_web.Datos.LeerUsuario(DatosPrueba.EmailTester)!.Bloqueado);

        // 4. En el log están los 3 fallos y el bloqueo
        Assert.AreEqual(fallidosAntes + 3, ContarLogs(Auditoria.LoginFallido));
        Assert.IsTrue(HayLog(Auditoria.CuentaBloqueada, Auditoria.NivelError));
        Assert.IsTrue(HayLog(Auditoria.CuentaBloqueada, Auditoria.NivelAviso));
    }

    // Mandamos el email en el primer paso y tiene que salir el mensaje de error
    private async Task ComprobarEmailNoValido(string email, string mensaje)
    {
        Dictionary<string, string> campos = new Dictionary<string, string>();
        campos.Add("NombreUsuario", email);

        HttpResponseMessage respuesta = await _web.EnviarFormulario("/Login", null, campos);
        string html = await ClienteWeb.Html(respuesta);

        Assert.AreEqual(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.IsTrue(html.Contains(mensaje));
    }

    // Con el email vacío no pasa al segundo paso
    [TestMethod]
    public async Task IT05_EmailVacio_MuestraError()
    {
        await ComprobarEmailNoValido("", "Debe indicar un correo electrónico.");
    }

    // Con un email mal escrito no pasa al segundo paso
    [TestMethod]
    public async Task IT05_EmailMalEscrito_MuestraError()
    {
        await ComprobarEmailNoValido("hola", "El formato del correo electrónico no es válido.");
    }

    // Cerrar sesión la borra y queda apuntado en el log
    [TestMethod]
    public async Task IT06_CerrarSesion_BorraSesionYAudita()
    {
        await _web.Login(DatosPrueba.EmailAdmin, DatosPrueba.ClaveAdmin);

        HttpResponseMessage respuesta = await _web.EnviarFormulario("/Principal", "CerrarSesion", new Dictionary<string, string>());

        // Nos manda al login y ya no podemos volver a la principal
        Assert.AreEqual("/Login", ClienteWeb.Destino(respuesta));
        HttpResponseMessage principal = await _web.Get("/Principal");
        Assert.AreEqual(HttpStatusCode.Redirect, principal.StatusCode);
        Assert.IsNotNull(UltimoLog(Auditoria.CierreSesion));
    }

    // Mandamos un formulario sin el token y la web lo tiene que rechazar
    private async Task ComprobarPostSinToken(string url)
    {
        Dictionary<string, string> campos = new Dictionary<string, string>();
        campos.Add("NombreUsuario", DatosPrueba.EmailAdmin);

        HttpResponseMessage respuesta = await _web.Http.PostAsync(url, new FormUrlEncodedContent(campos));

        Assert.AreEqual(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    // El primer paso del login sin token se rechaza
    [TestMethod]
    public async Task IT07_PostSinToken_Login()
    {
        await ComprobarPostSinToken("/Login");
    }

    // El segundo paso del login sin token se rechaza
    [TestMethod]
    public async Task IT07_PostSinToken_PasoContrasena()
    {
        await ComprobarPostSinToken("/Login/Index2?handler=Aceptar");
    }

    // Cerrar sesión sin token se rechaza
    [TestMethod]
    public async Task IT07_PostSinToken_CerrarSesion()
    {
        await ComprobarPostSinToken("/Principal?handler=CerrarSesion");
    }

    // La cookie de la sesión tiene que ser HttpOnly
    [TestMethod]
    public async Task IT08_CookieSesion_HttpOnly()
    {
        // La sesión se crea al mandar el email en el primer paso
        Dictionary<string, string> campos = new Dictionary<string, string>();
        campos.Add("NombreUsuario", DatosPrueba.EmailAdmin);
        HttpResponseMessage respuesta = await _web.EnviarFormulario("/Login", null, campos);

        // Juntamos todas las cookies en un texto y lo pasamos a minúsculas
        string cookie = string.Join(";", respuesta.Headers.GetValues("Set-Cookie")).ToLower();

        Assert.IsTrue(cookie.Contains(".aspnetcore.session"));
        Assert.IsTrue(cookie.Contains("httponly"));
    }

    // Pedimos un fichero de estilos y tiene que estar
    private async Task ComprobarFicheroDisponible(string url)
    {
        HttpResponseMessage respuesta = await _web.Get(url);

        Assert.AreEqual(HttpStatusCode.OK, respuesta.StatusCode);
    }

    // Los estilos de Bootstrap se cargan
    [TestMethod]
    public async Task IT09_Estilos_Bootstrap()
    {
        await ComprobarFicheroDisponible("/lib/bootstrap/dist/css/bootstrap.css");
    }

    // Los estilos generales de la web se cargan
    [TestMethod]
    public async Task IT09_Estilos_Site()
    {
        await ComprobarFicheroDisponible("/css/site.css");
    }

    // Los estilos del login se cargan
    [TestMethod]
    public async Task IT09_Estilos_Login()
    {
        await ComprobarFicheroDisponible("/css/login.css");
    }
}

using Datos;
using Nucleo.Modelos;
using System.Net;

namespace Web.Tests;

// Pruebas de integración de los permisos con las páginas y el log
[TestClass]
public class PermisosIntegracionTests
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

    // Función auxiliar para mirar si en el log hay un acceso denegado de ese email
    private bool HayAccesoDenegado(string email)
    {
        foreach (EntradaLog log in _web.Datos.Logs)
        {
            if (log.Accion == Auditoria.AccesoDenegado && log.NombreUsuario == email)
            {
                return true;
            }
        }

        return false;
    }

    // Función auxiliar para mirar si en el log hay algún acceso denegado
    private bool HayAlgunAccesoDenegado()
    {
        foreach (EntradaLog log in _web.Datos.Logs)
        {
            if (log.Accion == Auditoria.AccesoDenegado)
            {
                return true;
            }
        }

        return false;
    }

    // Función auxiliar para buscar un proyecto por sus siglas, o null si no está
    private Proyecto? BuscarProyecto(string siglas)
    {
        foreach (Proyecto p in _web.Datos.Proyectos.Values)
        {
            if (p.Siglas == siglas)
            {
                return p;
            }
        }

        return null;
    }

    // Función auxiliar para rellenar los campos del formulario de crear proyecto
    private static Dictionary<string, string> CamposProyecto(string nombre, string siglas, string descripcion)
    {
        Dictionary<string, string> campos = new Dictionary<string, string>();
        campos.Add("Nombre", nombre);
        campos.Add("Siglas", siglas);
        campos.Add("Descripcion", descripcion);
        return campos;
    }

    // Función auxiliar para rellenar los campos del perfil con el teléfono que digamos
    private static Dictionary<string, string> CamposPerfil(string telefono)
    {
        Dictionary<string, string> campos = new Dictionary<string, string>();
        campos.Add("EMail", DatosPrueba.EmailAdmin);
        campos.Add("Nombre", "Alex");
        campos.Add("Apellidos", "Alumno");
        campos.Add("Telefono", telefono);
        return campos;
    }

    // El administrador puede entrar en el log
    [TestMethod]
    public async Task IT10_Log_Admin()
    {
        await _web.Login(DatosPrueba.EmailAdmin, DatosPrueba.ClaveAdmin);

        HttpResponseMessage respuesta = await _web.Get("/Log");

        Assert.AreEqual(HttpStatusCode.OK, respuesta.StatusCode);
    }

    // Hacemos login, pedimos el log y tiene que denegarlo y apuntarlo
    private async Task ComprobarLogDenegado(string email, string clave)
    {
        // 1. Hacemos login y pedimos el log
        await _web.Login(email, clave);
        HttpResponseMessage respuesta = await _web.Get("/Log");

        // 2. Nos manda a la principal con el mensaje
        Assert.AreEqual("/Principal", ClienteWeb.Destino(respuesta));
        string html = await _web.GetHtml("/Principal");
        Assert.IsTrue(html.Contains("No tienes permiso para acceder a Log."));

        // 3. Queda apuntado en el log
        Assert.IsTrue(HayAccesoDenegado(email));
    }

    // Al tester no le deja entrar en el log
    [TestMethod]
    public async Task IT10_Log_TesterDenegado()
    {
        await ComprobarLogDenegado(DatosPrueba.EmailTester, DatosPrueba.ClaveTester);
    }

    // Al invitado no le deja entrar en el log
    [TestMethod]
    public async Task IT10_Log_InvitadoDenegado()
    {
        await ComprobarLogDenegado(DatosPrueba.EmailInvitado, DatosPrueba.ClaveInvitado);
    }

    // El invitado no tiene el área de proyectos
    [TestMethod]
    public async Task IT11_Proyectos_InvitadoDenegado()
    {
        await _web.Login(DatosPrueba.EmailInvitado, DatosPrueba.ClaveInvitado);

        HttpResponseMessage respuesta = await _web.Get("/Proyectos/Seleccionar");

        Assert.AreEqual("/Principal", ClienteWeb.Destino(respuesta));
        string html = await _web.GetHtml("/Principal");
        Assert.IsTrue(html.Contains("No tienes permiso para acceder a Proyectos."));
    }

    // Cada uno ve solo sus proyectos, y el administrador ve todos y el formulario de alta
    [TestMethod]
    public async Task IT12_Proyectos_CadaUsuarioVeLosSuyos()
    {
        // 1. El administrador entra en proyectos
        await _web.Login(DatosPrueba.EmailAdmin, DatosPrueba.ClaveAdmin);
        string htmlAdmin = await _web.GetHtml("/Proyectos/Seleccionar");

        // 2. El tester entra en proyectos desde otra web distinta
        string htmlTester;
        using (ClienteWeb webTester = new ClienteWeb())
        {
            await webTester.Login(DatosPrueba.EmailTester, DatosPrueba.ClaveTester);
            htmlTester = await webTester.GetHtml("/Proyectos/Seleccionar");
        }

        // 3. El administrador ve ARCH y puede crear proyectos
        Assert.IsTrue(htmlAdmin.Contains("ARCH"));
        Assert.IsTrue(htmlAdmin.Contains("handler=Crear"));

        // 4. El tester solo ve los suyos y no puede crear
        Assert.IsTrue(htmlTester.Contains("UBU-TPMP"));
        Assert.IsTrue(htmlTester.Contains("ACTFIS"));
        Assert.IsFalse(htmlTester.Contains("ARCH"));
        Assert.IsTrue(htmlTester.Contains("Solo el administrador puede crear proyectos."));
        Assert.IsFalse(htmlTester.Contains("handler=Crear"));
    }

    // El administrador crea un proyecto y se guarda en Datos
    [TestMethod]
    public async Task IT13_Admin_CreaProyecto()
    {
        await _web.Login(DatosPrueba.EmailAdmin, DatosPrueba.ClaveAdmin);

        Dictionary<string, string> campos = CamposProyecto("Proyecto de integración", "integ", "Creado desde la prueba");
        HttpResponseMessage respuesta = await _web.EnviarFormulario("/Proyectos/Seleccionar", "Crear", campos);

        Assert.AreEqual(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.AreEqual(DatosPrueba.NumProyectos + 1, _web.Datos.Proyectos.Count);
        // Las siglas se guardan en mayúsculas
        Assert.IsNotNull(BuscarProyecto("INTEG"));

        string html = await _web.GetHtml("/Proyectos/Seleccionar");
        Assert.IsTrue(html.Contains("El proyecto se ha creado y seleccionado."));
    }

    // Unas siglas repetidas no se dan de alta aunque cambien las mayúsculas
    [TestMethod]
    public async Task IT14_Admin_SiglasRepetidas()
    {
        await _web.Login(DatosPrueba.EmailAdmin, DatosPrueba.ClaveAdmin);

        Dictionary<string, string> campos = CamposProyecto("Otro nombre", "ubu-tpmp", "x");
        HttpResponseMessage respuesta = await _web.EnviarFormulario("/Proyectos/Seleccionar", "Crear", campos);
        string html = await ClienteWeb.Html(respuesta);

        Assert.IsTrue(html.Contains("Ya existe un proyecto con esas siglas."));
        Assert.AreEqual(DatosPrueba.NumProyectos, _web.Datos.Proyectos.Count);
    }

    // Aunque se salte la página, uno que no es administrador no puede crear proyectos
    [TestMethod]
    public async Task IT15_Tester_NoPuedeCrearProyecto()
    {
        await _web.Login(DatosPrueba.EmailTester, DatosPrueba.ClaveTester);

        Dictionary<string, string> campos = CamposProyecto("Proyecto pirata", "PIR", "x");
        HttpResponseMessage respuesta = await _web.EnviarFormulario("/Proyectos/Seleccionar", "Crear", campos);
        string html = await ClienteWeb.Html(respuesta);

        Assert.IsTrue(html.Contains("Solo el administrador puede crear proyectos."));
        Assert.AreEqual(DatosPrueba.NumProyectos, _web.Datos.Proyectos.Count);
        Assert.IsTrue(HayAlgunAccesoDenegado());
    }

    // El tester hace login y manda el formulario para seleccionar el proyecto
    private async Task<HttpResponseMessage> TesterSelecciona(int idProyecto)
    {
        await _web.Login(DatosPrueba.EmailTester, DatosPrueba.ClaveTester);

        Dictionary<string, string> campos = new Dictionary<string, string>();
        campos.Add("idProyecto", idProyecto.ToString());
        return await _web.EnviarFormulario("/Proyectos/Seleccionar", "Seleccionar", campos);
    }

    // El tester puede seleccionar ACTFIS porque está asignado
    [TestMethod]
    public async Task IT16_Tester_SeleccionaProyectoAsignado()
    {
        HttpResponseMessage respuesta = await TesterSelecciona(2);

        Assert.AreEqual(HttpStatusCode.Redirect, respuesta.StatusCode);
        string html = await _web.GetHtml("/Proyectos/Seleccionar");
        Assert.IsTrue(html.Contains("se ha seleccionado"));
    }

    // El tester no puede seleccionar ARCH porque no está asignado
    [TestMethod]
    public async Task IT16_Tester_SeleccionaProyectoNoAsignado()
    {
        HttpResponseMessage respuesta = await TesterSelecciona(3);
        string html = await ClienteWeb.Html(respuesta);

        Assert.IsTrue(html.Contains("El proyecto seleccionado no existe."));
    }

    // El tester no puede seleccionar un proyecto que no existe
    [TestMethod]
    public async Task IT16_Tester_SeleccionaProyectoQueNoExiste()
    {
        HttpResponseMessage respuesta = await TesterSelecciona(99);
        string html = await ClienteWeb.Html(respuesta);

        Assert.IsTrue(html.Contains("El proyecto seleccionado no existe."));
    }

    // El teléfono se ve descifrado en el perfil y al cambiarlo se guarda cifrado en Datos
    [TestMethod]
    public async Task IT17_Perfil_TelefonoCifrado()
    {
        // 1. En el perfil sale el teléfono descifrado
        await _web.Login(DatosPrueba.EmailAdmin, DatosPrueba.ClaveAdmin);
        string perfil = await _web.GetHtml("/Usuarios/Perfil");
        Assert.IsTrue(perfil.Contains("value=\"" + DatosPrueba.TelefonoAdmin + "\""));

        // 2. Lo cambiamos por otro
        HttpResponseMessage respuesta = await _web.EnviarFormulario("/Usuarios/Perfil", "GuardarPerfil", CamposPerfil("611222333"));

        // 3. Se guarda cifrado y se puede descifrar con la clave
        Usuario admin = _web.Datos.LeerUsuario(DatosPrueba.EmailAdmin)!;
        Assert.AreEqual(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.IsFalse(admin.TelefonoCifrado.Contains("611222333"));
        Assert.AreEqual("611222333", admin.ObtenerTelefono(ClienteWeb.ClaveCifrado));
    }

    // Un teléfono mal escrito no se guarda
    [TestMethod]
    public async Task IT18_Perfil_TelefonoNoValido()
    {
        await _web.Login(DatosPrueba.EmailAdmin, DatosPrueba.ClaveAdmin);

        HttpResponseMessage respuesta = await _web.EnviarFormulario("/Usuarios/Perfil", "GuardarPerfil", CamposPerfil("12ab"));
        string html = await ClienteWeb.Html(respuesta);

        Assert.IsTrue(html.Contains("El teléfono debe tener 9 cifras."));

        // Sigue teniendo el teléfono de antes
        Usuario admin = _web.Datos.LeerUsuario(DatosPrueba.EmailAdmin)!;
        Assert.AreEqual(DatosPrueba.TelefonoAdmin, admin.ObtenerTelefono(ClienteWeb.ClaveCifrado));
    }

    // La descripción no es obligatoria, se puede crear un proyecto sin ella
    [TestMethod]
    public async Task IT19_Admin_CreaProyectoSinDescripcion()
    {
        await _web.Login(DatosPrueba.EmailAdmin, DatosPrueba.ClaveAdmin);

        Dictionary<string, string> campos = CamposProyecto("Sin descripción", "SDESC", "");
        HttpResponseMessage respuesta = await _web.EnviarFormulario("/Proyectos/Seleccionar", "Crear", campos);

        Assert.AreEqual(HttpStatusCode.Redirect, respuesta.StatusCode);
        Proyecto? creado = BuscarProyecto("SDESC");
        Assert.IsNotNull(creado);
        Assert.AreEqual(string.Empty, creado.Descripcion);
    }
}

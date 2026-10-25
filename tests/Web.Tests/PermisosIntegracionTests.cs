using Datos;
using Nucleo.Modelos;
using System.Net;

namespace Web.Tests;

// Integración de permisos (Nucleo.Permisos) con las páginas y la auditoría, por HTTP
[TestClass]
public class PermisosIntegracionTests
{
    private ClienteWeb _web = null!;

    [TestInitialize]
    public void Inicializar() => _web = new ClienteWeb();

    [TestCleanup]
    public void Limpiar() => _web.Dispose();

    // IT-10: el Log solo lo ve el administrador; a los demás se les deniega y se audita
    [TestMethod]
    [DataRow(DatosPrueba.EmailAdmin, DatosPrueba.ClaveAdmin, true)]
    [DataRow(DatosPrueba.EmailTester, DatosPrueba.ClaveTester, false)]
    [DataRow(DatosPrueba.EmailInvitado, DatosPrueba.ClaveInvitado, false)]
    public async Task IT10_Log_SoloAdministrador(string email, string clave, bool permitido)
    {
        await _web.Login(email, clave);

        HttpResponseMessage respuesta = await _web.Get("/Log");

        if (permitido)
        {
            Assert.AreEqual(HttpStatusCode.OK, respuesta.StatusCode);
            return;
        }

        Assert.AreEqual("/Principal", ClienteWeb.Destino(respuesta));
        Assert.Contains("No tienes permiso para acceder a Log.", await _web.GetHtml("/Principal"));
        Assert.IsTrue(_web.Datos.Logs.Any(l => l.Accion == Auditoria.AccesoDenegado && l.NombreUsuario == email));
    }

    // IT-11: el invitado no tiene el área Proyectos
    [TestMethod]
    public async Task IT11_Proyectos_InvitadoDenegado()
    {
        await _web.Login(DatosPrueba.EmailInvitado, DatosPrueba.ClaveInvitado);

        HttpResponseMessage respuesta = await _web.Get("/Proyectos/Seleccionar");

        Assert.AreEqual("/Principal", ClienteWeb.Destino(respuesta));
        Assert.Contains("No tienes permiso para acceder a Proyectos.", await _web.GetHtml("/Principal"));
    }

    // IT-12: cada uno ve solo sus proyectos; el administrador todos y el formulario de alta
    [TestMethod]
    public async Task IT12_Proyectos_CadaUsuarioVeLosSuyos()
    {
        await _web.Login(DatosPrueba.EmailAdmin, DatosPrueba.ClaveAdmin);
        string htmlAdmin = await _web.GetHtml("/Proyectos/Seleccionar");

        using ClienteWeb tester = new ClienteWeb();
        await tester.Login(DatosPrueba.EmailTester, DatosPrueba.ClaveTester);
        string htmlTester = await tester.GetHtml("/Proyectos/Seleccionar");

        Assert.Contains("ARCH", htmlAdmin);
        Assert.Contains("handler=Crear", htmlAdmin);

        Assert.Contains("UBU-TPMP", htmlTester);
        Assert.Contains("ACTFIS", htmlTester);
        Assert.DoesNotContain("ARCH", htmlTester);
        Assert.Contains("Solo el administrador puede crear proyectos.", htmlTester);
        Assert.DoesNotContain("handler=Crear", htmlTester);
    }

    // IT-13: el administrador crea un proyecto y queda guardado en Datos
    [TestMethod]
    public async Task IT13_Admin_CreaProyecto()
    {
        await _web.Login(DatosPrueba.EmailAdmin, DatosPrueba.ClaveAdmin);

        HttpResponseMessage respuesta = await _web.EnviarFormulario("/Proyectos/Seleccionar", "Crear", new()
        {
            ["Nombre"] = "Proyecto de integración",
            ["Siglas"] = "integ",
            ["Descripcion"] = "Creado desde la prueba"
        });

        Assert.AreEqual(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.HasCount(DatosPrueba.NumProyectos + 1, _web.Datos.Proyectos);
        Assert.IsTrue(_web.Datos.Proyectos.Values.Any(p => p.Siglas == "INTEG"));
        Assert.Contains("El proyecto se ha creado y seleccionado.", await _web.GetHtml("/Proyectos/Seleccionar"));
    }

    // IT-14: siglas repetidas (sin mirar mayúsculas) no se dan de alta
    [TestMethod]
    public async Task IT14_Admin_SiglasRepetidas()
    {
        await _web.Login(DatosPrueba.EmailAdmin, DatosPrueba.ClaveAdmin);

        HttpResponseMessage respuesta = await _web.EnviarFormulario("/Proyectos/Seleccionar", "Crear", new()
        {
            ["Nombre"] = "Otro nombre",
            ["Siglas"] = "ubu-tpmp",
            ["Descripcion"] = "x"
        });

        Assert.Contains("Ya existe un proyecto con esas siglas.", await ClienteWeb.Html(respuesta));
        Assert.HasCount(DatosPrueba.NumProyectos, _web.Datos.Proyectos);
    }

    // IT-15: aunque se salte la interfaz, un no administrador no puede crear proyectos
    [TestMethod]
    public async Task IT15_Tester_NoPuedeCrearProyecto()
    {
        await _web.Login(DatosPrueba.EmailTester, DatosPrueba.ClaveTester);

        HttpResponseMessage respuesta = await _web.EnviarFormulario("/Proyectos/Seleccionar", "Crear", new()
        {
            ["Nombre"] = "Proyecto pirata",
            ["Siglas"] = "PIR",
            ["Descripcion"] = "x"
        });

        Assert.Contains("Solo el administrador puede crear proyectos.", await ClienteWeb.Html(respuesta));
        Assert.HasCount(DatosPrueba.NumProyectos, _web.Datos.Proyectos);
        Assert.IsTrue(_web.Datos.Logs.Any(l => l.Accion == Auditoria.AccesoDenegado));
    }

    // IT-16: seleccionar un proyecto propio funciona; uno ajeno o inactivo no
    [TestMethod]
    [DataRow(2, true, DisplayName = "IT-16 ACTFIS (asignado)")]
    [DataRow(3, false, DisplayName = "IT-16 ARCH (no asignado)")]
    [DataRow(99, false, DisplayName = "IT-16 proyecto que no existe")]
    public async Task IT16_Tester_SeleccionarProyecto(int idProyecto, bool permitido)
    {
        await _web.Login(DatosPrueba.EmailTester, DatosPrueba.ClaveTester);

        HttpResponseMessage respuesta = await _web.EnviarFormulario("/Proyectos/Seleccionar", "Seleccionar",
            new() { ["idProyecto"] = idProyecto.ToString() });
        string html = await ClienteWeb.Html(respuesta);

        if (permitido)
        {
            Assert.AreEqual(HttpStatusCode.Redirect, respuesta.StatusCode);
            Assert.Contains("se ha seleccionado", await _web.GetHtml("/Proyectos/Seleccionar"));
        }
        else
        {
            Assert.Contains("El proyecto seleccionado no existe.", html);
        }
    }

    // IT-17: el teléfono se ve descifrado en el perfil y al cambiarlo se guarda cifrado en Datos
    [TestMethod]
    public async Task IT17_Perfil_TelefonoCifrado()
    {
        await _web.Login(DatosPrueba.EmailAdmin, DatosPrueba.ClaveAdmin);
        Assert.Contains($"value=\"{DatosPrueba.TelefonoAdmin}\"", await _web.GetHtml("/Usuarios/Perfil"));

        HttpResponseMessage respuesta = await _web.EnviarFormulario("/Usuarios/Perfil", "GuardarPerfil", new()
        {
            ["EMail"] = DatosPrueba.EmailAdmin,
            ["Nombre"] = "Alex",
            ["Apellidos"] = "Alumno",
            ["Telefono"] = "611222333"
        });

        Usuario admin = _web.Datos.LeerUsuario(DatosPrueba.EmailAdmin)!;
        Assert.AreEqual(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.DoesNotContain("611222333", admin.TelefonoCifrado);
        Assert.AreEqual("611222333", admin.ObtenerTelefono(ClienteWeb.ClaveCifrado));
    }

    // IT-18: un teléfono mal escrito no se guarda
    [TestMethod]
    public async Task IT18_Perfil_TelefonoNoValido()
    {
        await _web.Login(DatosPrueba.EmailAdmin, DatosPrueba.ClaveAdmin);

        HttpResponseMessage respuesta = await _web.EnviarFormulario("/Usuarios/Perfil", "GuardarPerfil", new()
        {
            ["EMail"] = DatosPrueba.EmailAdmin,
            ["Nombre"] = "Alex",
            ["Apellidos"] = "Alumno",
            ["Telefono"] = "12ab"
        });

        Assert.Contains("El teléfono debe tener 9 cifras.", await ClienteWeb.Html(respuesta));
        Assert.AreEqual(DatosPrueba.TelefonoAdmin,
            _web.Datos.LeerUsuario(DatosPrueba.EmailAdmin)!.ObtenerTelefono(ClienteWeb.ClaveCifrado));
    }
}

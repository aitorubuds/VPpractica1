using Datos;
using OpenQA.Selenium;

namespace Web.UITests;

// Pruebas de la interfaz con Selenium: cada prueba se repite en Chrome, Edge y Firefox
[TestClass]
[TestCategory("UI")]
public class NavegadorTests
{
    public static IEnumerable<object[]> Navegadores =>
    [
        ["chrome"],
        ["edge"],
        ["firefox"]
    ];

    // UI-01: sin sesión, al pedir una página protegida te manda al login
    [TestMethod]
    [DynamicData(nameof(Navegadores))]
    public void UI01_SinSesion_VaAlLogin(string navegador)
    {
        using SesionNavegador s = new SesionNavegador(navegador);

        s.Ir("/Log");

        s.EsperarPagina("/Login");
        Assert.IsTrue(s.Existe("txtUsuario"));
    }

    // UI-02: login correcto, entra a la principal y en el menú sale su nombre
    [TestMethod]
    [DynamicData(nameof(Navegadores))]
    public void UI02_LoginCorrecto(string navegador)
    {
        using SesionNavegador s = new SesionNavegador(navegador);

        s.Login(DatosPrueba.EmailAdmin, DatosPrueba.ClaveAdmin);

        s.EsperarPagina("/Principal");
        Assert.IsFalse(string.IsNullOrWhiteSpace(s.Elemento("btnMenuUsuario").Text));
    }

    // UI-03: contraseña incorrecta, se queda en la página con el mensaje de error
    [TestMethod]
    [DynamicData(nameof(Navegadores))]
    public void UI03_ContrasenaIncorrecta(string navegador)
    {
        using SesionNavegador s = new SesionNavegador(navegador);

        s.Login(DatosPrueba.EmailAdmin, "Mala1234!");

        s.Espera.Until(_ => s.TextoPagina.Contains("Usuario o contraseña incorrectos."));
        Assert.AreEqual("/Login/Index2", s.RutaActual);
    }

    // UI-04: un email mal escrito lo para el propio navegador (input type="email")
    [TestMethod]
    [DynamicData(nameof(Navegadores))]
    public void UI04_EmailMalEscrito_NoAvanza(string navegador)
    {
        using SesionNavegador s = new SesionNavegador(navegador);
        s.Ir("/Login");

        s.Escribir("txtUsuario", "hola");
        s.Elemento("btnSiguiente").Click();

        bool valido = (bool)((IJavaScriptExecutor)s.Driver)
            .ExecuteScript("return arguments[0].validity.valid;", s.Elemento("txtUsuario"))!;
        Assert.IsFalse(valido);
        Assert.AreEqual("/Login", s.RutaActual);
    }

    // UI-05: el botón Atrás de la contraseña vuelve a pedir el email
    [TestMethod]
    [DynamicData(nameof(Navegadores))]
    public void UI05_BotonAtras(string navegador)
    {
        using SesionNavegador s = new SesionNavegador(navegador);
        s.Ir("/Login");
        s.Escribir("txtUsuario", DatosPrueba.EmailAdmin);
        s.Elemento("btnSiguiente").Click();
        s.EsperarPagina("/Login/Index2");

        s.Elemento("btnAtras").Click();

        s.EsperarPagina("/Login");
    }

    // UI-06: tres fallos bloquean la cuenta y luego ni con la buena entra
    [TestMethod]
    [DynamicData(nameof(Navegadores))]
    public void UI06_TresFallos_Bloquea(string navegador)
    {
        using SesionNavegador s = new SesionNavegador(navegador);

        for (int i = 0; i < 3; i++)
        {
            s.Login(DatosPrueba.EmailTester, "Mala1234!");
            s.Espera.Until(_ => s.TextoPagina.Contains("Usuario o contraseña incorrectos."));
        }
        s.Login(DatosPrueba.EmailTester, DatosPrueba.ClaveTester);

        s.Espera.Until(_ => s.TextoPagina.Contains("Usuario o contraseña incorrectos."));
        Assert.AreEqual("/Login/Index2", s.RutaActual);
        Assert.IsTrue(s.Datos.LeerUsuario(DatosPrueba.EmailTester)!.Bloqueado);
    }

    // UI-07: el tester entra al Log desde el menú y se le deniega con mensaje
    [TestMethod]
    [DynamicData(nameof(Navegadores))]
    public void UI07_Tester_LogDenegado(string navegador)
    {
        using SesionNavegador s = new SesionNavegador(navegador);
        s.Login(DatosPrueba.EmailTester, DatosPrueba.ClaveTester);
        s.EsperarPagina("/Principal");

        s.Menu("lnkLog");

        Assert.AreEqual("No tienes permiso para acceder a Log.", s.Elemento("mensajeError").Text);
        Assert.AreEqual("/Principal", s.RutaActual);
    }

    // UI-08: el administrador sí ve el Log, con los eventos (incluido su propio login)
    [TestMethod]
    [DynamicData(nameof(Navegadores))]
    public void UI08_Admin_VeLog(string navegador)
    {
        using SesionNavegador s = new SesionNavegador(navegador);
        s.Login(DatosPrueba.EmailAdmin, DatosPrueba.ClaveAdmin);
        s.EsperarPagina("/Principal");

        s.Menu("lnkLog");

        Assert.Contains(DatosPrueba.EmailAdmin, s.Elemento("tablaLog").Text);
    }

    // UI-09: el tester solo ve sus proyectos y no tiene el formulario de alta
    [TestMethod]
    [DynamicData(nameof(Navegadores))]
    public void UI09_Tester_VeSoloSusProyectos(string navegador)
    {
        using SesionNavegador s = new SesionNavegador(navegador);
        s.Login(DatosPrueba.EmailTester, DatosPrueba.ClaveTester);
        s.EsperarPagina("/Principal");

        s.Menu("lnkProyectos");

        Assert.IsTrue(s.Existe("avisoSoloAdministrador"));
        Assert.IsFalse(s.Existe("btnCrearProyecto"));
        Assert.IsTrue(s.Existe("btnSeleccionar-ACTFIS"));
        Assert.DoesNotContain("ARCH", s.TextoPagina);
    }

    // UI-10: el administrador crea un proyecto desde el formulario
    [TestMethod]
    [DynamicData(nameof(Navegadores))]
    public void UI10_Admin_CreaProyecto(string navegador)
    {
        using SesionNavegador s = new SesionNavegador(navegador);
        s.Login(DatosPrueba.EmailAdmin, DatosPrueba.ClaveAdmin);
        s.EsperarPagina("/Principal");
        s.Menu("lnkProyectos");

        s.Escribir("Nombre", "Proyecto Selenium");
        s.Escribir("Siglas", "sel");
        s.Escribir("Descripcion", "Creado desde el navegador");
        s.Elemento("btnCrearProyecto").Click();

        Assert.AreEqual("El proyecto se ha creado y seleccionado.", s.Elemento("mensajeCorrecto").Text);
        Assert.Contains("Proyecto Selenium", s.TextoPagina);
        Assert.IsTrue(s.Datos.Proyectos.Values.Any(p => p.Siglas == "SEL"));
    }

    // UI-11: siglas repetidas, se muestra el error y no se crea
    [TestMethod]
    [DynamicData(nameof(Navegadores))]
    public void UI11_Admin_SiglasRepetidas(string navegador)
    {
        using SesionNavegador s = new SesionNavegador(navegador);
        s.Login(DatosPrueba.EmailAdmin, DatosPrueba.ClaveAdmin);
        s.EsperarPagina("/Principal");
        s.Menu("lnkProyectos");

        s.Escribir("Nombre", "Otro nombre");
        s.Escribir("Siglas", "actfis");
        s.Elemento("btnCrearProyecto").Click();

        s.Espera.Until(_ => s.TextoPagina.Contains("Ya existe un proyecto con esas siglas."));
        Assert.HasCount(DatosPrueba.NumProyectos, s.Datos.Proyectos);
    }

    // UI-12: el tester selecciona uno de sus proyectos
    [TestMethod]
    [DynamicData(nameof(Navegadores))]
    public void UI12_Tester_SeleccionaProyecto(string navegador)
    {
        using SesionNavegador s = new SesionNavegador(navegador);
        s.Login(DatosPrueba.EmailTester, DatosPrueba.ClaveTester);
        s.EsperarPagina("/Principal");
        s.Menu("lnkProyectos");

        s.Elemento("btnSeleccionar-ACTFIS").Click();

        Assert.Contains("se ha seleccionado", s.Elemento("mensajeCorrecto").Text);
        Assert.IsFalse(s.Existe("btnSeleccionar-ACTFIS"));
    }

    // UI-13: cambiar el teléfono en el perfil y que se mantenga al volver a entrar
    [TestMethod]
    [DynamicData(nameof(Navegadores))]
    public void UI13_Perfil_CambiaTelefono(string navegador)
    {
        using SesionNavegador s = new SesionNavegador(navegador);
        s.Login(DatosPrueba.EmailAdmin, DatosPrueba.ClaveAdmin);
        s.EsperarPagina("/Principal");
        s.Menu("lnkPerfil");
        Assert.AreEqual(DatosPrueba.TelefonoAdmin, s.Elemento("Telefono").GetDomProperty("value"));

        s.Escribir("Telefono", "611222333");
        s.Elemento("btnGuardarPerfil").Click();
        s.Elemento("mensajePerfil");
        s.Ir("/Usuarios/Perfil");

        Assert.AreEqual("611222333", s.Elemento("Telefono").GetDomProperty("value"));
    }

    // UI-14: un teléfono mal escrito muestra el error y no se guarda
    [TestMethod]
    [DynamicData(nameof(Navegadores))]
    public void UI14_Perfil_TelefonoNoValido(string navegador)
    {
        using SesionNavegador s = new SesionNavegador(navegador);
        s.Login(DatosPrueba.EmailAdmin, DatosPrueba.ClaveAdmin);
        s.EsperarPagina("/Principal");
        s.Menu("lnkPerfil");

        s.Escribir("Telefono", "12ab");
        s.Elemento("btnGuardarPerfil").Click();

        s.Espera.Until(_ => s.TextoPagina.Contains("El teléfono debe tener 9 cifras."));
        s.Ir("/Usuarios/Perfil");
        Assert.AreEqual(DatosPrueba.TelefonoAdmin, s.Elemento("Telefono").GetDomProperty("value"));
    }

    // UI-15: cerrar sesión desde el menú y que ya no se pueda volver a la principal
    [TestMethod]
    [DynamicData(nameof(Navegadores))]
    public void UI15_CerrarSesion(string navegador)
    {
        using SesionNavegador s = new SesionNavegador(navegador);
        s.Login(DatosPrueba.EmailAdmin, DatosPrueba.ClaveAdmin);
        s.EsperarPagina("/Principal");

        s.Menu("btnCerrarSesion");
        s.EsperarPagina("/Login");
        s.Ir("/Principal");

        s.EsperarPagina("/Login");
    }
}

using Datos;
using Nucleo.Modelos;
using OpenQA.Selenium;

namespace Web.UITests;

// Pruebas de la interfaz con Selenium, cada prueba se hace en Chrome, Edge y Firefox
[TestClass]
[TestCategory("UI")]
public class NavegadorTests
{
    // Función auxiliar para mirar si hay un proyecto con esas siglas en la base de datos
    private static bool ExisteProyecto(SesionNavegador sesion, string siglas)
    {
        foreach (Proyecto p in sesion.Datos.Proyectos.Values)
        {
            if (p.Siglas == siglas)
            {
                return true;
            }
        }

        return false;
    }

    // Sin sesión, al pedir una página protegida nos manda al login
    [TestMethod]
    [DataRow("chrome")]
    [DataRow("edge")]
    [DataRow("firefox")]
    public void UI01_SinSesion_VaAlLogin(string navegador)
    {
        using (SesionNavegador sesion = new SesionNavegador(navegador))
        {
            sesion.Ir("/Log");

            sesion.EsperarPagina("/Login");
            Assert.IsTrue(sesion.Existe("txtUsuario"));
        }
    }

    // Con el login correcto entra a la principal y en el menú sale su nombre
    [TestMethod]
    [DataRow("chrome")]
    [DataRow("edge")]
    [DataRow("firefox")]
    public void UI02_LoginCorrecto(string navegador)
    {
        using (SesionNavegador sesion = new SesionNavegador(navegador))
        {
            sesion.Login(DatosPrueba.EmailAdmin, DatosPrueba.ClaveAdmin);

            sesion.EsperarPagina("/Principal");
            string nombre = sesion.Elemento("btnMenuUsuario").Text;
            Assert.IsFalse(string.IsNullOrWhiteSpace(nombre));
        }
    }

    // Con la contraseña mal se queda en la página con el mensaje de error
    [TestMethod]
    [DataRow("chrome")]
    [DataRow("edge")]
    [DataRow("firefox")]
    public void UI03_ContrasenaIncorrecta(string navegador)
    {
        using (SesionNavegador sesion = new SesionNavegador(navegador))
        {
            sesion.Login(DatosPrueba.EmailAdmin, "Mala1234!");

            sesion.EsperarTexto("Usuario o contraseña incorrectos.");
            Assert.AreEqual("/Login/Index2", sesion.RutaActual);
        }
    }

    // Un email mal escrito lo para el propio navegador porque el campo es de tipo email
    [TestMethod]
    [DataRow("chrome")]
    [DataRow("edge")]
    [DataRow("firefox")]
    public void UI04_EmailMalEscrito_NoAvanza(string navegador)
    {
        using (SesionNavegador sesion = new SesionNavegador(navegador))
        {
            sesion.Ir("/Login");

            sesion.Escribir("txtUsuario", "hola");
            sesion.Elemento("btnSiguiente").Click();

            // Le preguntamos al navegador si el campo es válido
            IJavaScriptExecutor javascript = (IJavaScriptExecutor)sesion.Driver;
            object? respuesta = javascript.ExecuteScript("return arguments[0].validity.valid;", sesion.Elemento("txtUsuario"));
            bool valido = (bool)respuesta!;

            Assert.IsFalse(valido);
            Assert.AreEqual("/Login", sesion.RutaActual);
        }
    }

    // El botón Atrás de la contraseña vuelve a pedir el email
    [TestMethod]
    [DataRow("chrome")]
    [DataRow("edge")]
    [DataRow("firefox")]
    public void UI05_BotonAtras(string navegador)
    {
        using (SesionNavegador sesion = new SesionNavegador(navegador))
        {
            // Llegamos a la página de la contraseña
            sesion.Ir("/Login");
            sesion.Escribir("txtUsuario", DatosPrueba.EmailAdmin);
            sesion.Elemento("btnSiguiente").Click();
            sesion.EsperarPagina("/Login/Index2");

            // Pulsamos Atrás
            sesion.Elemento("btnAtras").Click();

            sesion.EsperarPagina("/Login");
        }
    }

    // Tres fallos bloquean la cuenta y luego ni con la clave buena entra
    [TestMethod]
    [DataRow("chrome")]
    [DataRow("edge")]
    [DataRow("firefox")]
    public void UI06_TresFallos_Bloquea(string navegador)
    {
        using (SesionNavegador sesion = new SesionNavegador(navegador))
        {
            // Fallamos 3 veces
            sesion.Login(DatosPrueba.EmailTester, "Mala1234!");
            sesion.EsperarTexto("Usuario o contraseña incorrectos.");
            sesion.Login(DatosPrueba.EmailTester, "Mala1234!");
            sesion.EsperarTexto("Usuario o contraseña incorrectos.");
            sesion.Login(DatosPrueba.EmailTester, "Mala1234!");
            sesion.EsperarTexto("Usuario o contraseña incorrectos.");

            // Probamos con la clave buena
            sesion.Login(DatosPrueba.EmailTester, DatosPrueba.ClaveTester);

            sesion.EsperarTexto("Usuario o contraseña incorrectos.");
            Assert.AreEqual("/Login/Index2", sesion.RutaActual);
            Assert.IsTrue(sesion.Datos.LeerUsuario(DatosPrueba.EmailTester)!.Bloqueado);
        }
    }

    // El tester entra al log desde el menú y le sale que no tiene permiso
    [TestMethod]
    [DataRow("chrome")]
    [DataRow("edge")]
    [DataRow("firefox")]
    public void UI07_Tester_LogDenegado(string navegador)
    {
        using (SesionNavegador sesion = new SesionNavegador(navegador))
        {
            sesion.Login(DatosPrueba.EmailTester, DatosPrueba.ClaveTester);
            sesion.EsperarPagina("/Principal");

            sesion.Menu("lnkLog");

            Assert.AreEqual("No tienes permiso para acceder a Log.", sesion.Elemento("mensajeError").Text);
            Assert.AreEqual("/Principal", sesion.RutaActual);
        }
    }

    // El administrador sí ve el log, y en él sale su propio login
    [TestMethod]
    [DataRow("chrome")]
    [DataRow("edge")]
    [DataRow("firefox")]
    public void UI08_Admin_VeLog(string navegador)
    {
        using (SesionNavegador sesion = new SesionNavegador(navegador))
        {
            sesion.Login(DatosPrueba.EmailAdmin, DatosPrueba.ClaveAdmin);
            sesion.EsperarPagina("/Principal");

            sesion.Menu("lnkLog");

            string tabla = sesion.Elemento("tablaLog").Text;
            Assert.IsTrue(tabla.Contains(DatosPrueba.EmailAdmin));
        }
    }

    // El tester solo ve sus proyectos y no tiene el formulario de alta
    [TestMethod]
    [DataRow("chrome")]
    [DataRow("edge")]
    [DataRow("firefox")]
    public void UI09_Tester_VeSoloSusProyectos(string navegador)
    {
        using (SesionNavegador sesion = new SesionNavegador(navegador))
        {
            sesion.Login(DatosPrueba.EmailTester, DatosPrueba.ClaveTester);
            sesion.EsperarPagina("/Principal");

            sesion.Menu("lnkProyectos");

            Assert.IsTrue(sesion.Existe("avisoSoloAdministrador"));
            Assert.IsFalse(sesion.Existe("btnCrearProyecto"));
            Assert.IsTrue(sesion.Existe("btnSeleccionar-ACTFIS"));
            Assert.IsFalse(sesion.TextoPagina.Contains("ARCH"));
        }
    }

    // El administrador crea un proyecto desde el formulario
    [TestMethod]
    [DataRow("chrome")]
    [DataRow("edge")]
    [DataRow("firefox")]
    public void UI10_Admin_CreaProyecto(string navegador)
    {
        using (SesionNavegador sesion = new SesionNavegador(navegador))
        {
            sesion.Login(DatosPrueba.EmailAdmin, DatosPrueba.ClaveAdmin);
            sesion.EsperarPagina("/Principal");
            sesion.Menu("lnkProyectos");

            // Rellenamos el formulario y lo mandamos
            sesion.Escribir("Nombre", "Proyecto Selenium");
            sesion.Escribir("Siglas", "sel");
            sesion.Escribir("Descripcion", "Creado desde el navegador");
            sesion.Elemento("btnCrearProyecto").Click();

            Assert.AreEqual("El proyecto se ha creado y seleccionado.", sesion.Elemento("mensajeCorrecto").Text);
            Assert.IsTrue(sesion.TextoPagina.Contains("Proyecto Selenium"));
            // Las siglas se guardan en mayúsculas
            Assert.IsTrue(ExisteProyecto(sesion, "SEL"));
        }
    }

    // Con siglas repetidas sale el error y no se crea
    [TestMethod]
    [DataRow("chrome")]
    [DataRow("edge")]
    [DataRow("firefox")]
    public void UI11_Admin_SiglasRepetidas(string navegador)
    {
        using (SesionNavegador sesion = new SesionNavegador(navegador))
        {
            sesion.Login(DatosPrueba.EmailAdmin, DatosPrueba.ClaveAdmin);
            sesion.EsperarPagina("/Principal");
            sesion.Menu("lnkProyectos");

            sesion.Escribir("Nombre", "Otro nombre");
            sesion.Escribir("Siglas", "actfis");
            sesion.Elemento("btnCrearProyecto").Click();

            sesion.EsperarTexto("Ya existe un proyecto con esas siglas.");
            Assert.AreEqual(DatosPrueba.NumProyectos, sesion.Datos.Proyectos.Count);
        }
    }

    // El tester selecciona uno de sus proyectos
    [TestMethod]
    [DataRow("chrome")]
    [DataRow("edge")]
    [DataRow("firefox")]
    public void UI12_Tester_SeleccionaProyecto(string navegador)
    {
        using (SesionNavegador sesion = new SesionNavegador(navegador))
        {
            sesion.Login(DatosPrueba.EmailTester, DatosPrueba.ClaveTester);
            sesion.EsperarPagina("/Principal");
            sesion.Menu("lnkProyectos");

            sesion.Elemento("btnSeleccionar-ACTFIS").Click();

            Assert.IsTrue(sesion.Elemento("mensajeCorrecto").Text.Contains("se ha seleccionado"));
            // Ya está seleccionado, así que no sale su botón
            Assert.IsFalse(sesion.Existe("btnSeleccionar-ACTFIS"));
        }
    }

    // Cambiamos el teléfono en el perfil y se mantiene al volver a entrar
    [TestMethod]
    [DataRow("chrome")]
    [DataRow("edge")]
    [DataRow("firefox")]
    public void UI13_Perfil_CambiaTelefono(string navegador)
    {
        using (SesionNavegador sesion = new SesionNavegador(navegador))
        {
            // 1. Entramos al perfil y sale su teléfono
            sesion.Login(DatosPrueba.EmailAdmin, DatosPrueba.ClaveAdmin);
            sesion.EsperarPagina("/Principal");
            sesion.Menu("lnkPerfil");
            Assert.AreEqual(DatosPrueba.TelefonoAdmin, sesion.Elemento("Telefono").GetDomProperty("value"));

            // 2. Lo cambiamos y guardamos
            sesion.Escribir("Telefono", "611222333");
            sesion.Elemento("btnGuardarPerfil").Click();
            sesion.Elemento("mensajePerfil");

            // 3. Volvemos a entrar y sale el nuevo
            sesion.Ir("/Usuarios/Perfil");
            Assert.AreEqual("611222333", sesion.Elemento("Telefono").GetDomProperty("value"));
        }
    }

    // Un teléfono mal escrito muestra el error y no se guarda
    [TestMethod]
    [DataRow("chrome")]
    [DataRow("edge")]
    [DataRow("firefox")]
    public void UI14_Perfil_TelefonoNoValido(string navegador)
    {
        using (SesionNavegador sesion = new SesionNavegador(navegador))
        {
            sesion.Login(DatosPrueba.EmailAdmin, DatosPrueba.ClaveAdmin);
            sesion.EsperarPagina("/Principal");
            sesion.Menu("lnkPerfil");

            sesion.Escribir("Telefono", "12ab");
            sesion.Elemento("btnGuardarPerfil").Click();
            sesion.EsperarTexto("El teléfono debe tener 9 cifras.");

            // Volvemos a entrar y sigue el de antes
            sesion.Ir("/Usuarios/Perfil");
            Assert.AreEqual(DatosPrueba.TelefonoAdmin, sesion.Elemento("Telefono").GetDomProperty("value"));
        }
    }

    // Cerramos sesión desde el menú y ya no se puede volver a la principal
    [TestMethod]
    [DataRow("chrome")]
    [DataRow("edge")]
    [DataRow("firefox")]
    public void UI15_CerrarSesion(string navegador)
    {
        using (SesionNavegador sesion = new SesionNavegador(navegador))
        {
            sesion.Login(DatosPrueba.EmailAdmin, DatosPrueba.ClaveAdmin);
            sesion.EsperarPagina("/Principal");

            sesion.Menu("btnCerrarSesion");
            sesion.EsperarPagina("/Login");

            // Intentamos volver a la principal
            sesion.Ir("/Principal");

            sesion.EsperarPagina("/Login");
        }
    }
}

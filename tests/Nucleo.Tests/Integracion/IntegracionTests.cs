using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nucleo.Modelos;
using Datos;
using Nucleo.Seguridad;

namespace Nucleo.Tests.Integracion;

[TestClass]
public class IntegracionTests
{
    private global::Datos.Datos _datos = null!;

    // vaciar la bd antes de probar el flujo
    [TestInitialize]
    public void Inicializar()
    {
        _datos = new global::Datos.Datos();
        _datos.Reiniciar();
    }

    // Probamos el flujo completo de registrar un usuario, guardarlo en bd y loguearse
    [TestMethod]
    public void Flujo_RegistroYLogin()
    {
        // 1. Creamos el usuario con su clave segura inicial
        Usuario nuevo = new Usuario();
        nuevo.Cuenta = "alex";
        nuevo.Nombre = "Alejandro";
        nuevo.EMail = "alex@ubu.es";
        nuevo.EstablecerContrasena("MiClaveSegura123!");

        // 2. Lo guardamos en la base de datos
        Usuario? guardado = _datos.GuardarUsuario(nuevo);

        Assert.IsNotNull(guardado);
        Assert.IsTrue(guardado.Id > 0);

        // 3. Lo recuperamos de la base de datos simulando un nuevo request
        Usuario? recuperado = _datos.LeerUsuario(guardado.Id);

        Assert.IsNotNull(recuperado);
        // 4. Comprobamos que el login con su clave funciona
        Assert.IsTrue(recuperado.ComprobarContrasena("MiClaveSegura123!"));
    }

    // Probamos el ciclo de fallo de login, bloqueo, registro en logs y cambio de clave
    [TestMethod]
    public void Flujo_BloqueoYCambioClave()
    {
        // 1. Damos de alta al usuario
        Usuario u = new Usuario { Cuenta = "aitor", Nombre = "Aitor" };
        u.EstablecerContrasena("ClaveSecreta123!");
        _datos.GuardarUsuario(u);

        // 2. Alguien intenta entrar y falla 3 veces
        u.ComprobarContrasena("Mala1");
        u.ComprobarContrasena("Mala2");
        u.ComprobarContrasena("Mala3");

        // Verificamos que se ha bloqueado
        Assert.IsTrue(u.Bloqueado);

        // 3. Registramos en el log que la cuenta ha sido bloqueada
        EntradaLog avisoLog = new EntradaLog
        {
            UsuarioId = u.Id,
            Accion = "Cuenta bloqueada por 3 intentos fallidos"
        };
        _datos.AgregarLog(avisoLog);

        Assert.AreEqual(1, _datos.Logs.Count);

        // 4. El administrador desbloquea la cuenta
        u.Desbloquear();
        Assert.IsFalse(u.Bloqueado);

        // 5. El usuario cambia su contraseña a una nueva
        bool cambioOk = u.CambiarContrasena("ClaveSecreta123!", "NuevaClaveSuperSegura999!");
        Assert.IsTrue(cambioOk);

        // Guardamos los cambios en la base de datos
        _datos.GuardarUsuario(u);

        // 6. Verificamos que ahora solo puede entrar con la contraseña nueva
        Usuario? usuarioActualizado = _datos.LeerUsuario(u.Id);
        Assert.IsNotNull(usuarioActualizado);
        Assert.IsFalse(usuarioActualizado.ComprobarContrasena("ClaveSecreta123!"));
        Assert.IsTrue(usuarioActualizado.ComprobarContrasena("NuevaClaveSuperSegura999!"));
    }

    // Con los datos de prueba, cada rol tiene acceso solo a sus áreas (Datos + Permisos)
    [TestMethod]
    [DataRow(DatosPrueba.EmailAdmin, true, true, true)]
    [DataRow(DatosPrueba.EmailTester, true, true, false)]
    [DataRow(DatosPrueba.EmailInvitado, false, true, false)]
    public void Flujo_PermisosSegunRol(string email, bool proyectos, bool perfil, bool log)
    {
        IDatos datos = DatosPrueba.Crear(ServicioCifrado.GenerarClave());
        Usuario u = datos.LeerUsuario(email)!;
        Rol? rol = datos.LeerRol(u.RolId);

        Assert.AreEqual(proyectos, Permisos.TieneAcceso(u, rol, Permisos.AreaProyectos));
        Assert.AreEqual(perfil, Permisos.TieneAcceso(u, rol, Permisos.AreaPerfil));
        Assert.AreEqual(log, Permisos.TieneAcceso(u, rol, Permisos.AreaLog));
    }

    // Al borrar el rol del tester se queda sin rol y pierde el acceso (cascada de Datos + Permisos)
    [TestMethod]
    public void Flujo_BorrarRol_QuitaAcceso()
    {
        IDatos datos = DatosPrueba.Crear(ServicioCifrado.GenerarClave());
        Usuario tester = datos.LeerUsuario(DatosPrueba.EmailTester)!;
        int idRol = tester.RolId;

        datos.EliminarRol(idRol);
        tester = datos.LeerUsuario(DatosPrueba.EmailTester)!;

        Assert.AreEqual(0, tester.RolId);
        Assert.IsFalse(Permisos.TieneAcceso(tester, datos.LeerRol(tester.RolId), Permisos.AreaProyectos));
    }

    // Al borrar al tester deja de estar asignado a sus proyectos, pero su historial de log se mantiene
    [TestMethod]
    public void Flujo_BorrarUsuario_LoQuitaDeProyectos()
    {
        IDatos datos = DatosPrueba.Crear(ServicioCifrado.GenerarClave());
        Usuario tester = datos.LeerUsuario(DatosPrueba.EmailTester)!;
        Auditoria.Registrar(datos, Auditoria.NivelInfo, Auditoria.LoginCorrecto, "Entra", tester);
        int logsAntes = datos.Logs.Count;

        datos.EliminarUsuario(tester.Id);

        Assert.IsFalse(datos.Proyectos.Values.Any(p => Permisos.PuedeVerProyecto(tester, p)));
        Assert.HasCount(logsAntes, datos.Logs);
        Assert.IsTrue(datos.Logs.Any(l => l.UsuarioId == tester.Id));
    }

    // Login completo sin web: fallos auditados, bloqueo, desbloqueo, el teléfono cifrado sigue legible
    [TestMethod]
    public void Flujo_LoginAuditadoYTelefono()
    {
        string clave = ServicioCifrado.GenerarClave();
        IDatos datos = DatosPrueba.Crear(clave);
        Usuario admin = datos.LeerUsuario(DatosPrueba.EmailAdmin)!;
        int logsAntes = datos.Logs.Count;

        for (int i = 0; i < 3; i++)
        {
            Assert.IsFalse(admin.ComprobarContrasena("Mala1234!"));
            Auditoria.Registrar(datos, Auditoria.NivelAviso, Auditoria.LoginFallido, "Fallo", admin);
        }
        Assert.IsTrue(datos.LeerUsuario(DatosPrueba.EmailAdmin)!.Bloqueado);

        admin.Desbloquear();
        datos.GuardarUsuario(admin);

        Assert.IsTrue(datos.LeerUsuario(DatosPrueba.EmailAdmin)!.ComprobarContrasena(DatosPrueba.ClaveAdmin));
        Assert.HasCount(logsAntes + 3, datos.Logs);
        Assert.AreEqual(DatosPrueba.TelefonoAdmin, admin.ObtenerTelefono(clave));
    }
}

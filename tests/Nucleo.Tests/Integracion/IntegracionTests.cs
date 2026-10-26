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

    // Con los datos de prueba, el administrador entra en proyectos, perfil y log
    [TestMethod]
    public void Flujo_PermisosAdmin()
    {
        // 1. Cargamos los datos de prueba y buscamos al administrador
        IDatos datos = DatosPrueba.Crear(ServicioCifrado.GenerarClave());
        Usuario admin = datos.LeerUsuario(DatosPrueba.EmailAdmin)!;
        Rol? rol = datos.LeerRol(admin.RolId);

        // 2. Comprobamos a qué áreas tiene acceso
        Assert.IsTrue(Permisos.TieneAcceso(admin, rol, Permisos.AreaProyectos));
        Assert.IsTrue(Permisos.TieneAcceso(admin, rol, Permisos.AreaPerfil));
        Assert.IsTrue(Permisos.TieneAcceso(admin, rol, Permisos.AreaLog));
    }

    // Con los datos de prueba, el tester entra en proyectos y perfil pero no en el log
    [TestMethod]
    public void Flujo_PermisosTester()
    {
        // 1. Cargamos los datos de prueba y buscamos al tester
        IDatos datos = DatosPrueba.Crear(ServicioCifrado.GenerarClave());
        Usuario tester = datos.LeerUsuario(DatosPrueba.EmailTester)!;
        Rol? rol = datos.LeerRol(tester.RolId);

        // 2. Comprobamos a qué áreas tiene acceso
        Assert.IsTrue(Permisos.TieneAcceso(tester, rol, Permisos.AreaProyectos));
        Assert.IsTrue(Permisos.TieneAcceso(tester, rol, Permisos.AreaPerfil));
        Assert.IsFalse(Permisos.TieneAcceso(tester, rol, Permisos.AreaLog));
    }

    // Con los datos de prueba, el invitado solo entra en su perfil
    [TestMethod]
    public void Flujo_PermisosInvitado()
    {
        // 1. Cargamos los datos de prueba y buscamos al invitado
        IDatos datos = DatosPrueba.Crear(ServicioCifrado.GenerarClave());
        Usuario invitado = datos.LeerUsuario(DatosPrueba.EmailInvitado)!;
        Rol? rol = datos.LeerRol(invitado.RolId);

        // 2. Comprobamos a qué áreas tiene acceso
        Assert.IsFalse(Permisos.TieneAcceso(invitado, rol, Permisos.AreaProyectos));
        Assert.IsTrue(Permisos.TieneAcceso(invitado, rol, Permisos.AreaPerfil));
        Assert.IsFalse(Permisos.TieneAcceso(invitado, rol, Permisos.AreaLog));
    }

    // Si borramos el rol del tester se queda sin rol y ya no puede entrar
    [TestMethod]
    public void Flujo_BorrarRol_QuitaAcceso()
    {
        // 1. Cargamos los datos de prueba y buscamos el rol del tester
        IDatos datos = DatosPrueba.Crear(ServicioCifrado.GenerarClave());
        Usuario tester = datos.LeerUsuario(DatosPrueba.EmailTester)!;
        int idRol = tester.RolId;

        // 2. Borramos ese rol
        datos.EliminarRol(idRol);

        // 3. Volvemos a leer al tester y se ha quedado sin rol
        tester = datos.LeerUsuario(DatosPrueba.EmailTester)!;
        Assert.AreEqual(0, tester.RolId);

        // 4. Sin rol ya no puede entrar en proyectos
        Rol? rol = datos.LeerRol(tester.RolId);
        Assert.IsFalse(Permisos.TieneAcceso(tester, rol, Permisos.AreaProyectos));
    }

    // Al borrar al tester deja de estar asignado a sus proyectos, pero su historial de log se mantiene
    [TestMethod]
    public void Flujo_BorrarUsuario_LoQuitaDeProyectos()
    {
        // 1. Cargamos los datos de prueba y el tester inicia sesión
        IDatos datos = DatosPrueba.Crear(ServicioCifrado.GenerarClave());
        Usuario tester = datos.LeerUsuario(DatosPrueba.EmailTester)!;
        Auditoria.Registrar(datos, Auditoria.NivelInfo, Auditoria.LoginCorrecto, "Entra", tester);
        int logsAntes = datos.Logs.Count;

        // 2. Borramos al tester
        datos.EliminarUsuario(tester.Id);

        // 3. Ya no puede ver ningún proyecto
        foreach (Proyecto p in datos.Proyectos.Values)
        {
            Assert.IsFalse(Permisos.PuedeVerProyecto(tester, p));
        }

        // 4. Los logs siguen ahí, incluidos los suyos
        Assert.AreEqual(logsAntes, datos.Logs.Count);

        bool tieneLogs = false;
        foreach (EntradaLog log in datos.Logs)
        {
            if (log.UsuarioId == tester.Id)
            {
                tieneLogs = true;
            }
        }
        Assert.IsTrue(tieneLogs);
    }

    // Probamos el login entero, falla 3 veces, se bloquea, lo desbloqueamos y el teléfono se sigue leyendo
    [TestMethod]
    public void Flujo_LoginAuditadoYTelefono()
    {
        // 1. Cargamos los datos de prueba y buscamos al administrador
        string clave = ServicioCifrado.GenerarClave();
        IDatos datos = DatosPrueba.Crear(clave);
        Usuario admin = datos.LeerUsuario(DatosPrueba.EmailAdmin)!;
        int logsAntes = datos.Logs.Count;

        // 2. Falla 3 veces y cada fallo queda en el log
        Assert.IsFalse(admin.ComprobarContrasena("Mala1234!"));
        Auditoria.Registrar(datos, Auditoria.NivelAviso, Auditoria.LoginFallido, "Fallo", admin);
        Assert.IsFalse(admin.ComprobarContrasena("Mala1234!"));
        Auditoria.Registrar(datos, Auditoria.NivelAviso, Auditoria.LoginFallido, "Fallo", admin);
        Assert.IsFalse(admin.ComprobarContrasena("Mala1234!"));
        Auditoria.Registrar(datos, Auditoria.NivelAviso, Auditoria.LoginFallido, "Fallo", admin);

        // Verificamos que se ha bloqueado
        Assert.IsTrue(datos.LeerUsuario(DatosPrueba.EmailAdmin)!.Bloqueado);

        // 3. Lo desbloqueamos y guardamos
        admin.Desbloquear();
        datos.GuardarUsuario(admin);

        // 4. Ya puede entrar con su clave
        Usuario recuperado = datos.LeerUsuario(DatosPrueba.EmailAdmin)!;
        Assert.IsTrue(recuperado.ComprobarContrasena(DatosPrueba.ClaveAdmin));

        // 5. Hay 3 logs nuevos y el teléfono se sigue leyendo
        Assert.AreEqual(logsAntes + 3, datos.Logs.Count);
        Assert.AreEqual(DatosPrueba.TelefonoAdmin, admin.ObtenerTelefono(clave));
    }
}

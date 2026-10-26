using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nucleo.Modelos;
using Nucleo.Seguridad;
using System.Collections.Generic;

namespace Nucleo.Tests.Seguridad;

// Pruebas de los permisos por área, por proyecto y del administrador
[TestClass]
public class PermisosTests
{
    // Función auxiliar para crear el rol Tester con acceso a proyectos y perfil
    private static Rol CrearRolTester()
    {
        Rol r = new Rol();
        r.Id = 2;
        r.Nombre = "Tester";
        r.Permisos = new List<string> { Permisos.AreaProyectos, Permisos.AreaPerfil };
        return r;
    }

    // Función auxiliar para crear un usuario de prueba con el rol que digamos
    private static Usuario CrearUsuario(int rolId = 2, bool admin = false)
    {
        Usuario u = new Usuario();
        u.Id = 5;
        u.Cuenta = "u";
        u.RolId = rolId;
        u.EsAdministrador = admin;
        return u;
    }

    // El rol Tester da acceso al área de proyectos
    [TestMethod]
    public void TieneAcceso_AreaProyectos()
    {
        bool resultado = Permisos.TieneAcceso(CrearUsuario(), CrearRolTester(), Permisos.AreaProyectos);

        Assert.IsTrue(resultado);
    }

    // El rol Tester da acceso al área de perfil
    [TestMethod]
    public void TieneAcceso_AreaPerfil()
    {
        bool resultado = Permisos.TieneAcceso(CrearUsuario(), CrearRolTester(), Permisos.AreaPerfil);

        Assert.IsTrue(resultado);
    }

    // El área se compara sin mirar mayúsculas
    [TestMethod]
    public void TieneAcceso_AreaEnMinusculas()
    {
        bool resultado = Permisos.TieneAcceso(CrearUsuario(), CrearRolTester(), "proyectos");

        Assert.IsTrue(resultado);
    }

    // El rol Tester no da acceso al log
    [TestMethod]
    public void TieneAcceso_AreaLog()
    {
        bool resultado = Permisos.TieneAcceso(CrearUsuario(), CrearRolTester(), Permisos.AreaLog);

        Assert.IsFalse(resultado);
    }

    // El rol Tester no da acceso a la gestión de usuarios
    [TestMethod]
    public void TieneAcceso_AreaUsuarios()
    {
        bool resultado = Permisos.TieneAcceso(CrearUsuario(), CrearRolTester(), Permisos.AreaUsuarios);

        Assert.IsFalse(resultado);
    }

    // El administrador entra en el log aunque no tenga rol
    [TestMethod]
    public void TieneAcceso_AdminLog()
    {
        Usuario admin = CrearUsuario(rolId: 0, admin: true);

        bool resultado = Permisos.TieneAcceso(admin, null, Permisos.AreaLog);

        Assert.IsTrue(resultado);
    }

    // El administrador entra en la gestión de usuarios aunque no tenga rol
    [TestMethod]
    public void TieneAcceso_AdminUsuarios()
    {
        Usuario admin = CrearUsuario(rolId: 0, admin: true);

        bool resultado = Permisos.TieneAcceso(admin, null, Permisos.AreaUsuarios);

        Assert.IsTrue(resultado);
    }

    // Sin usuario, sin rol, con un rol que no es el suyo, inactivo o bloqueado no entra
    [TestMethod]
    public void TieneAcceso_CasosDenegados()
    {
        Rol tester = CrearRolTester();
        Usuario inactivo = CrearUsuario();
        inactivo.Activo = false;
        Usuario bloqueado = CrearUsuario(admin: true);
        bloqueado.Bloqueado = true;

        Assert.IsFalse(Permisos.TieneAcceso(null, tester, Permisos.AreaPerfil));
        Assert.IsFalse(Permisos.TieneAcceso(CrearUsuario(), null, Permisos.AreaPerfil));
        Assert.IsFalse(Permisos.TieneAcceso(CrearUsuario(rolId: 3), tester, Permisos.AreaPerfil));
        Assert.IsFalse(Permisos.TieneAcceso(inactivo, tester, Permisos.AreaPerfil));
        Assert.IsFalse(Permisos.TieneAcceso(bloqueado, tester, Permisos.AreaPerfil));
    }

    // Solo ve los proyectos donde está asignado, menos el administrador que ve todos
    [TestMethod]
    public void PuedeVerProyecto_SegunAsignacion()
    {
        // Un proyecto donde está el usuario 5 y otro donde no
        Proyecto asignado = new Proyecto();
        asignado.Id = 1;
        asignado.UsuariosAsignados = new List<int> { 5 };

        Proyecto ajeno = new Proyecto();
        ajeno.Id = 2;
        ajeno.UsuariosAsignados = new List<int> { 9 };

        Assert.IsTrue(Permisos.PuedeVerProyecto(CrearUsuario(), asignado));
        Assert.IsFalse(Permisos.PuedeVerProyecto(CrearUsuario(), ajeno));
        Assert.IsTrue(Permisos.PuedeVerProyecto(CrearUsuario(admin: true), ajeno));
        Assert.IsFalse(Permisos.PuedeVerProyecto(null, asignado));
        Assert.IsFalse(Permisos.PuedeVerProyecto(CrearUsuario(), null));
    }

    // Solo un administrador activo y sin bloquear crea proyectos
    [TestMethod]
    public void PuedeCrearProyectos_SoloAdministrador()
    {
        Usuario adminInactivo = CrearUsuario(admin: true);
        adminInactivo.Activo = false;
        Usuario adminBloqueado = CrearUsuario(admin: true);
        adminBloqueado.Bloqueado = true;

        Assert.IsTrue(Permisos.PuedeCrearProyectos(CrearUsuario(admin: true)));
        Assert.IsFalse(Permisos.PuedeCrearProyectos(CrearUsuario()));
        Assert.IsFalse(Permisos.PuedeCrearProyectos(adminInactivo));
        Assert.IsFalse(Permisos.PuedeCrearProyectos(adminBloqueado));
        Assert.IsFalse(Permisos.PuedeCrearProyectos(null));
    }
}

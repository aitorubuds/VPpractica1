using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nucleo.Modelos;
using Nucleo.Seguridad;
using System.Collections.Generic;

namespace Nucleo.Tests.Seguridad;

// Permisos por área, por proyecto y del administrador (acta del cliente)
[TestClass]
public class PermisosTests
{
    private static readonly Rol RolTester = new Rol
    {
        Id = 2,
        Nombre = "Tester",
        Permisos = new List<string> { Permisos.AreaProyectos, Permisos.AreaPerfil }
    };

    private static Usuario CrearUsuario(int rolId = 2, bool admin = false)
    {
        return new Usuario { Id = 5, Cuenta = "u", RolId = rolId, EsAdministrador = admin };
    }

    #region TieneAcceso

    // El rol da acceso a sus áreas y no a las demás
    [TestMethod]
    [DataRow(Permisos.AreaProyectos, true)]
    [DataRow(Permisos.AreaPerfil, true)]
    [DataRow("proyectos", true)]
    [DataRow(Permisos.AreaLog, false)]
    [DataRow(Permisos.AreaUsuarios, false)]
    public void TieneAcceso_SegunPermisosDelRol(string area, bool esperado)
    {
        Assert.AreEqual(esperado, Permisos.TieneAcceso(CrearUsuario(), RolTester, area));
    }

    // El administrador entra en cualquier área aunque no tenga rol
    [TestMethod]
    [DataRow(Permisos.AreaLog)]
    [DataRow(Permisos.AreaUsuarios)]
    public void TieneAcceso_Administrador_Siempre(string area)
    {
        Assert.IsTrue(Permisos.TieneAcceso(CrearUsuario(rolId: 0, admin: true), null, area));
    }

    // Sin usuario, sin rol, con un rol que no es el suyo, inactivo o bloqueado -> no entra
    [TestMethod]
    public void TieneAcceso_CasosDenegados()
    {
        Usuario inactivo = CrearUsuario();
        inactivo.Activo = false;
        Usuario bloqueado = CrearUsuario(admin: true);
        bloqueado.Bloqueado = true;

        Assert.IsFalse(Permisos.TieneAcceso(null, RolTester, Permisos.AreaPerfil));
        Assert.IsFalse(Permisos.TieneAcceso(CrearUsuario(), null, Permisos.AreaPerfil));
        Assert.IsFalse(Permisos.TieneAcceso(CrearUsuario(rolId: 3), RolTester, Permisos.AreaPerfil));
        Assert.IsFalse(Permisos.TieneAcceso(inactivo, RolTester, Permisos.AreaPerfil));
        Assert.IsFalse(Permisos.TieneAcceso(bloqueado, RolTester, Permisos.AreaPerfil));
    }

    #endregion

    #region Proyectos

    // Solo ve los proyectos donde está asignado; el administrador ve todos
    [TestMethod]
    public void PuedeVerProyecto_SegunAsignacion()
    {
        Proyecto asignado = new Proyecto { Id = 1, UsuariosAsignados = new List<int> { 5 } };
        Proyecto ajeno = new Proyecto { Id = 2, UsuariosAsignados = new List<int> { 9 } };

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

    #endregion
}

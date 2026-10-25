using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nucleo.Modelos;
using Nucleo.Seguridad;
using Datos;
using System.Linq;

namespace Nucleo.Tests.Datos;

// Comprueba que DatosPrueba deja siempre la base de datos en el mismo estado
[TestClass]
public class DatosPruebaTests
{
    private global::Datos.Datos _datos = null!;
    private string _clave = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _clave = ServicioCifrado.GenerarClave();
        _datos = DatosPrueba.Crear(_clave);
    }

    // 1. Se cargan el número de elementos esperado
    [TestMethod]
    public void Crear_CargaNumeroConocido()
    {
        Assert.AreEqual(DatosPrueba.NumUsuarios, _datos.Usuarios.Count);
        Assert.AreEqual(DatosPrueba.NumRoles, _datos.Roles.Count);
        Assert.AreEqual(DatosPrueba.NumProyectos, _datos.Proyectos.Count);
        Assert.AreEqual(DatosPrueba.NumLogs, _datos.Logs.Count);
    }

    // 2. Los usuarios activos pueden entrar con su contraseña
    [TestMethod]
    [DataRow(DatosPrueba.EmailAdmin, DatosPrueba.ClaveAdmin)]
    [DataRow(DatosPrueba.EmailTester, DatosPrueba.ClaveTester)]
    [DataRow(DatosPrueba.EmailInvitado, DatosPrueba.ClaveInvitado)]
    public void UsuariosActivos_EntranConSuClave(string email, string clave)
    {
        Usuario? usuario = _datos.LeerUsuario(email);

        Assert.IsNotNull(usuario);
        Assert.IsTrue(usuario.ComprobarContrasena(clave));
    }

    // 3. El usuario dado de baja existe pero no puede entrar
    [TestMethod]
    public void UsuarioBaja_NoPuedeEntrar()
    {
        Usuario? baja = _datos.LeerUsuario(DatosPrueba.EmailBaja);

        Assert.IsNotNull(baja);
        Assert.IsFalse(baja.Activo);
        Assert.IsFalse(baja.ComprobarContrasena(DatosPrueba.ClaveBaja));
    }

    // 4. Los Ids son los esperados (1, 2, 3...) y cada usuario tiene su rol
    [TestMethod]
    public void Crear_IdsYRolesConocidos()
    {
        Assert.AreEqual(1, _datos.LeerUsuario(DatosPrueba.EmailAdmin)!.Id);
        Assert.AreEqual("Administrador", _datos.LeerRol(_datos.LeerUsuario(DatosPrueba.EmailAdmin)!.RolId)!.Nombre);
        Assert.AreEqual("Tester", _datos.LeerRol(_datos.LeerUsuario(DatosPrueba.EmailTester)!.RolId)!.Nombre);
        Assert.AreEqual("Invitado", _datos.LeerRol(_datos.LeerUsuario(DatosPrueba.EmailInvitado)!.RolId)!.Nombre);
        Assert.AreEqual("UBU-TPMP", _datos.LeerProyecto(1)!.Siglas);
    }

    // 5. Los proyectos apuntan a usuarios que existen y hay uno inactivo
    [TestMethod]
    public void Proyectos_ReferenciasValidas()
    {
        foreach (Proyecto proyecto in _datos.Proyectos.Values)
        {
            foreach (int idUsuario in proyecto.UsuariosAsignados)
            {
                Assert.IsNotNull(_datos.LeerUsuario(idUsuario));
            }
        }

        Assert.AreEqual(1, _datos.Proyectos.Values.Count(p => !p.Activo));
    }

    // 6. Cargar otra vez después de cambios vuelve al estado conocido
    [TestMethod]
    public void Cargar_DespuesDeCambios_VuelveAlEstadoConocido()
    {
        _datos.EliminarUsuario(1);
        _datos.GuardarProyecto(new Proyecto { Nombre = "Nuevo" });
        _datos.LimpiarLog();

        DatosPrueba.Cargar(_datos, _clave);

        Assert.AreEqual(DatosPrueba.NumUsuarios, _datos.Usuarios.Count);
        Assert.AreEqual(DatosPrueba.NumProyectos, _datos.Proyectos.Count);
        Assert.AreEqual(DatosPrueba.NumLogs, _datos.Logs.Count);
        Assert.AreEqual(1, _datos.LeerUsuario(DatosPrueba.EmailAdmin)!.Id);
    }

    // 7. Se puede simular el alta de elementos partiendo de los datos conocidos
    [TestMethod]
    public void Simular_AltaSobreDatosConocidos()
    {
        Usuario nuevo = new Usuario { Cuenta = "nuevo", EMail = "nuevo@ubu.es" };
        Usuario duplicado = new Usuario { Cuenta = "copia", EMail = DatosPrueba.EmailTester };

        Assert.AreEqual(DatosPrueba.NumUsuarios + 1, _datos.GuardarUsuario(nuevo)!.Id);
        Assert.IsNull(_datos.GuardarUsuario(duplicado));
    }

    // 8. Solo admin es administrador
    [TestMethod]
    public void SoloAdmin_EsAdministrador()
    {
        Assert.AreEqual(1, _datos.Usuarios.Values.Count(u => u.EsAdministrador));
        Assert.IsTrue(_datos.LeerUsuario(DatosPrueba.EmailAdmin)!.EsAdministrador);
    }

    // 9. Los teléfonos se guardan cifrados y se recuperan con la clave
    [TestMethod]
    [DataRow(DatosPrueba.EmailAdmin, DatosPrueba.TelefonoAdmin)]
    [DataRow(DatosPrueba.EmailTester, DatosPrueba.TelefonoTester)]
    public void Telefonos_GuardadosCifrados(string email, string telefono)
    {
        Usuario usuario = _datos.LeerUsuario(email)!;

        Assert.AreNotEqual(telefono, usuario.TelefonoCifrado);
        Assert.DoesNotContain(telefono, usuario.TelefonoCifrado);
        Assert.AreEqual(telefono, usuario.ObtenerTelefono(_clave));
    }
}

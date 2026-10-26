using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nucleo.Modelos;
using Nucleo.Seguridad;
using Datos;

namespace Nucleo.Tests.Datos;

// Comprueba que DatosPrueba deja siempre la base de datos en el mismo estado
[TestClass]
public class DatosPruebaTests
{
    private global::Datos.Datos _datos = null!;
    private string _clave = null!;

    // Cargar los datos de prueba antes de cada test
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

    // 2. El administrador puede entrar con su contraseña
    [TestMethod]
    public void Admin_EntraConSuClave()
    {
        Usuario? admin = _datos.LeerUsuario(DatosPrueba.EmailAdmin);

        Assert.IsNotNull(admin);
        Assert.IsTrue(admin.ComprobarContrasena(DatosPrueba.ClaveAdmin));
    }

    // 3. El tester puede entrar con su contraseña
    [TestMethod]
    public void Tester_EntraConSuClave()
    {
        Usuario? tester = _datos.LeerUsuario(DatosPrueba.EmailTester);

        Assert.IsNotNull(tester);
        Assert.IsTrue(tester.ComprobarContrasena(DatosPrueba.ClaveTester));
    }

    // 4. El invitado puede entrar con su contraseña
    [TestMethod]
    public void Invitado_EntraConSuClave()
    {
        Usuario? invitado = _datos.LeerUsuario(DatosPrueba.EmailInvitado);

        Assert.IsNotNull(invitado);
        Assert.IsTrue(invitado.ComprobarContrasena(DatosPrueba.ClaveInvitado));
    }

    // 5. El usuario dado de baja existe pero no puede entrar
    [TestMethod]
    public void UsuarioBaja_NoPuedeEntrar()
    {
        Usuario? baja = _datos.LeerUsuario(DatosPrueba.EmailBaja);

        Assert.IsNotNull(baja);
        Assert.IsFalse(baja.Activo);
        Assert.IsFalse(baja.ComprobarContrasena(DatosPrueba.ClaveBaja));
    }

    // 6. Los Ids empiezan en 1 y cada usuario tiene su rol
    [TestMethod]
    public void Crear_IdsYRolesConocidos()
    {
        Usuario admin = _datos.LeerUsuario(DatosPrueba.EmailAdmin)!;
        Usuario tester = _datos.LeerUsuario(DatosPrueba.EmailTester)!;
        Usuario invitado = _datos.LeerUsuario(DatosPrueba.EmailInvitado)!;
        Proyecto proyecto = _datos.LeerProyecto(1)!;

        // El administrador es el primero
        Assert.AreEqual(1, admin.Id);

        // Cada uno tiene su rol
        Assert.AreEqual("Administrador", _datos.LeerRol(admin.RolId)!.Nombre);
        Assert.AreEqual("Tester", _datos.LeerRol(tester.RolId)!.Nombre);
        Assert.AreEqual("Invitado", _datos.LeerRol(invitado.RolId)!.Nombre);

        Assert.AreEqual("UBU-TPMP", proyecto.Siglas);
    }

    // 7. Los proyectos apuntan a usuarios que existen y hay uno inactivo
    [TestMethod]
    public void Proyectos_ReferenciasValidas()
    {
        int inactivos = 0;

        foreach (Proyecto proyecto in _datos.Proyectos.Values)
        {
            // Todos los usuarios asignados tienen que existir
            foreach (int idUsuario in proyecto.UsuariosAsignados)
            {
                Assert.IsNotNull(_datos.LeerUsuario(idUsuario));
            }

            // Contamos los proyectos inactivos
            if (!proyecto.Activo)
            {
                inactivos++;
            }
        }

        Assert.AreEqual(1, inactivos);
    }

    // 8. Cargar otra vez después de cambios vuelve al estado conocido
    [TestMethod]
    public void Cargar_DespuesDeCambios_VuelveAlEstadoConocido()
    {
        // Hacemos cambios en los datos
        _datos.EliminarUsuario(1);
        _datos.GuardarProyecto(new Proyecto { Nombre = "Nuevo" });
        _datos.LimpiarLog();

        // Volvemos a cargar
        DatosPrueba.Cargar(_datos, _clave);

        Assert.AreEqual(DatosPrueba.NumUsuarios, _datos.Usuarios.Count);
        Assert.AreEqual(DatosPrueba.NumProyectos, _datos.Proyectos.Count);
        Assert.AreEqual(DatosPrueba.NumLogs, _datos.Logs.Count);
        Assert.AreEqual(1, _datos.LeerUsuario(DatosPrueba.EmailAdmin)!.Id);
    }

    // 9. Se puede simular el alta de elementos partiendo de los datos conocidos
    [TestMethod]
    public void Simular_AltaSobreDatosConocidos()
    {
        Usuario nuevo = new Usuario();
        nuevo.Cuenta = "nuevo";
        nuevo.EMail = "nuevo@ubu.es";

        // Este tiene el email del tester, así que no se puede dar de alta
        Usuario duplicado = new Usuario();
        duplicado.Cuenta = "copia";
        duplicado.EMail = DatosPrueba.EmailTester;

        Usuario? guardado = _datos.GuardarUsuario(nuevo);
        Usuario? rechazado = _datos.GuardarUsuario(duplicado);

        Assert.IsNotNull(guardado);
        Assert.AreEqual(DatosPrueba.NumUsuarios + 1, guardado.Id);
        Assert.IsNull(rechazado);
    }

    // 10. Solo admin es administrador
    [TestMethod]
    public void SoloAdmin_EsAdministrador()
    {
        int administradores = 0;

        foreach (Usuario u in _datos.Usuarios.Values)
        {
            if (u.EsAdministrador)
            {
                administradores++;
            }
        }

        Assert.AreEqual(1, administradores);
        Assert.IsTrue(_datos.LeerUsuario(DatosPrueba.EmailAdmin)!.EsAdministrador);
    }

    // 11. El teléfono del administrador se guarda cifrado y se recupera con la clave
    [TestMethod]
    public void TelefonoAdmin_GuardadoCifrado()
    {
        Usuario admin = _datos.LeerUsuario(DatosPrueba.EmailAdmin)!;

        Assert.AreNotEqual(DatosPrueba.TelefonoAdmin, admin.TelefonoCifrado);
        Assert.DoesNotContain(DatosPrueba.TelefonoAdmin, admin.TelefonoCifrado);
        Assert.AreEqual(DatosPrueba.TelefonoAdmin, admin.ObtenerTelefono(_clave));
    }

    // 12. El teléfono del tester se guarda cifrado y se recupera con la clave
    [TestMethod]
    public void TelefonoTester_GuardadoCifrado()
    {
        Usuario tester = _datos.LeerUsuario(DatosPrueba.EmailTester)!;

        Assert.AreNotEqual(DatosPrueba.TelefonoTester, tester.TelefonoCifrado);
        Assert.DoesNotContain(DatosPrueba.TelefonoTester, tester.TelefonoCifrado);
        Assert.AreEqual(DatosPrueba.TelefonoTester, tester.ObtenerTelefono(_clave));
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nucleo.Modelos;
using Datos;

namespace Nucleo.Tests.Datos;

[TestClass]
public class DatosTests
{
    private global::Datos.Datos _datos = null!;

    // vaciar bd antes de cada test
    [TestInitialize]
    public void Inicializar()
    {
        _datos = new global::Datos.Datos();
        _datos.Reiniciar();
    }

    // 1. Añadimos nuevos usuarios
    [TestMethod]
    public void Usuario_GuardarNuevo()
    {
        Usuario u = new Usuario();
        u.Cuenta = "alex";
        u.Nombre = "Alejandro";
        u.EMail = "alex@ubu.es";

        Usuario? guardado = _datos.GuardarUsuario(u);

        Assert.IsNotNull(guardado);
        Assert.IsTrue(guardado.Id > 0);
        Assert.AreEqual("alex", guardado.Cuenta);
    }

    // 2. Añadimos nuevos roles
    [TestMethod]
    public void Rol_GuardarNuevo()
    {
        Rol r = new Rol { Id = -1, Nombre = "Administrador" };

        Rol? guardado = _datos.GuardarRol(r);

        Assert.IsNotNull(guardado);
        Assert.IsTrue(guardado.Id > 0);
        Assert.AreEqual("Administrador", guardado.Nombre);
    }

    // 3. Añadimos nuevos proyectos
    [TestMethod]
    public void Proyecto_GuardarNuevo()
    {
        Proyecto p = new Proyecto { Id = -1, Nombre = "Sistema Préstamos", Descripcion = "Proyecto bicicletas" };

        Proyecto? guardado = _datos.GuardarProyecto(p);

        Assert.IsNotNull(guardado);
        Assert.IsTrue(guardado.Id > 0);
        Assert.AreEqual("Sistema Préstamos", guardado.Nombre);
    }

    // 4. Guardamos y limpiamos logs
    [TestMethod]
    public void Log_GuardarYLimpiar()
    {
        EntradaLog log1 = new EntradaLog { UsuarioId = 1, Accion = "Inicio de sesión" };
        EntradaLog log2 = new EntradaLog { UsuarioId = 1, Accion = "Cambio de clave" };

        _datos.AgregarLog(log1);
        _datos.AgregarLog(log2);

        Assert.AreEqual(2, _datos.Logs.Count);

        int borrados = _datos.LimpiarLog();

        Assert.AreEqual(2, borrados);
        Assert.AreEqual(0, _datos.Logs.Count);
    }

    // 5. Asignamos IDs automáticos cuando Id es menor que cero
    [TestMethod]
    public void Id_Autoincremental()
    {
        Usuario u1 = new Usuario { Cuenta = "user1", Nombre = "Uno" };
        Usuario u2 = new Usuario { Cuenta = "user2", Nombre = "Dos" };

        _datos.GuardarUsuario(u1);
        _datos.GuardarUsuario(u2);

        // El segundo usuario tiene que recibir el ID siguiente
        Assert.AreEqual(1, u1.Id);
        Assert.AreEqual(2, u2.Id);
    }

    // 6. Buscamos por código y por email
    [TestMethod]
    public void Buscar_PorCodigo()
    {
        Usuario u = new Usuario { Cuenta = "pedro", Nombre = "Pedro", EMail = "pedro@ubu.es" };
        _datos.GuardarUsuario(u);

        Usuario? porId = _datos.LeerUsuario(u.Id);
        Usuario? porEmail = _datos.LeerUsuario("pedro@ubu.es");

        Assert.IsNotNull(porId);
        Assert.IsNotNull(porEmail);
        Assert.AreEqual("Pedro", porId.Nombre);
        Assert.AreEqual("pedro", porEmail.Cuenta);
    }

    // 7. Sacamos colecciones completas de datos
    [TestMethod]
    public void Colecciones_Completas()
    {
        _datos.GuardarUsuario(new Usuario { Cuenta = "u1" });
        _datos.GuardarRol(new Rol { Nombre = "Tester" });
        _datos.GuardarProyecto(new Proyecto { Nombre = "Web App" });

        Assert.AreEqual(1, _datos.Usuarios.Count);
        Assert.AreEqual(1, _datos.Roles.Count);
        Assert.AreEqual(1, _datos.Proyectos.Count);
    }

    // 8. Actualizamos datos que ya existen manteniendo su mismo Id
    [TestMethod]
    public void Actualizar_Existente()
    {
        Usuario u = new Usuario { Cuenta = "aitor", Nombre = "Aitor Original" };
        _datos.GuardarUsuario(u);

        // Cambiamos el nombre y guardamos
        u.Nombre = "Aitor Modificado";
        _datos.GuardarUsuario(u);

        Usuario? recuperado = _datos.LeerUsuario(u.Id);

        Assert.IsNotNull(recuperado);
        Assert.AreEqual("Aitor Modificado", recuperado.Nombre);
        // Debe seguir habiendo solo 1 usuario en la lista
        Assert.AreEqual(1, _datos.Usuarios.Count);
    }

    // 9. Borramos datos existentes
    [TestMethod]
    public void Eliminar_Datos()
    {
        Usuario u = _datos.GuardarUsuario(new Usuario { Cuenta = "borrar" })!;
        Rol r = _datos.GuardarRol(new Rol { Nombre = "RolBorrar" })!;
        Proyecto p = _datos.GuardarProyecto(new Proyecto { Nombre = "ProyBorrar" })!;

        Assert.IsNotNull(_datos.EliminarUsuario(u.Id));
        Assert.IsNotNull(_datos.EliminarRol(r.Id));
        Assert.IsNotNull(_datos.EliminarProyecto(p.Id));

        // Comprobamos que ya no están
        Assert.IsNull(_datos.LeerUsuario(u.Id));
        Assert.IsNull(_datos.LeerRol(r.Id));
        Assert.IsNull(_datos.LeerProyecto(p.Id));
    }

    // 10. Reiniciamos la base de datos entera
    [TestMethod]
    public void Reiniciar_LimpiaTodo()
    {
        _datos.GuardarUsuario(new Usuario { Cuenta = "u1" });
        _datos.GuardarRol(new Rol { Nombre = "Tester" });
        _datos.GuardarProyecto(new Proyecto { Nombre = "Web App" });
        _datos.AgregarLog(new EntradaLog { Accion = "Accion1" });

        // Vaciamos todo
        _datos.Reiniciar();

        Assert.AreEqual(0, _datos.Usuarios.Count);
        Assert.AreEqual(0, _datos.Roles.Count);
        Assert.AreEqual(0, _datos.Proyectos.Count);
        Assert.AreEqual(0, _datos.Logs.Count);
    }

    // 11. Buscamos cosas que no existen y debe devolver null
    [TestMethod]
    public void Buscar_Inexistente()
    {
        Usuario? porIdFalso = _datos.LeerUsuario(9999);
        Usuario? porEmailFalso = _datos.LeerUsuario("noexiste@ubu.es");
        Rol? rolFalso = _datos.LeerRol(9999);
        Proyecto? proyFalso = _datos.LeerProyecto(9999);

        Assert.IsNull(porIdFalso);
        Assert.IsNull(porEmailFalso);
        Assert.IsNull(rolFalso);
        Assert.IsNull(proyFalso);
    }

    // 12. Guardar con un código >= 0 que no existe no inserta nada y devuelve null
    [TestMethod]
    [DataRow(0)]
    [DataRow(5)]
    [DataRow(9999)]
    public void Guardar_CodigoInexistente_DevuelveNull(int id)
    {
        Usuario? usuario = _datos.GuardarUsuario(new Usuario { Id = id, Cuenta = "fantasma" });
        Rol? rol = _datos.GuardarRol(new Rol { Id = id, Nombre = "Fantasma" });
        Proyecto? proyecto = _datos.GuardarProyecto(new Proyecto { Id = id, Nombre = "Fantasma" });

        Assert.IsNull(usuario);
        Assert.IsNull(rol);
        Assert.IsNull(proyecto);
        Assert.AreEqual(0, _datos.Usuarios.Count);
        Assert.AreEqual(0, _datos.Roles.Count);
        Assert.AreEqual(0, _datos.Proyectos.Count);
    }

    // 13. Actualizar un rol y un proyecto existentes mantiene el Id y no duplica
    [TestMethod]
    public void Actualizar_RolYProyectoExistentes()
    {
        Rol r = _datos.GuardarRol(new Rol { Nombre = "Tester" })!;
        Proyecto p = _datos.GuardarProyecto(new Proyecto { Nombre = "Web App" })!;

        Rol? rolActualizado = _datos.GuardarRol(new Rol { Id = r.Id, Nombre = "Jefe de pruebas" });
        Proyecto? proyActualizado = _datos.GuardarProyecto(new Proyecto { Id = p.Id, Nombre = "Web App 2" });

        Assert.IsNotNull(rolActualizado);
        Assert.IsNotNull(proyActualizado);
        Assert.AreEqual("Jefe de pruebas", _datos.LeerRol(r.Id)!.Nombre);
        Assert.AreEqual("Web App 2", _datos.LeerProyecto(p.Id)!.Nombre);
        Assert.AreEqual(1, _datos.Roles.Count);
        Assert.AreEqual(1, _datos.Proyectos.Count);
    }

    // 14. Eliminar devuelve el mismo objeto que se ha borrado, con todos sus datos
    [TestMethod]
    public void Eliminar_DevuelveObjetoBorrado()
    {
        Usuario u = _datos.GuardarUsuario(new Usuario { Cuenta = "borrar", Nombre = "Borrar", EMail = "borrar@ubu.es" })!;
        Rol r = _datos.GuardarRol(new Rol { Nombre = "RolBorrar" })!;
        Proyecto p = _datos.GuardarProyecto(new Proyecto { Nombre = "ProyBorrar", Siglas = "PB" })!;

        Usuario? usuarioBorrado = _datos.EliminarUsuario(u.Id);
        Rol? rolBorrado = _datos.EliminarRol(r.Id);
        Proyecto? proyectoBorrado = _datos.EliminarProyecto(p.Id);

        Assert.AreSame(u, usuarioBorrado);
        Assert.AreSame(r, rolBorrado);
        Assert.AreSame(p, proyectoBorrado);
        Assert.AreEqual("borrar@ubu.es", usuarioBorrado!.EMail);
        Assert.AreEqual("RolBorrar", rolBorrado!.Nombre);
        Assert.AreEqual("PB", proyectoBorrado!.Siglas);
    }

    // 15. Eliminar algo que no existe devuelve null y no toca lo demás
    [TestMethod]
    [DataRow(-1)]
    [DataRow(0)]
    [DataRow(9999)]
    public void Eliminar_Inexistente_DevuelveNull(int id)
    {
        _datos.GuardarUsuario(new Usuario { Cuenta = "u1" });
        _datos.GuardarRol(new Rol { Nombre = "Tester" });
        _datos.GuardarProyecto(new Proyecto { Nombre = "Web App" });

        Assert.IsNull(_datos.EliminarUsuario(id));
        Assert.IsNull(_datos.EliminarRol(id));
        Assert.IsNull(_datos.EliminarProyecto(id));
        Assert.AreEqual(1, _datos.Usuarios.Count);
        Assert.AreEqual(1, _datos.Roles.Count);
        Assert.AreEqual(1, _datos.Proyectos.Count);
    }

    // 16. Borrar dos veces lo mismo: la segunda vez ya no existe
    [TestMethod]
    public void Eliminar_DosVeces_SegundaDevuelveNull()
    {
        Usuario u = _datos.GuardarUsuario(new Usuario { Cuenta = "doble" })!;

        Assert.IsNotNull(_datos.EliminarUsuario(u.Id));
        Assert.IsNull(_datos.EliminarUsuario(u.Id));
    }

    // 17. Las colecciones son copias: cambiarlas desde fuera no afecta a la base de datos
    [TestMethod]
    public void Colecciones_SonCopias_NoModificanDatos()
    {
        Usuario u = _datos.GuardarUsuario(new Usuario { Cuenta = "u1" })!;
        Rol r = _datos.GuardarRol(new Rol { Nombre = "Tester" })!;
        Proyecto p = _datos.GuardarProyecto(new Proyecto { Nombre = "Web App" })!;
        _datos.AgregarLog(new EntradaLog { Accion = "Accion1" });

        // Intentamos borrar y añadir saltándonos los métodos de Datos
        _datos.Usuarios.Remove(u.Id);
        _datos.Roles.Clear();
        _datos.Proyectos[999] = new Proyecto { Id = 999, Nombre = "Colado" };
        _datos.Logs.Clear();

        Assert.IsNotNull(_datos.LeerUsuario(u.Id));
        Assert.IsNotNull(_datos.LeerRol(r.Id));
        Assert.IsNull(_datos.LeerProyecto(999));
        Assert.IsNotNull(_datos.LeerProyecto(p.Id));
        Assert.AreEqual(1, _datos.Logs.Count);
    }

    // 18. Cada consulta devuelve una copia nueva con los datos actuales
    [TestMethod]
    public void Colecciones_ReflejanCambiosPosteriores()
    {
        var antes = _datos.Usuarios;

        _datos.GuardarUsuario(new Usuario { Cuenta = "u1" });

        Assert.AreEqual(0, antes.Count);
        Assert.AreEqual(1, _datos.Usuarios.Count);
        Assert.AreNotSame(_datos.Usuarios, _datos.Usuarios);
    }
}

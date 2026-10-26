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

    // 12. Guardar con código 0 (no existe) no inserta nada y devuelve null
    [TestMethod]
    public void Guardar_CodigoCero()
    {
        Usuario? usuario = _datos.GuardarUsuario(new Usuario { Id = 0, Cuenta = "fantasma" });
        Rol? rol = _datos.GuardarRol(new Rol { Id = 0, Nombre = "Fantasma" });
        Proyecto? proyecto = _datos.GuardarProyecto(new Proyecto { Id = 0, Nombre = "Fantasma" });

        Assert.IsNull(usuario);
        Assert.IsNull(rol);
        Assert.IsNull(proyecto);
        // No se ha insertado nada
        Assert.AreEqual(0, _datos.Usuarios.Count);
        Assert.AreEqual(0, _datos.Roles.Count);
        Assert.AreEqual(0, _datos.Proyectos.Count);
    }

    // 13. Guardar con código 5 (no existe) no inserta nada y devuelve null
    [TestMethod]
    public void Guardar_CodigoCinco()
    {
        Usuario? usuario = _datos.GuardarUsuario(new Usuario { Id = 5, Cuenta = "fantasma" });
        Rol? rol = _datos.GuardarRol(new Rol { Id = 5, Nombre = "Fantasma" });
        Proyecto? proyecto = _datos.GuardarProyecto(new Proyecto { Id = 5, Nombre = "Fantasma" });

        Assert.IsNull(usuario);
        Assert.IsNull(rol);
        Assert.IsNull(proyecto);
        // No se ha insertado nada
        Assert.AreEqual(0, _datos.Usuarios.Count);
        Assert.AreEqual(0, _datos.Roles.Count);
        Assert.AreEqual(0, _datos.Proyectos.Count);
    }

    // 14. Guardar con código 9999 (no existe) no inserta nada y devuelve null
    [TestMethod]
    public void Guardar_CodigoGrande()
    {
        Usuario? usuario = _datos.GuardarUsuario(new Usuario { Id = 9999, Cuenta = "fantasma" });
        Rol? rol = _datos.GuardarRol(new Rol { Id = 9999, Nombre = "Fantasma" });
        Proyecto? proyecto = _datos.GuardarProyecto(new Proyecto { Id = 9999, Nombre = "Fantasma" });

        Assert.IsNull(usuario);
        Assert.IsNull(rol);
        Assert.IsNull(proyecto);
        // No se ha insertado nada
        Assert.AreEqual(0, _datos.Usuarios.Count);
        Assert.AreEqual(0, _datos.Roles.Count);
        Assert.AreEqual(0, _datos.Proyectos.Count);
    }

    // 15. Actualizar un rol y un proyecto existentes mantiene el Id y no duplica
    [TestMethod]
    public void Actualizar_RolYProyecto()
    {
        Rol r = _datos.GuardarRol(new Rol { Nombre = "Tester" })!;
        Proyecto p = _datos.GuardarProyecto(new Proyecto { Nombre = "Web App" })!;

        // Guardamos otros objetos con el mismo Id y nombres nuevos
        Rol? rolActualizado = _datos.GuardarRol(new Rol { Id = r.Id, Nombre = "Jefe de pruebas" });
        Proyecto? proyActualizado = _datos.GuardarProyecto(new Proyecto { Id = p.Id, Nombre = "Web App 2" });

        Assert.IsNotNull(rolActualizado);
        Assert.IsNotNull(proyActualizado);

        Rol? rolRecuperado = _datos.LeerRol(r.Id);
        Proyecto? proyRecuperado = _datos.LeerProyecto(p.Id);

        Assert.IsNotNull(rolRecuperado);
        Assert.IsNotNull(proyRecuperado);
        Assert.AreEqual("Jefe de pruebas", rolRecuperado.Nombre);
        Assert.AreEqual("Web App 2", proyRecuperado.Nombre);
        // Debe seguir habiendo solo 1 rol y 1 proyecto
        Assert.AreEqual(1, _datos.Roles.Count);
        Assert.AreEqual(1, _datos.Proyectos.Count);
    }

    // 16. Eliminar devuelve el objeto que se ha borrado, con todos sus datos
    [TestMethod]
    public void Eliminar_DevuelveBorrado()
    {
        Usuario u = new Usuario();
        u.Cuenta = "borrar";
        u.Nombre = "Borrar";
        u.EMail = "borrar@ubu.es";
        _datos.GuardarUsuario(u);

        Rol r = new Rol();
        r.Nombre = "RolBorrar";
        _datos.GuardarRol(r);

        Proyecto p = new Proyecto();
        p.Nombre = "ProyBorrar";
        p.Siglas = "PB";
        _datos.GuardarProyecto(p);

        Usuario? usuarioBorrado = _datos.EliminarUsuario(u.Id);
        Rol? rolBorrado = _datos.EliminarRol(r.Id);
        Proyecto? proyectoBorrado = _datos.EliminarProyecto(p.Id);

        Assert.IsNotNull(usuarioBorrado);
        Assert.IsNotNull(rolBorrado);
        Assert.IsNotNull(proyectoBorrado);
        Assert.AreEqual(u.Id, usuarioBorrado.Id);
        Assert.AreEqual(r.Id, rolBorrado.Id);
        Assert.AreEqual(p.Id, proyectoBorrado.Id);
        Assert.AreEqual("borrar@ubu.es", usuarioBorrado.EMail);
        Assert.AreEqual("RolBorrar", rolBorrado.Nombre);
        Assert.AreEqual("PB", proyectoBorrado.Siglas);
    }

    // 17. Eliminar con código -1 devuelve null y no toca lo demás
    [TestMethod]
    public void Eliminar_CodigoNegativo()
    {
        _datos.GuardarUsuario(new Usuario { Cuenta = "u1" });
        _datos.GuardarRol(new Rol { Nombre = "Tester" });
        _datos.GuardarProyecto(new Proyecto { Nombre = "Web App" });

        Assert.IsNull(_datos.EliminarUsuario(-1));
        Assert.IsNull(_datos.EliminarRol(-1));
        Assert.IsNull(_datos.EliminarProyecto(-1));
        // Lo que había sigue ahí
        Assert.AreEqual(1, _datos.Usuarios.Count);
        Assert.AreEqual(1, _datos.Roles.Count);
        Assert.AreEqual(1, _datos.Proyectos.Count);
    }

    // 18. Eliminar con código 0 devuelve null y no toca lo demás
    [TestMethod]
    public void Eliminar_CodigoCero()
    {
        _datos.GuardarUsuario(new Usuario { Cuenta = "u1" });
        _datos.GuardarRol(new Rol { Nombre = "Tester" });
        _datos.GuardarProyecto(new Proyecto { Nombre = "Web App" });

        Assert.IsNull(_datos.EliminarUsuario(0));
        Assert.IsNull(_datos.EliminarRol(0));
        Assert.IsNull(_datos.EliminarProyecto(0));
        // Lo que había sigue ahí
        Assert.AreEqual(1, _datos.Usuarios.Count);
        Assert.AreEqual(1, _datos.Roles.Count);
        Assert.AreEqual(1, _datos.Proyectos.Count);
    }

    // 19. Eliminar con código 9999 (no existe) devuelve null y no toca lo demás
    [TestMethod]
    public void Eliminar_CodigoGrande()
    {
        _datos.GuardarUsuario(new Usuario { Cuenta = "u1" });
        _datos.GuardarRol(new Rol { Nombre = "Tester" });
        _datos.GuardarProyecto(new Proyecto { Nombre = "Web App" });

        Assert.IsNull(_datos.EliminarUsuario(9999));
        Assert.IsNull(_datos.EliminarRol(9999));
        Assert.IsNull(_datos.EliminarProyecto(9999));
        // Lo que había sigue ahí
        Assert.AreEqual(1, _datos.Usuarios.Count);
        Assert.AreEqual(1, _datos.Roles.Count);
        Assert.AreEqual(1, _datos.Proyectos.Count);
    }

    // 20. Borrar dos veces lo mismo: la segunda vez ya no existe
    [TestMethod]
    public void Eliminar_DosVeces()
    {
        Usuario u = _datos.GuardarUsuario(new Usuario { Cuenta = "doble" })!;

        Usuario? primera = _datos.EliminarUsuario(u.Id);
        Usuario? segunda = _datos.EliminarUsuario(u.Id);

        Assert.IsNotNull(primera);
        Assert.IsNull(segunda);
    }

    // 21. Las colecciones son copias: cambiarlas desde fuera no afecta a la base de datos
    [TestMethod]
    public void Colecciones_SonCopias()
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

        // La base de datos no ha cambiado
        Assert.IsNotNull(_datos.LeerUsuario(u.Id));
        Assert.IsNotNull(_datos.LeerRol(r.Id));
        Assert.IsNull(_datos.LeerProyecto(999));
        Assert.IsNotNull(_datos.LeerProyecto(p.Id));
        Assert.AreEqual(1, _datos.Logs.Count);
    }

    // 22. Cada consulta devuelve una copia nueva con los datos actuales
    [TestMethod]
    public void Colecciones_ReflejanCambios()
    {
        Dictionary<int, Usuario> antes = _datos.Usuarios;

        _datos.GuardarUsuario(new Usuario { Cuenta = "u1" });

        Dictionary<int, Usuario> despues = _datos.Usuarios;

        // La copia de antes no cambia y la nueva ya trae el usuario
        Assert.AreEqual(0, antes.Count);
        Assert.AreEqual(1, despues.Count);
    }

    // 23. No se puede dar de alta un usuario con un email que ya existe
    [TestMethod]
    public void Usuario_EmailDuplicado()
    {
        _datos.GuardarUsuario(new Usuario { Cuenta = "pedro", EMail = "pedro@ubu.es" });

        Usuario? duplicado = _datos.GuardarUsuario(new Usuario { Cuenta = "otro", EMail = "pedro@ubu.es" });

        Assert.IsNull(duplicado);
        Assert.AreEqual(1, _datos.Usuarios.Count);
    }

    // 24. Tampoco si el email repetido está en mayúsculas
    [TestMethod]
    public void Usuario_EmailDuplicadoMayusculas()
    {
        _datos.GuardarUsuario(new Usuario { Cuenta = "pedro", EMail = "pedro@ubu.es" });

        Usuario? duplicado = _datos.GuardarUsuario(new Usuario { Cuenta = "otro", EMail = "PEDRO@UBU.ES" });

        Assert.IsNull(duplicado);
        Assert.AreEqual(1, _datos.Usuarios.Count);
    }

    // 25. Tampoco si el email repetido mezcla mayúsculas y minúsculas
    [TestMethod]
    public void Usuario_EmailDuplicadoMezclado()
    {
        _datos.GuardarUsuario(new Usuario { Cuenta = "pedro", EMail = "pedro@ubu.es" });

        Usuario? duplicado = _datos.GuardarUsuario(new Usuario { Cuenta = "otro", EMail = "Pedro@Ubu.Es" });

        Assert.IsNull(duplicado);
        Assert.AreEqual(1, _datos.Usuarios.Count);
    }

    // 26. Tampoco se puede cambiar el email de un usuario por el de otro
    [TestMethod]
    public void Usuario_CambiarEmailRepetido()
    {
        _datos.GuardarUsuario(new Usuario { Cuenta = "pedro", EMail = "pedro@ubu.es" });
        Usuario ana = _datos.GuardarUsuario(new Usuario { Cuenta = "ana", EMail = "ana@ubu.es" })!;

        // Ana intenta quedarse con el email de Pedro
        ana.EMail = "pedro@ubu.es";
        Usuario? resultado = _datos.GuardarUsuario(ana);

        Assert.IsNull(resultado);
    }

    // 27. Un usuario puede volver a guardarse con su propio email, y un alta rechazada no gasta Id
    [TestMethod]
    public void Usuario_GuardarConSuEmail()
    {
        Usuario pedro = _datos.GuardarUsuario(new Usuario { Cuenta = "pedro", EMail = "pedro@ubu.es" })!;

        // Este alta se rechaza por email repetido
        _datos.GuardarUsuario(new Usuario { Cuenta = "copia", EMail = "pedro@ubu.es" });

        // Pedro se guarda otra vez con su mismo email
        pedro.Nombre = "Pedro";
        Usuario? actualizado = _datos.GuardarUsuario(pedro);

        // El siguiente alta recibe el Id 2, no el 3
        Usuario ana = _datos.GuardarUsuario(new Usuario { Cuenta = "ana", EMail = "ana@ubu.es" })!;

        Assert.IsNotNull(actualizado);
        Assert.AreEqual(2, ana.Id);
    }

    // 28. Buscar por email no distingue mayúsculas
    [TestMethod]
    public void Buscar_PorEmailMayusculas()
    {
        _datos.GuardarUsuario(new Usuario { Cuenta = "pedro", EMail = "pedro@ubu.es" });

        Usuario? encontrado = _datos.LeerUsuario("PEDRO@ubu.ES");

        Assert.IsNotNull(encontrado);
    }

    // 29. Al borrar un usuario desaparece de los proyectos donde estaba asignado
    [TestMethod]
    public void EliminarUsuario_QuitaDeProyectos()
    {
        Usuario pedro = _datos.GuardarUsuario(new Usuario { Cuenta = "pedro" })!;
        Usuario ana = _datos.GuardarUsuario(new Usuario { Cuenta = "ana" })!;

        Proyecto p = new Proyecto();
        p.Nombre = "Web App";
        p.UsuariosAsignados = new List<int> { pedro.Id, ana.Id };
        _datos.GuardarProyecto(p);

        _datos.EliminarUsuario(pedro.Id);

        Proyecto? recuperado = _datos.LeerProyecto(p.Id);

        Assert.IsNotNull(recuperado);
        // Solo queda Ana asignada
        CollectionAssert.AreEqual(new List<int> { ana.Id }, recuperado.UsuariosAsignados);
    }

    // 30. Al borrar un rol, los usuarios que lo tenían se quedan sin rol (0)
    [TestMethod]
    public void EliminarRol_UsuariosSinRol()
    {
        Rol tester = _datos.GuardarRol(new Rol { Nombre = "Tester" })!;
        Rol jefe = _datos.GuardarRol(new Rol { Nombre = "Jefe" })!;
        Usuario pedro = _datos.GuardarUsuario(new Usuario { Cuenta = "pedro", RolId = tester.Id })!;
        Usuario ana = _datos.GuardarUsuario(new Usuario { Cuenta = "ana", RolId = jefe.Id })!;

        _datos.EliminarRol(tester.Id);

        Usuario? pedroRecuperado = _datos.LeerUsuario(pedro.Id);
        Usuario? anaRecuperada = _datos.LeerUsuario(ana.Id);

        Assert.IsNotNull(pedroRecuperado);
        Assert.IsNotNull(anaRecuperada);
        // Pedro se queda sin rol y Ana mantiene el suyo
        Assert.AreEqual(0, pedroRecuperado.RolId);
        Assert.AreEqual(jefe.Id, anaRecuperada.RolId);
    }

    // 31. Al borrar un usuario se mantienen sus logs (auditoría)
    [TestMethod]
    public void EliminarUsuario_MantieneLogs()
    {
        Usuario pedro = _datos.GuardarUsuario(new Usuario { Cuenta = "pedro" })!;
        _datos.AgregarLog(new EntradaLog { UsuarioId = pedro.Id, Accion = "Inicio de sesión" });

        _datos.EliminarUsuario(pedro.Id);

        Assert.AreEqual(1, _datos.Logs.Count);
    }

    // 32. Guardar null no rompe y devuelve null
    [TestMethod]
    public void Guardar_Nulo()
    {
        Usuario? usuario = _datos.GuardarUsuario(null!);
        Rol? rol = _datos.GuardarRol(null!);
        Proyecto? proyecto = _datos.GuardarProyecto(null!);

        Assert.IsNull(usuario);
        Assert.IsNull(rol);
        Assert.IsNull(proyecto);
        Assert.AreEqual(0, _datos.Usuarios.Count);
        Assert.AreEqual(0, _datos.Roles.Count);
        Assert.AreEqual(0, _datos.Proyectos.Count);
    }

    // 33. Agregar un log nulo lanza ArgumentNullException y no añade nada
    [TestMethod]
    public void AgregarLog_Nulo()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => _datos.AgregarLog(null!));
        Assert.AreEqual(0, _datos.Logs.Count);
    }

    // 34. Buscar por email nulo devuelve null
    [TestMethod]
    public void Buscar_EmailNulo()
    {
        _datos.GuardarUsuario(new Usuario { Cuenta = "sinEmail" });
        _datos.GuardarUsuario(new Usuario { Cuenta = "pedro", EMail = "pedro@ubu.es" });

        Usuario? encontrado = _datos.LeerUsuario(null!);

        Assert.IsNull(encontrado);
    }

    // 35. Buscar por email vacío devuelve null
    [TestMethod]
    public void Buscar_EmailVacio()
    {
        // Un usuario sin email no debe salir al buscar por ""
        _datos.GuardarUsuario(new Usuario { Cuenta = "sinEmail" });
        _datos.GuardarUsuario(new Usuario { Cuenta = "pedro", EMail = "pedro@ubu.es" });

        Usuario? encontrado = _datos.LeerUsuario("");

        Assert.IsNull(encontrado);
    }

    // 36. Buscar por email con solo espacios devuelve null
    [TestMethod]
    public void Buscar_EmailEspacios()
    {
        _datos.GuardarUsuario(new Usuario { Cuenta = "sinEmail" });
        _datos.GuardarUsuario(new Usuario { Cuenta = "pedro", EMail = "pedro@ubu.es" });

        Usuario? encontrado = _datos.LeerUsuario("   ");

        Assert.IsNull(encontrado);
    }

    // 37. Buscar por código -1 devuelve null
    [TestMethod]
    public void Buscar_CodigoNegativo()
    {
        _datos.GuardarUsuario(new Usuario { Cuenta = "u1" });
        _datos.GuardarRol(new Rol { Nombre = "Tester" });
        _datos.GuardarProyecto(new Proyecto { Nombre = "Web App" });

        Assert.IsNull(_datos.LeerUsuario(-1));
        Assert.IsNull(_datos.LeerRol(-1));
        Assert.IsNull(_datos.LeerProyecto(-1));
    }

    // 38. Buscar por código 0 devuelve null
    [TestMethod]
    public void Buscar_CodigoCero()
    {
        _datos.GuardarUsuario(new Usuario { Cuenta = "u1" });
        _datos.GuardarRol(new Rol { Nombre = "Tester" });
        _datos.GuardarProyecto(new Proyecto { Nombre = "Web App" });

        Assert.IsNull(_datos.LeerUsuario(0));
        Assert.IsNull(_datos.LeerRol(0));
        Assert.IsNull(_datos.LeerProyecto(0));
    }

    // 39. Buscar por el código más pequeño posible devuelve null
    [TestMethod]
    public void Buscar_CodigoMinimo()
    {
        _datos.GuardarUsuario(new Usuario { Cuenta = "u1" });
        _datos.GuardarRol(new Rol { Nombre = "Tester" });
        _datos.GuardarProyecto(new Proyecto { Nombre = "Web App" });

        Assert.IsNull(_datos.LeerUsuario(int.MinValue));
        Assert.IsNull(_datos.LeerRol(int.MinValue));
        Assert.IsNull(_datos.LeerProyecto(int.MinValue));
    }

    // 40. Limpiar el log cuando está vacío devuelve 0
    [TestMethod]
    public void LimpiarLog_Vacio()
    {
        int borrados = _datos.LimpiarLog();

        Assert.AreEqual(0, borrados);
    }

    // 41. Después de Reiniciar los Ids vuelven a empezar en 1
    [TestMethod]
    public void Reiniciar_IdsEmpiezanDeNuevo()
    {
        _datos.GuardarUsuario(new Usuario { Cuenta = "u1" });
        _datos.GuardarUsuario(new Usuario { Cuenta = "u2" });
        _datos.GuardarRol(new Rol { Nombre = "Tester" });
        _datos.GuardarProyecto(new Proyecto { Nombre = "Web App" });

        // Vaciamos todo
        _datos.Reiniciar();

        // Damos de alta otra vez
        Usuario u = _datos.GuardarUsuario(new Usuario { Cuenta = "nuevo" })!;
        Rol r = _datos.GuardarRol(new Rol { Nombre = "Nuevo" })!;
        Proyecto p = _datos.GuardarProyecto(new Proyecto { Nombre = "Nuevo" })!;

        Assert.AreEqual(1, u.Id);
        Assert.AreEqual(1, r.Id);
        Assert.AreEqual(1, p.Id);
    }

    // 42. Un Id borrado no se reutiliza en el siguiente alta
    [TestMethod]
    public void Eliminar_NoReutilizaIds()
    {
        Usuario u1 = _datos.GuardarUsuario(new Usuario { Cuenta = "u1" })!;
        _datos.EliminarUsuario(u1.Id);

        Usuario u2 = _datos.GuardarUsuario(new Usuario { Cuenta = "u2" })!;

        Assert.AreNotEqual(u1.Id, u2.Id);
    }

    // 43. Los logs reciben Ids consecutivos
    [TestMethod]
    public void AgregarLog_IdsConsecutivos()
    {
        EntradaLog log1 = new EntradaLog();
        log1.Accion = "Accion1";
        EntradaLog log2 = new EntradaLog();
        log2.Accion = "Accion2";

        _datos.AgregarLog(log1);
        _datos.AgregarLog(log2);

        Assert.AreEqual(1, log1.Id);
        Assert.AreEqual(2, log2.Id);
    }
}

using Nucleo.Modelos;
using System;
using System.Collections.Generic;

namespace Datos;

// Crea la base de datos en un estado conocido con datos conocidos (PR1 p.1).
// La usan la web al arrancar y las pruebas, así siempre se parte de lo mismo.
public static class DatosPrueba
{
    // Contraseñas de los usuarios de prueba (todas cumplen ServicioContrasenas)
    public const string EmailAdmin = "admin@ubu.es";
    public const string ClaveAdmin = "Password123!";
    public const string EmailTester = "tester@ubu.es";
    public const string ClaveTester = "Tester123!";
    public const string EmailInvitado = "invitado@ubu.es";
    public const string ClaveInvitado = "Invitado123!";
    public const string EmailBaja = "baja@ubu.es";
    public const string ClaveBaja = "Baja1234!";

    public const int NumUsuarios = 4;
    public const int NumRoles = 3;
    public const int NumProyectos = 3;
    public const int NumLogs = 2;

    // Devuelve una base de datos nueva ya cargada
    public static Datos Crear()
    {
        Datos datos = new Datos();
        Cargar(datos);
        return datos;
    }

    // Vacía la base de datos y la deja en el estado conocido
    public static void Cargar(IDatos datos)
    {
        datos.Reiniciar();

        // Roles: Ids 1, 2 y 3
        Rol administrador = datos.GuardarRol(new Rol
        {
            Nombre = "Administrador",
            Permisos = new List<string> { "Proyectos", "Usuarios", "Log", "Perfil" }
        })!;
        Rol tester = datos.GuardarRol(new Rol
        {
            Nombre = "Tester",
            Permisos = new List<string> { "Proyectos", "Perfil" }
        })!;
        Rol invitado = datos.GuardarRol(new Rol
        {
            Nombre = "Invitado",
            Permisos = new List<string> { "Perfil" }
        })!;

        // Usuarios: Ids 1 a 4
        Usuario admin = CrearUsuario(datos, "admin", "Alex", "Alumno", EmailAdmin, ClaveAdmin, administrador.Id, true);
        Usuario usuarioTester = CrearUsuario(datos, "tester", "Tere", "Tester", EmailTester, ClaveTester, tester.Id, true);
        CrearUsuario(datos, "invitado", "Iván", "Invitado", EmailInvitado, ClaveInvitado, invitado.Id, true);
        CrearUsuario(datos, "baja", "Bea", "Baja", EmailBaja, ClaveBaja, tester.Id, false);

        // Proyectos: Ids 1, 2 y 3 (el último inactivo)
        datos.GuardarProyecto(new Proyecto
        {
            Nombre = "Plataforma UBU TPMP",
            Siglas = "UBU-TPMP",
            Descripcion = "Proyecto de gestión de pruebas y validación.",
            Activo = true,
            UsuariosAsignados = new List<int> { admin.Id, usuarioTester.Id }
        });
        datos.GuardarProyecto(new Proyecto
        {
            Nombre = "Actividades físicas",
            Siglas = "ACTFIS",
            Descripcion = "Registro de actividades físicas de los usuarios.",
            Activo = true,
            UsuariosAsignados = new List<int> { usuarioTester.Id }
        });
        datos.GuardarProyecto(new Proyecto
        {
            Nombre = "Proyecto archivado",
            Siglas = "ARCH",
            Descripcion = "Proyecto antiguo que ya no está activo.",
            Activo = false,
            UsuariosAsignados = new List<int> { admin.Id }
        });

        // Logs: Ids 1 y 2
        datos.AgregarLog(new EntradaLog(new DateTime(2026, 10, 1, 9, 0, 0), "Info", "Carga de datos de prueba", "sistema")
        {
            Accion = "Carga de datos"
        });
        datos.AgregarLog(new EntradaLog(new DateTime(2026, 10, 1, 9, 5, 0), "Aviso", "Intento de acceso fallido", EmailBaja)
        {
            Accion = "Login fallido"
        });
    }

    private static Usuario CrearUsuario(
        IDatos datos,
        string cuenta,
        string nombre,
        string apellidos,
        string email,
        string clave,
        int rolId,
        bool activo)
    {
        Usuario usuario = new Usuario
        {
            Cuenta = cuenta,
            Nombre = nombre,
            Apellidos = apellidos,
            EMail = email,
            RolId = rolId,
            Activo = activo
        };
        usuario.EstablecerContrasena(clave);

        return datos.GuardarUsuario(usuario)!;
    }
}

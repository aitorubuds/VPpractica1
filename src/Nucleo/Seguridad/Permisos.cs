using Nucleo.Modelos;
using System;

namespace Nucleo.Seguridad;

// Permisos por área y por proyecto según el rol del usuario
public static class Permisos
{
    // Áreas de la aplicación
    public const string AreaProyectos = "Proyectos";
    public const string AreaUsuarios = "Usuarios";
    public const string AreaLog = "Log";
    public const string AreaPerfil = "Perfil";

    // El administrador entra en todo y los demás solo donde les deje su rol
    public static bool TieneAcceso(Usuario? usuario, Rol? rol, string area)
    {
        // Sin usuario, inactivo o bloqueado no entra
        if (usuario == null || !usuario.Activo || usuario.Bloqueado)
        {
            return false;
        }

        if (usuario.EsAdministrador)
        {
            return true;
        }

        // El rol tiene que existir y ser el suyo
        if (rol == null || rol.Id != usuario.RolId)
        {
            return false;
        }

        // Miramos si el área está entre los permisos del rol (sin mirar mayúsculas)
        foreach (string permiso in rol.Permisos)
        {
            if (string.Equals(permiso, area, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    // Un usuario solo ve los proyectos a los que está asignado
    public static bool PuedeVerProyecto(Usuario? usuario, Proyecto? proyecto)
    {
        if (usuario == null || proyecto == null)
        {
            return false;
        }

        // El administrador ve todos los proyectos
        if (usuario.EsAdministrador)
        {
            return true;
        }

        return proyecto.UsuariosAsignados.Contains(usuario.Id);
    }

    // Solo el administrador crea proyectos
    public static bool PuedeCrearProyectos(Usuario? usuario)
    {
        if (usuario == null)
        {
            return false;
        }

        // Tiene que ser administrador, estar activo y no estar bloqueado
        if (usuario.EsAdministrador && usuario.Activo && !usuario.Bloqueado)
        {
            return true;
        }

        return false;
    }
}

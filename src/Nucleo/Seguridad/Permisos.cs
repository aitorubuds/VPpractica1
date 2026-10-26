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

    // El administrador entra en todo; el resto según los permisos de su rol
    public static bool TieneAcceso(Usuario? usuario, Rol? rol, string area)
    {
        if (usuario is null || !usuario.Activo || usuario.Bloqueado)
        {
            return false;
        }

        if (usuario.EsAdministrador)
        {
            return true;
        }

        if (rol is null || rol.Id != usuario.RolId)
        {
            return false;
        }

        return rol.Permisos.Contains(area, StringComparer.OrdinalIgnoreCase);
    }

    // Un usuario solo ve los proyectos a los que está asignado
    public static bool PuedeVerProyecto(Usuario? usuario, Proyecto? proyecto)
    {
        if (usuario is null || proyecto is null)
        {
            return false;
        }

        return usuario.EsAdministrador || proyecto.UsuariosAsignados.Contains(usuario.Id);
    }

    // Solo el administrador crea proyectos
    public static bool PuedeCrearProyectos(Usuario? usuario)
    {
        return usuario is not null && usuario.EsAdministrador && usuario.Activo && !usuario.Bloqueado;
    }
}

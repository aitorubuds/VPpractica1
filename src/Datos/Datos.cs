using Nucleo.Modelos;
using System;
using System.Collections.Generic;

namespace Datos;

public class Datos : IDatos
{
    private readonly Dictionary<int, Usuario> _usuarios = new();
    private readonly Dictionary<int, Rol> _roles = new();
    private readonly Dictionary<int, Proyecto> _proyectos = new();
    private readonly List<EntradaLog> _logs = new();
    private int _ultimoIdUsuario = 0;
    private int _ultimoIdRol = 0;
    private int _ultimoIdProyecto = 0;

    // Se devuelven copias para que desde fuera no se pueda tocar el almacenamiento interno
    public Dictionary<int, Usuario> Usuarios => new(_usuarios);
    public Dictionary<int, Rol> Roles => new(_roles);
    public Dictionary<int, Proyecto> Proyectos => new(_proyectos);
    public List<EntradaLog> Logs => new(_logs);

    public Usuario? GuardarUsuario(Usuario usuario)
    {
        // Si el código no existe no se puede actualizar
        if (usuario is null || (usuario.Id >= 0 && !_usuarios.ContainsKey(usuario.Id)))
        {
            return null;
        }

        // El email no se puede repetir entre usuarios distintos
        if (EmailEnUso(usuario.EMail, usuario.Id))
        {
            return null;
        }

        if (usuario.Id < 0)
        {
            usuario.Id = ++_ultimoIdUsuario;
        }

        _usuarios[usuario.Id] = usuario;
        return usuario;
    }

    public Usuario? LeerUsuario(int idUsuario)
    {
        return _usuarios.GetValueOrDefault(idUsuario);
    }

    public Usuario? LeerUsuario(string eMail)
    {
        // Sin email no se busca: si no, "" encontraría a los usuarios sin email
        if (string.IsNullOrWhiteSpace(eMail))
        {
            return null;
        }

        // El email no distingue mayúsculas
        return _usuarios.Values.FirstOrDefault(
            u => string.Equals(u.EMail, eMail, StringComparison.OrdinalIgnoreCase));
    }

    public Usuario? EliminarUsuario(int idUsuario)
    {
        // Devuelve el usuario borrado o null si no existía
        if (!_usuarios.Remove(idUsuario, out Usuario? borrado))
        {
            return null;
        }

        // Lo quitamos también de los proyectos donde estaba asignado
        foreach (Proyecto proyecto in _proyectos.Values)
        {
            proyecto.UsuariosAsignados.Remove(idUsuario);
        }

        // Los logs no se borran porque son el registro de auditoría
        return borrado;
    }

    private bool EmailEnUso(string eMail, int idUsuario)
    {
        Usuario? otro = LeerUsuario(eMail);
        return otro is not null && otro.Id != idUsuario;
    }

    public Rol? GuardarRol(Rol rol)
    {
        if (rol is null)
        {
            return null;
        }

        if (rol.Id < 0)
        {
            rol.Id = ++_ultimoIdRol;
        }
        else if (!_roles.ContainsKey(rol.Id))
        {
            return null;
        }

        _roles[rol.Id] = rol;
        return rol;
    }

    public Rol? LeerRol(int idRol)
    {
        return _roles.GetValueOrDefault(idRol);
    }

    public Rol? EliminarRol(int idRol)
    {
        if (!_roles.Remove(idRol, out Rol? borrado))
        {
            return null;
        }

        // Los usuarios que tenían ese rol se quedan sin rol (0)
        foreach (Usuario usuario in _usuarios.Values)
        {
            if (usuario.RolId == idRol)
            {
                usuario.RolId = 0;
            }
        }

        return borrado;
    }

    public Proyecto? GuardarProyecto(Proyecto proyecto)
    {
        if (proyecto is null)
        {
            return null;
        }

        if (proyecto.Id < 0)
        {
            proyecto.Id = ++_ultimoIdProyecto;
        }
        else if (!_proyectos.ContainsKey(proyecto.Id))
        {
            return null;
        }

        _proyectos[proyecto.Id] = proyecto;
        return proyecto;
    }

    public Proyecto? LeerProyecto(int idProyecto)
    {
        return _proyectos.GetValueOrDefault(idProyecto);
    }

    public Proyecto? EliminarProyecto(int idProyecto)
    {
        _proyectos.Remove(idProyecto, out Proyecto? borrado);
        return borrado;
    }

    public void AgregarLog(EntradaLog log)
    {
        // Un log nulo es un error de programación, no se puede ignorar sin más
        ArgumentNullException.ThrowIfNull(log);

        log.Id = _logs.Count + 1;
        _logs.Add(log);
    }

    public int LimpiarLog()
    {
        int count = _logs.Count;
        _logs.Clear();
        return count;
    }

    public void Reiniciar()
    {
        _usuarios.Clear();
        _roles.Clear();
        _proyectos.Clear();
        _logs.Clear();
        _ultimoIdUsuario = 0;
        _ultimoIdRol = 0;
        _ultimoIdProyecto = 0;
    }
}

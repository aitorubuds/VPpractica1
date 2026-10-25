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

    public Dictionary<int, Usuario> Usuarios => _usuarios;
    public Dictionary<int, Rol> Roles => _roles;
    public Dictionary<int, Proyecto> Proyectos => _proyectos;
    public List<EntradaLog> Logs => _logs;

    public Usuario? GuardarUsuario(Usuario usuario)
    {
        if (usuario.Id < 0)
        {
            usuario.Id = ++_ultimoIdUsuario;
        }
        else if (!_usuarios.ContainsKey(usuario.Id))
        {
            // Si el código no existe no se puede actualizar
            return null;
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
        return _usuarios.Values.FirstOrDefault(u => u.EMail == eMail);
    }

    public Usuario? EliminarUsuario(int idUsuario)
    {
        if (_usuarios.ContainsKey(idUsuario))
        {
            _usuarios.Remove(idUsuario);
            return new Usuario { Id = idUsuario };
        }
        return null;
    }

    public Rol? GuardarRol(Rol rol)
    {
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
        if (_roles.ContainsKey(idRol))
        {
            _roles.Remove(idRol);
            return new Rol { Id = idRol };
        }
        return null;
    }

    public Proyecto? GuardarProyecto(Proyecto proyecto)
    {
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
        if (_proyectos.ContainsKey(idProyecto))
        {
            _proyectos.Remove(idProyecto);
            return new Proyecto { Id = idProyecto };
        }
        return null;
    }

    public void AgregarLog(EntradaLog log)
    {
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

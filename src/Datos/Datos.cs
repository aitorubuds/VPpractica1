using Nucleo.Modelos;
using System;
using System.Collections.Generic;

namespace Datos;

public class Datos : IDatos
{
    private readonly Dictionary<int, Usuario> _usuarios = new Dictionary<int, Usuario>();
    private readonly Dictionary<int, Rol> _roles = new Dictionary<int, Rol>();
    private readonly Dictionary<int, Proyecto> _proyectos = new Dictionary<int, Proyecto>();
    private readonly List<EntradaLog> _logs = new List<EntradaLog>();
    private int _ultimoIdUsuario = 0;
    private int _ultimoIdRol = 0;
    private int _ultimoIdProyecto = 0;

    // Devolvemos copias para que desde fuera no puedan cambiar los datos
    public Dictionary<int, Usuario> Usuarios
    {
        get { return new Dictionary<int, Usuario>(_usuarios); }
    }

    public Dictionary<int, Rol> Roles
    {
        get { return new Dictionary<int, Rol>(_roles); }
    }

    public Dictionary<int, Proyecto> Proyectos
    {
        get { return new Dictionary<int, Proyecto>(_proyectos); }
    }

    public List<EntradaLog> Logs
    {
        get { return new List<EntradaLog>(_logs); }
    }

    // Guardar un usuario nuevo o actualizar uno que ya existe
    public Usuario? GuardarUsuario(Usuario usuario)
    {
        // Sin usuario no hay nada que guardar
        if (usuario == null)
        {
            return null;
        }

        // Si el código no existe no se puede actualizar
        if (usuario.Id >= 0 && !_usuarios.ContainsKey(usuario.Id))
        {
            return null;
        }

        // El email no se puede repetir entre usuarios distintos
        if (EmailEnUso(usuario.EMail, usuario.Id))
        {
            return null;
        }

        // Si es nuevo le damos el siguiente Id
        if (usuario.Id < 0)
        {
            _ultimoIdUsuario++;
            usuario.Id = _ultimoIdUsuario;
        }

        _usuarios[usuario.Id] = usuario;
        return usuario;
    }

    // Buscar un usuario por su código
    public Usuario? LeerUsuario(int idUsuario)
    {
        if (_usuarios.ContainsKey(idUsuario))
        {
            return _usuarios[idUsuario];
        }

        return null;
    }

    // Buscar un usuario por su email
    public Usuario? LeerUsuario(string eMail)
    {
        // Si el email está vacío no buscamos, porque si no saldrían los usuarios sin email
        if (string.IsNullOrWhiteSpace(eMail))
        {
            return null;
        }

        // El email no distingue mayúsculas
        foreach (Usuario u in _usuarios.Values)
        {
            if (string.Equals(u.EMail, eMail, StringComparison.OrdinalIgnoreCase))
            {
                return u;
            }
        }

        return null;
    }

    // Borrar un usuario y devolverlo, o null si no existía
    public Usuario? EliminarUsuario(int idUsuario)
    {
        if (!_usuarios.ContainsKey(idUsuario))
        {
            return null;
        }

        Usuario borrado = _usuarios[idUsuario];
        _usuarios.Remove(idUsuario);

        // Lo quitamos también de los proyectos donde estaba asignado
        foreach (Proyecto proyecto in _proyectos.Values)
        {
            proyecto.UsuariosAsignados.Remove(idUsuario);
        }

        // Los logs no se borran porque son el registro de auditoría
        return borrado;
    }

    // Comprobar si otro usuario distinto ya tiene ese email
    private bool EmailEnUso(string eMail, int idUsuario)
    {
        Usuario? otro = LeerUsuario(eMail);

        if (otro == null)
        {
            return false;
        }

        return otro.Id != idUsuario;
    }

    // Guardar un rol nuevo o actualizar uno que ya existe
    public Rol? GuardarRol(Rol rol)
    {
        if (rol == null)
        {
            return null;
        }

        if (rol.Id < 0)
        {
            // Si es nuevo le damos el siguiente Id
            _ultimoIdRol++;
            rol.Id = _ultimoIdRol;
        }
        else if (!_roles.ContainsKey(rol.Id))
        {
            // Si el código no existe no se puede actualizar
            return null;
        }

        _roles[rol.Id] = rol;
        return rol;
    }

    // Buscar un rol por su código
    public Rol? LeerRol(int idRol)
    {
        if (_roles.ContainsKey(idRol))
        {
            return _roles[idRol];
        }

        return null;
    }

    // Borrar un rol y devolverlo, o null si no existía
    public Rol? EliminarRol(int idRol)
    {
        if (!_roles.ContainsKey(idRol))
        {
            return null;
        }

        Rol borrado = _roles[idRol];
        _roles.Remove(idRol);

        // Los usuarios que tenían ese rol se quedan sin rol, con el 0
        foreach (Usuario usuario in _usuarios.Values)
        {
            if (usuario.RolId == idRol)
            {
                usuario.RolId = 0;
            }
        }

        return borrado;
    }

    // Guardar un proyecto nuevo o actualizar uno que ya existe
    public Proyecto? GuardarProyecto(Proyecto proyecto)
    {
        if (proyecto == null)
        {
            return null;
        }

        if (proyecto.Id < 0)
        {
            // Si es nuevo le damos el siguiente Id
            _ultimoIdProyecto++;
            proyecto.Id = _ultimoIdProyecto;
        }
        else if (!_proyectos.ContainsKey(proyecto.Id))
        {
            // Si el código no existe no se puede actualizar
            return null;
        }

        _proyectos[proyecto.Id] = proyecto;
        return proyecto;
    }

    // Buscar un proyecto por su código
    public Proyecto? LeerProyecto(int idProyecto)
    {
        if (_proyectos.ContainsKey(idProyecto))
        {
            return _proyectos[idProyecto];
        }

        return null;
    }

    // Borrar un proyecto y devolverlo, o null si no existía
    public Proyecto? EliminarProyecto(int idProyecto)
    {
        if (!_proyectos.ContainsKey(idProyecto))
        {
            return null;
        }

        Proyecto borrado = _proyectos[idProyecto];
        _proyectos.Remove(idProyecto);
        return borrado;
    }

    // Añadir una entrada al log
    public void AgregarLog(EntradaLog log)
    {
        // Si el log es null lanzamos una excepción
        if (log == null)
        {
            throw new ArgumentNullException(nameof(log));
        }

        log.Id = _logs.Count + 1;
        _logs.Add(log);
    }

    // Vaciar el log y devolver cuántas entradas había
    public int LimpiarLog()
    {
        int count = _logs.Count;
        _logs.Clear();
        return count;
    }

    // Dejar la base de datos vacía y los Ids desde el principio
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

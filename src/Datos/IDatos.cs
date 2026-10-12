using Nucleo.Modelos;
using System.Collections.Generic;

namespace Datos;

public interface IDatos
{
    public Dictionary<int, Usuario> Usuarios { get; }
    public Dictionary<int, Rol> Roles { get; }
    public Dictionary<int, Proyecto> Proyectos { get; }
    public List<EntradaLog> Logs { get; }

    #region Usuarios
    Usuario? GuardarUsuario(Usuario usuario);
    Usuario? LeerUsuario(int idUsuario);
    Usuario? LeerUsuario(string eMail);
    Usuario? EliminarUsuario(int idUsuario);
    #endregion

    #region Roles
    Rol? GuardarRol(Rol rol);
    Rol? LeerRol(int idRol);
    Rol? EliminarRol(int idRol);
    #endregion

    #region Proyectos
    Proyecto? GuardarProyecto(Proyecto proyecto);
    Proyecto? LeerProyecto(int idProyecto);
    Proyecto? EliminarProyecto(int idProyecto);
    #endregion

    #region Log
    void AgregarLog(EntradaLog log);
    int LimpiarLog();
    #endregion

    void Reiniciar();
}

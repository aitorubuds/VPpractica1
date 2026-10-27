using Nucleo.Modelos;
using System;

namespace Datos;

// Registro de auditoría: todos los accesos quedan en el log
public static class Auditoria
{
    // Niveles que se muestran en la página de Log
    public const string NivelInfo = "Info";
    public const string NivelAviso = "Aviso";
    public const string NivelError = "Error";

    // Acciones que se registran
    public const string LoginCorrecto = "Inicio de sesión";
    public const string LoginFallido = "Login fallido";
    public const string CuentaBloqueada = "Cuenta bloqueada";
    public const string CuentaInactiva = "Cuenta inactiva";
    public const string CierreSesion = "Cierre de sesión";
    public const string AccesoDenegado = "Acceso denegado";

    // Añade una entrada al log; si no hay usuario se guarda el email que se intentó usar
    public static EntradaLog Registrar(
        IDatos datos,
        string nivel,
        string accion,
        string mensaje,
        Usuario? usuario,
        string emailIntentado = "")
    {
        EntradaLog log = new EntradaLog(
            DateTime.Now,
            nivel,
            mensaje,
            usuario?.EMail ?? emailIntentado)
        {
            Accion = accion,
            UsuarioId = usuario?.Id ?? 0
        };

        datos.AgregarLog(log);
        return log;
    }
}

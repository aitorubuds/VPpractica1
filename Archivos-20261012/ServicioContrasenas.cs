using System;
using System.Collections.Generic;
using System.Text;

namespace TestProjectManagementPlatform.Nucleo.Seguridad;

/// <summary>
/// Proporciona operaciones relacionadas con contraseñas.
/// </summary>
public static class ServicioContrasenas
{
    /// <summary>
    /// Determina si una contraseña cumple la política de seguridad.
    /// </summary>
    /// <param name="contrasena">
    /// Contraseña a validar.
    /// </param>
    /// <returns>
    /// True si la contraseña es válida; en caso contrario false.
    /// </returns>
    public static bool EsValida(
        string contrasena)
    {
        if (string.IsNullOrWhiteSpace(contrasena))
        {
            return false;
        }

        if (contrasena.Length < 8)
        {
            return false;
        }

        if (!contrasena.Any(char.IsUpper))
        {
            return false;
        }

        if (!contrasena.Any(char.IsLower))
        {
            return false;
        }

        if (!contrasena.Any(char.IsDigit))
        {
            return false;
        }

        if (!contrasena.Any(c => !char.IsLetterOrDigit(c)))
        {
            return false;
        }

        return true;
    }
}
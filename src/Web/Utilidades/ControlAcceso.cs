using Datos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nucleo.Modelos;
using Nucleo.Seguridad;

namespace www.Utilidades;

// Comprueba el permiso de un área y, si no lo tiene, lo audita y lo manda a la página principal
public static class ControlAcceso
{
    public static IActionResult? ComprobarArea(PageModel pagina, IDatos datos, Usuario usuario, string area)
    {
        Rol? rol = datos.LeerRol(usuario.RolId);

        if (Permisos.TieneAcceso(usuario, rol, area))
        {
            return null;
        }

        Auditoria.Registrar(datos, Auditoria.NivelAviso, Auditoria.AccesoDenegado,
            $"Acceso denegado al área {area}", usuario);

        pagina.TempData["MensajeError"] = $"No tienes permiso para acceder a {area}.";

        return pagina.RedirectToPage("/Principal/Index");
    }
}

using Datos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nucleo.Modelos;
using www.Utilidades;

namespace www.Pages.Principal;

public class IndexModel : PageModel
{
    private readonly IDatos _datos;

    public Usuario? UsuarioActual { get; private set; }

    public string NombreUsuario { get; set; } = "";

    public IndexModel(IDatos datos)
    {
        _datos = datos;
    }

    public IActionResult OnGet()
    {
        int? idUsuario = HttpContext.Session.GetInt32(ClavesSesion.UsuarioId);

        // sin sesión iniciada vuelve al login
        if (idUsuario is null or <= 0)
        {
            EliminarDatosSesion();
            return RedirectToPage("/Login/Index");
        }

        UsuarioActual = _datos.LeerUsuario(idUsuario.Value);

        if (UsuarioActual is null)
        {
            EliminarDatosSesion();
            return RedirectToPage("/Login/Index");
        }

        NombreUsuario = UsuarioActual.EMail; // Sustituir por tu propiedad real

        return Page();
    }

    public IActionResult OnPostCerrarSesion() {
        int? idUsuario = HttpContext.Session.GetInt32(ClavesSesion.UsuarioId);
        if (idUsuario is not null)
        {
            var usuario = _datos.LeerUsuario(idUsuario.Value);
            if (usuario is not null)
            { // Registrar el evento antes de limpiar la sesión.
                Auditoria.Registrar(_datos, Auditoria.NivelInfo, Auditoria.CierreSesion,
                    "Cierre de sesión", usuario);
            }
        }
        EliminarDatosSesion();
        return RedirectToPage("/Login/Index");
    }

    private void EliminarDatosSesion() { 
        HttpContext.Session.Remove(ClavesSesion.EmailPendiente);
        HttpContext.Session.Remove(ClavesSesion.UsuarioId);
        HttpContext.Session.Remove(ClavesSesion.ProyectoActualId);
    }
}


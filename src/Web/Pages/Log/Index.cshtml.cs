using Datos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nucleo.Modelos;
using Nucleo.Seguridad;
using www.Utilidades;

namespace www.Pages.Log;

public class IndexModel : PageModel
{
    private readonly IDatos _datos;

    public IndexModel(IDatos datos)
    {
        _datos = datos;
    }

    public string NombreUsuario { get; private set; } = "";

    public List<EntradaLog> Eventos { get; private set; } = [];

    public List<string> Niveles { get; private set; } = [];

    [BindProperty(SupportsGet = true)]
    public string TextoBusqueda { get; set; } = "";

    [BindProperty(SupportsGet = true)]
    public string NivelSeleccionado { get; set; } = "";

    public IActionResult OnGet()
    {
        int? idUsuario =
            HttpContext.Session.GetInt32(
                ClavesSesion.UsuarioId);

        if (idUsuario is null)
        {
            return RedirectToPage("/Login/Index");
        }

        var usuario =
            _datos.LeerUsuario(idUsuario.Value);

        if (usuario is null)
        {
            HttpContext.Session.Remove(
                ClavesSesion.UsuarioId);

            return RedirectToPage("/Login/Index");
        }

        // El log solo lo ve quien tiene permiso del área Log (el administrador)
        IActionResult? denegado = ControlAcceso.ComprobarArea(this, _datos, usuario, Permisos.AreaLog);

        if (denegado is not null)
        {
            return denegado;
        }

        NombreUsuario = usuario.EMail;

        var eventos = _datos.Logs
            .Select(log =>
                new EntradaLog(
                    log.FechaHora,
                    log.Nivel,
                    log.Mensaje,
                    log.NombreUsuario))
            .ToList();
        Niveles = eventos
            .Select(evento => evento.Nivel)
            .Where(nivel =>
                !string.IsNullOrWhiteSpace(nivel))
            .Distinct()
            .OrderBy(nivel => nivel)
            .ToList();

        if (!string.IsNullOrWhiteSpace(
            TextoBusqueda))
        {
            eventos = eventos
                .Where(evento =>
                    evento.Mensaje.Contains(
                        TextoBusqueda,
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    evento.NombreUsuario.Contains(
                        TextoBusqueda,
                        StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(
            NivelSeleccionado))
        {
            eventos = eventos
                .Where(evento =>
                    string.Equals(
                        evento.Nivel,
                        NivelSeleccionado,
                        StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        Eventos = eventos
            .OrderByDescending(
                evento => evento.FechaHora)
            .ToList();

        return Page();
    }
}
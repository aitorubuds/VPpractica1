using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using www.Utilidades;

namespace www.Pages.Login;

public class IndexModel : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Debe indicar un correo electrónico.")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
    public string NombreUsuario { get; set; } = "";

    public IActionResult OnGet()
    {
        // Limpiar la sesión al iniciar sesión
        HttpContext.Session.Remove(ClavesSesion.EmailPendiente);
        HttpContext.Session.Remove(ClavesSesion.ProyectoActualId);
        HttpContext.Session.Remove(ClavesSesion.UsuarioId);
        return Page();
    }
    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        string emailNormalizado = NombreUsuario.Trim();

        HttpContext.Session.SetString(
            ClavesSesion.EmailPendiente,
            emailNormalizado);

        return RedirectToPage("/Login/Index2");
    }
}
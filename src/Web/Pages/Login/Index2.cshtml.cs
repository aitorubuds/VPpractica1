using Datos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nucleo.Modelos;
using System.ComponentModel.DataAnnotations;
using www.Utilidades;

namespace www.Pages.Login;

public class Index2Model : PageModel
{
    private readonly IDatos _datos;

    public Index2Model(IDatos datos)
    {
        _datos = datos;
    }

    [BindProperty]
    [Required(ErrorMessage = "Debe indicar una contraseña.")]
    [DataType(DataType.Password)]
    public string Contraseña { get; set; } = "";
    public string EMail { get; set; } = "";
    public IActionResult OnGet()
    {
        EMail = ObtenerEmailPendiente();

        if (string.IsNullOrEmpty(EMail))
        {
            return RedirectToPage("/Login/Index");
        }

        return Page();
    }

    public IActionResult OnPostAtras()
    {
        HttpContext.Session.Remove(ClavesSesion.EmailPendiente);

        return RedirectToPage("/Login/Index");
    }

    public IActionResult OnPostAceptar()
    {
        EMail = ObtenerEmailPendiente();

        if (string.IsNullOrEmpty(EMail))
        {
            return RedirectToPage("/Login/Index");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        Usuario? usuario = _datos.LeerUsuario(EMail);

        if (usuario is null)
        {
            ModelState.AddModelError(
                "Contraseña",
                "Usuario o contraseña incorrectos.");

            return Page();
        }

        bool contraseñaCorrecta = usuario.CompruebaPassword(Contraseña);

        if (!contraseñaCorrecta)
        {
            ModelState.AddModelError(
                "Contraseña",
                "Usuario o contraseña incorrectos.");

            return Page();
        }

        // Usuario correctamente autenticado.
        HttpContext.Session.Remove(
            ClavesSesion.EmailPendiente);

        HttpContext.Session.SetInt32(
            ClavesSesion.UsuarioId,
            usuario.IdUsuario);

        return RedirectToPage("/Principal/Index");
    }
    private string ObtenerEmailPendiente()
    {
        return HttpContext.Session.GetString(
            ClavesSesion.EmailPendiente) ?? "";
    }

}
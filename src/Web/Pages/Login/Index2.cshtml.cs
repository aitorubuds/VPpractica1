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

        // Todos los intentos quedan auditados. Al usuario siempre se le da
        // el mismo mensaje para no dar pistas de qué ha fallado.
        if (usuario is null)
        {
            Auditoria.Registrar(_datos, Auditoria.NivelAviso, Auditoria.LoginFallido,
                $"Intento de acceso con un email que no existe: {EMail}", null, EMail);

            return ErrorLogin();
        }

        if (usuario.Bloqueado)
        {
            Auditoria.Registrar(_datos, Auditoria.NivelAviso, Auditoria.CuentaBloqueada,
                "Intento de acceso con la cuenta bloqueada", usuario);

            return ErrorLogin();
        }

        if (!usuario.Activo)
        {
            Auditoria.Registrar(_datos, Auditoria.NivelAviso, Auditoria.CuentaInactiva,
                "Intento de acceso con la cuenta dada de baja", usuario);

            return ErrorLogin();
        }

        bool contraseñaCorrecta = usuario.CompruebaPassword(Contraseña);

        if (!contraseñaCorrecta)
        {
            Auditoria.Registrar(_datos, Auditoria.NivelAviso, Auditoria.LoginFallido,
                $"Contraseña incorrecta (intento {usuario.IntentosFallidos})", usuario);

            // Al tercer fallo ComprobarContrasena bloquea la cuenta
            if (usuario.Bloqueado)
            {
                Auditoria.Registrar(_datos, Auditoria.NivelError, Auditoria.CuentaBloqueada,
                    "Cuenta bloqueada tras 3 intentos fallidos", usuario);
            }

            return ErrorLogin();
        }

        // Usuario correctamente autenticado.
        usuario.FechaUltimoAcceso = DateTime.Now;
        Auditoria.Registrar(_datos, Auditoria.NivelInfo, Auditoria.LoginCorrecto,
            "Inicio de sesión correcto", usuario);

        HttpContext.Session.Remove(
            ClavesSesion.EmailPendiente);

        HttpContext.Session.SetInt32(
            ClavesSesion.UsuarioId,
            usuario.IdUsuario);

        return RedirectToPage("/Principal/Index");
    }
    private IActionResult ErrorLogin()
    {
        ModelState.AddModelError(
            "Contraseña",
            "Usuario o contraseña incorrectos.");

        return Page();
    }

    private string ObtenerEmailPendiente()
    {
        return HttpContext.Session.GetString(
            ClavesSesion.EmailPendiente) ?? "";
    }

}
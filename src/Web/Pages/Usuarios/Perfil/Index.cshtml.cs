using System.ComponentModel.DataAnnotations;
using Datos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nucleo.Modelos;
using Nucleo.Servicios;
using www.Utilidades;
using UsuarioModelo = Nucleo.Modelos.Usuario;

namespace www.Pages.Usuarios.Perfil;

public class IndexModel : PageModel
{
    private readonly IDatos _datos;
    private readonly ConfiguracionCifrado _cifrado;

    public IndexModel(IDatos datos, ConfiguracionCifrado cifrado)
    {
        _datos = datos;
        _cifrado = cifrado;
    }

    public string NombreUsuarioCabecera { get; private set; } = string.Empty;

    public string NombreProyectoActual { get; private set; }
        = "Sin proyecto seleccionado";

    public int IdUsuario { get; private set; }

    public bool SinPassword { get; private set; }

    public bool CuentaActiva { get; private set; }

    public int Rol { get; private set; }

    public int IntentosErroneos { get; private set; }

    public int NumeroPasswordsAnteriores { get; private set; }

    public string FechaUltimoAccesoTexto { get; private set; }
        = "No consta";

    public string FechaAccesoActualTexto { get; private set; }
        = "No consta";

    public string UltimoCambioPasswordTexto { get; private set; }
        = "No consta";

    public string BloquearAccesoHastaTexto { get; private set; }
        = "No consta";

    public string CaducidadCuentaTexto { get; private set; }
        = "No consta";

    [BindProperty]
    [Required(ErrorMessage = "Debe indicar el correo electrónico.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no es válido.")]
    [Display(Name = "Correo electrónico")]
    public string EMail { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Debe indicar el nombre.")]
    [StringLength(
        100,
        ErrorMessage = "El nombre no puede superar 100 caracteres.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Debe indicar los apellidos.")]
    [StringLength(
        150,
        ErrorMessage = "Los apellidos no pueden superar 150 caracteres.")]
    [Display(Name = "Apellidos")]
    public string Apellidos { get; set; } = string.Empty;

    // Dato sensible: en la base de datos se guarda cifrado
    [BindProperty]
    [RegularExpression(@"^\d{9}$", ErrorMessage = "El teléfono debe tener 9 cifras.")]
    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Debe indicar la contraseña actual.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña actual")]
    public string PasswordActual { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Debe indicar la nueva contraseña.")]
    [DataType(DataType.Password)]
    [StringLength(
        100,
        MinimumLength = 8,
        ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    [Display(Name = "Nueva contraseña")]
    public string PasswordNueva { get; set; } = string.Empty;

    [BindProperty]
    [Required(ErrorMessage = "Debe repetir la nueva contraseña.")]
    [DataType(DataType.Password)]
    [Compare(
        nameof(PasswordNueva),
        ErrorMessage = "Las nuevas contraseñas no coinciden.")]
    [Display(Name = "Repetir nueva contraseña")]
    public string ConfirmacionPassword { get; set; } = string.Empty;

    public IActionResult OnGet()
    {
        UsuarioModelo? usuario = ObtenerUsuarioActual();

        if (usuario is null)
        {
            LimpiarSesion();
            return RedirectToPage("/Login/Index");
        }

        CargarCabecera(usuario);
        CargarDatosEditables(usuario);
        CargarDatosCuenta(usuario);

        return Page();
    }

    public IActionResult OnPostGuardarPerfil()
    {
        UsuarioModelo? usuario = ObtenerUsuarioActual();

        if (usuario is null)
        {
            LimpiarSesion();
            return RedirectToPage("/Login/Index");
        }

        CargarCabecera(usuario);
        CargarDatosCuenta(usuario);
        QuitarValidacionPassword();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        string emailNormalizado = EMail.Trim();

        UsuarioModelo? usuarioConMismoEmail =
            _datos.LeerUsuario(emailNormalizado);

        if (usuarioConMismoEmail is not null &&
            usuarioConMismoEmail.IdUsuario != usuario.IdUsuario)
        {
            ModelState.AddModelError(
                nameof(EMail),
                "Ya existe otro usuario con ese correo electrónico.");

            return Page();
        }

        usuario.EMail = emailNormalizado;
        usuario.Nombre = Nombre.Trim();
        usuario.Apellidos = Apellidos.Trim();
        usuario.EstablecerTelefono(Telefono ?? string.Empty, _cifrado.Clave);

        _datos.GuardarUsuario(usuario);

        TempData["MensajePerfil"] =
            "Los datos del perfil se han actualizado.";

        return RedirectToPage();
    }

    public IActionResult OnPostCambiarPassword()
    {
        UsuarioModelo? usuario = ObtenerUsuarioActual();

        if (usuario is null)
        {
            LimpiarSesion();
            return RedirectToPage("/Login/Index");
        }

        CargarCabecera(usuario);
        CargarDatosEditables(usuario);
        CargarDatosCuenta(usuario);
        QuitarValidacionPerfil();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (!usuario.CompruebaPassword(PasswordActual))
        {
            ModelState.AddModelError(
                nameof(PasswordActual),
                "La contraseña actual no es correcta.");

            return Page();
        }

        bool cambioCorrecto = usuario.CambiaPassword(
            PasswordActual,
            PasswordNueva);

        if (!cambioCorrecto)
        {
            ModelState.AddModelError(
                nameof(PasswordNueva),
                "No se ha podido cambiar la contraseña.");

            return Page();
        }

        _datos.GuardarUsuario(usuario);

        TempData["MensajePassword"] =
            "La contraseña se ha cambiado correctamente.";

        return RedirectToPage();
    }

    private Usuario? ObtenerUsuarioActual()
    {
        int? idUsuario = HttpContext.Session.GetInt32(
            ClavesSesion.UsuarioId);

        if (idUsuario is not int id)
        {
            return null;
        }

        return _datos.LeerUsuario(id);
    }

    private void CargarDatosEditables(Usuario usuario)
    {
        EMail = usuario.EMail;
        Nombre = usuario.Nombre;
        Apellidos = usuario.Apellidos;
        Telefono = usuario.ObtenerTelefono(_cifrado.Clave);
    }

    private void CargarDatosCuenta(Usuario usuario)
    {
        IdUsuario = usuario.IdUsuario;
        SinPassword = usuario.TieneCredencialesConfiguradas;
        CuentaActiva = usuario.Activa;
        Rol = 0;
        IntentosErroneos = usuario.IntentosErroneos;
        NumeroPasswordsAnteriores = usuario.ListaPasswordsCount;

        FechaUltimoAccesoTexto = Formatos.FormatearFecha(usuario.FechaUltimoAcceso);

        FechaAccesoActualTexto = Formatos.FormatearFecha(usuario.FechaUltimoAcceso);

        UltimoCambioPasswordTexto = Formatos.FormatearFecha(usuario.UltimoCambioPassword);

        BloquearAccesoHastaTexto = Formatos.FormatearFecha(usuario.BloquearAccesoHasta);

        CaducidadCuentaTexto = Formatos.FormatearFecha(usuario.CaducidadCuenta);
    }

    private void CargarCabecera(Usuario usuario)
    {
        NombreUsuarioCabecera =
            $"{usuario.Nombre} {usuario.Apellidos}".Trim();

        int? idProyecto = HttpContext.Session.GetInt32(
            ClavesSesion.ProyectoActualId);

        if (idProyecto is not int id)
        {
            NombreProyectoActual =
                "Sin proyecto seleccionado";

            return;
        }

        Proyecto? proyecto = _datos.LeerProyecto(id);

        if (proyecto is null || !proyecto.Activo)
        {
            HttpContext.Session.Remove(
                ClavesSesion.ProyectoActualId);

            NombreProyectoActual =
                "Sin proyecto seleccionado";

            return;
        }

        NombreProyectoActual = proyecto.Nombre;
    }

    private static string FormatearFecha(DateTime fecha)
    {
        if (fecha == default)
        {
            return "No consta";
        }

        return fecha.ToString("dd/MM/yyyy HH:mm:ss");
    }

    private void QuitarValidacionPassword()
    {
        ModelState.Remove(nameof(PasswordActual));
        ModelState.Remove(nameof(PasswordNueva));
        ModelState.Remove(nameof(ConfirmacionPassword));
    }

    private void QuitarValidacionPerfil()
    {
        ModelState.Remove(nameof(EMail));
        ModelState.Remove(nameof(Nombre));
        ModelState.Remove(nameof(Apellidos));
        ModelState.Remove(nameof(Telefono));
    }

    private void LimpiarSesion()
    {
        HttpContext.Session.Remove(
            ClavesSesion.EmailPendiente);

        HttpContext.Session.Remove(
            ClavesSesion.UsuarioId);

        HttpContext.Session.Remove(
            ClavesSesion.ProyectoActualId);
    }
}

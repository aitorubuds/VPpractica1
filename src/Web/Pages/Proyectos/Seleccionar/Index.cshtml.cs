using System.ComponentModel.DataAnnotations;
using Datos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nucleo.Modelos;
using Nucleo.Seguridad;
using www.Utilidades;

namespace www.Pages.Proyectos.Seleccionar;

public class IndexModel : PageModel
{
    private readonly IDatos _datos;

    public IndexModel(IDatos datos)
    {
        _datos = datos;
    }

    public List<Proyecto> ProyectosDisponibles
    {
        get;
        private set;
    } = [];

    public List<Proyecto> ProyectosDesactivados
    {
        get;
        private set;
    } = [];

    public int? IdProyectoActual
    {
        get;
        private set;
    }

    public string NombreProyectoActual
    {
        get;
        private set;
    } = "Sin proyecto seleccionado";

    public string NombreUsuario
    {
        get;
        private set;
    } = string.Empty;

    // Solo el administrador ve el formulario de alta
    public bool PuedeCrear { get; private set; }

    private Usuario? _usuario;

    [BindProperty]
    [Required(
        ErrorMessage = "Debe indicar el nombre del proyecto.")]
    [StringLength(
        100,
        MinimumLength = 3,
        ErrorMessage =
            "El nombre debe tener entre 3 y 100 caracteres.")]
    [Display(Name = "Nombre")]
    public string Nombre
    {
        get;
        set;
    } = string.Empty;

    [BindProperty]
    [Required(
        ErrorMessage = "Debe indicar las siglas del proyecto.")]
    [StringLength(
        15,
        MinimumLength = 2,
        ErrorMessage =
            "Las siglas deben tener entre 2 y 15 caracteres.")]
    [Display(Name = "Siglas")]
    public string Siglas
    {
        get;
        set;
    } = string.Empty;

    [BindProperty]
    [StringLength(
        500,
        ErrorMessage =
            "La descripción no puede superar 500 caracteres.")]
    [Display(Name = "Descripción")]
    // Opcional: con string (no anulable) ASP.NET la trataba como obligatoria (DEF-05)
    public string? Descripcion
    {
        get;
        set;
    } = string.Empty;

    public IActionResult OnGet()
    {
        IActionResult? resultado = CargarDatosComunes();

        if (resultado is not null)
        {
            return resultado;
        }

        return Page();
    }

    public IActionResult OnPostCrear()
    {
        IActionResult? resultado = CargarDatosComunes();

        if (resultado is not null)
        {
            return resultado;
        }

        // Crear proyectos es una tarea de gestión: solo el administrador
        if (!PuedeCrear)
        {
            Auditoria.Registrar(_datos, Auditoria.NivelAviso, Auditoria.AccesoDenegado,
                "Intento de crear un proyecto sin ser administrador", _usuario);

            ModelState.AddModelError(
                string.Empty,
                "Solo el administrador puede crear proyectos.");

            return Page();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        string nombreNormalizado = Nombre.Trim();

        string siglasNormalizadas =
            Siglas.Trim().ToUpperInvariant();

        // Se comprueba contra todos los proyectos, no solo los que ve el usuario
        List<Proyecto> todosLosProyectos =
            _datos.ProyectosTodos().ToList();

        bool nombreRepetido =
            todosLosProyectos.Any(proyecto =>
                string.Equals(
                    proyecto.Nombre,
                    nombreNormalizado,
                    StringComparison.OrdinalIgnoreCase));

        if (nombreRepetido)
        {
            ModelState.AddModelError(
                nameof(Nombre),
                "Ya existe un proyecto con ese nombre.");

            return Page();
        }

        bool siglasRepetidas =
            todosLosProyectos.Any(proyecto =>
                string.Equals(
                    proyecto.Siglas,
                    siglasNormalizadas,
                    StringComparison.OrdinalIgnoreCase));

        if (siglasRepetidas)
        {
            ModelState.AddModelError(
                nameof(Siglas),
                "Ya existe un proyecto con esas siglas.");

            return Page();
        }

        var nuevoProyecto = new Proyecto
        {
            IdProyecto = -1,
            Nombre = nombreNormalizado,
            Siglas = siglasNormalizadas,
            Descripcion = Descripcion?.Trim() ?? string.Empty,
            FechaCreacion = DateTime.Now,

            // Activo significa que el proyecto está disponible.
            Activo = true
        };

        Proyecto? proyectoGuardado =
            _datos.GuardarProyecto(nuevoProyecto);

        // Si la capa de datos no lo guarda avisamos en vez de romper
        if (proyectoGuardado is null)
        {
            ModelState.AddModelError(
                string.Empty,
                "No se ha podido crear el proyecto.");

            return Page();
        }

        // El nuevo proyecto se establece como proyecto actual
        // de este usuario.
        HttpContext.Session.SetInt32(
            ClavesSesion.ProyectoActualId,
            proyectoGuardado.IdProyecto);

        TempData["MensajeCorrecto"] =
            "El proyecto se ha creado y seleccionado.";

        return RedirectToPage();
    }

    public IActionResult OnPostSeleccionar(int idProyecto)
    {
        /*
         * Este formulario solamente envía el identificador.
         * Se eliminan los errores de validación correspondientes
         * al formulario de creación.
         */
        ModelState.Clear();

        IActionResult? resultado = CargarDatosComunes();

        if (resultado is not null)
        {
            return resultado;
        }

        Proyecto? proyecto =
            _datos.LeerProyecto(idProyecto);

        // Un proyecto al que no está asignado es como si no existiera para él
        if (proyecto is not null && !Permisos.PuedeVerProyecto(_usuario, proyecto))
        {
            Auditoria.Registrar(_datos, Auditoria.NivelAviso, Auditoria.AccesoDenegado,
                $"Intento de seleccionar el proyecto {proyecto.Siglas} sin estar asignado", _usuario);

            proyecto = null;
        }

        if (proyecto is null)
        {
            ModelState.AddModelError(
                string.Empty,
                "El proyecto seleccionado no existe.");

            return Page();
        }

        if (!proyecto.Activo)
        {
            ModelState.AddModelError(
                string.Empty,
                "El proyecto está desactivado y no puede seleccionarse.");

            return Page();
        }

        HttpContext.Session.SetInt32(
            ClavesSesion.ProyectoActualId,
            proyecto.IdProyecto);

        TempData["MensajeCorrecto"] =
            $"El proyecto «{proyecto.Nombre}» se ha seleccionado.";

        return RedirectToPage();
    }

    private IActionResult? CargarDatosComunes()
    {
        int? idUsuario =
            HttpContext.Session.GetInt32(
                ClavesSesion.UsuarioId);

        if (idUsuario is null)
        {
            return RedirectToPage("/Login/Index");
        }

        Usuario? usuario =
            _datos.LeerUsuario(idUsuario.Value);

        if (usuario is null)
        {
            EliminarDatosSesion();

            return RedirectToPage("/Login/Index");
        }

        // Nombre y apellidos para el menú de la cabecera
        NombreUsuario = usuario.ToString();

        // Hace falta permiso del área Proyectos (el invitado no lo tiene)
        IActionResult? denegado = ControlAcceso.ComprobarArea(this, _datos, usuario, Permisos.AreaProyectos);

        if (denegado is not null)
        {
            return denegado;
        }

        _usuario = usuario;
        PuedeCrear = Permisos.PuedeCrearProyectos(usuario);

        IdProyectoActual =
            HttpContext.Session.GetInt32(
                ClavesSesion.ProyectoActualId);

        // Cada usuario solo ve los proyectos a los que está asignado
        List<Proyecto> proyectos =
            _datos
                .ProyectosTodos()
                .Where(proyecto => Permisos.PuedeVerProyecto(usuario, proyecto))
                .OrderBy(proyecto => proyecto.Nombre)
                .ToList();

        ProyectosDisponibles =
            proyectos
                .Where(proyecto => proyecto.Activo)
                .ToList();

        ProyectosDesactivados =
            proyectos
                .Where(proyecto => !proyecto.Activo)
                .ToList();

        ComprobarProyectoActual();

        return null;
    }

    private void ComprobarProyectoActual()
    {
        if (IdProyectoActual is null)
        {
            NombreProyectoActual =
                "Sin proyecto seleccionado";

            return;
        }

        Proyecto? proyectoActual =
            _datos.LeerProyecto(
                IdProyectoActual.Value);

        if (proyectoActual is null ||
            !proyectoActual.Activo)
        {
            HttpContext.Session.Remove(
                ClavesSesion.ProyectoActualId);

            IdProyectoActual = null;

            NombreProyectoActual =
                "Sin proyecto seleccionado";

            return;
        }

        NombreProyectoActual =
            proyectoActual.Nombre;
    }

    private void EliminarDatosSesion()
    {
        HttpContext.Session.Remove(
            ClavesSesion.EmailPendiente);

        HttpContext.Session.Remove(
            ClavesSesion.UsuarioId);

        HttpContext.Session.Remove(
            ClavesSesion.ProyectoActualId);
    }
}
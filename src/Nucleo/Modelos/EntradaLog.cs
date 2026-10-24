namespace Nucleo.Modelos;

public class EntradaLog
{
    public int Id { get; set; } = -1;
    public DateTime FechaHora { get; set; } = DateTime.Now;
    public int UsuarioId { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string Detalles { get; set; } = string.Empty;

    // Propiedades y constructor para la vista web de Log
    public string Nivel { get; set; } = "Info";
    public string Mensaje { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;

    public EntradaLog()
    {
    }

    public EntradaLog(DateTime fechaHora, string nivel, string mensaje, string nombreUsuario)
    {
        FechaHora = fechaHora;
        Nivel = nivel;
        Mensaje = mensaje;
        NombreUsuario = nombreUsuario;
    }
}

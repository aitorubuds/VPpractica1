namespace Nucleo.Modelos;

public class EntradaLog
{
    public int Id { get; set; } = -1;
    public DateTime FechaHora { get; set; } = DateTime.Now;
    public int UsuarioId { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string Detalles { get; set; } = string.Empty;
}

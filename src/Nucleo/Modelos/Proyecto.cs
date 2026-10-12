namespace Nucleo.Modelos;

public class Proyecto
{
    public int Id { get; set; } = -1;
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public List<int> UsuariosAsignados { get; set; } = new();
}

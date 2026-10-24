namespace Nucleo.Modelos;

public class Proyecto
{
    public int Id { get; set; } = -1;
    public int IdProyecto { get => Id; set => Id = value; }
    public string Nombre { get; set; } = string.Empty;
    public string Siglas { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public bool Activo { get; set; } = true;
    public List<int> UsuariosAsignados { get; set; } = new();
}

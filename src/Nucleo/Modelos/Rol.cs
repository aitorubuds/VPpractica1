namespace Nucleo.Modelos;

public class Rol
{
    public int Id { get; set; } = -1;
    public string Nombre { get; set; } = string.Empty;
    public List<string> Permisos { get; set; } = new();
}

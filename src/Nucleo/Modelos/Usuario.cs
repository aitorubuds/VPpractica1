namespace Nucleo.Modelos;

public class Usuario
{
    public int Id { get; set; } = -1;
    public string Cuenta { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string EMail { get; set; } = string.Empty;
    public string HashContrasena { get; set; } = string.Empty;
    public string Sal { get; set; } = string.Empty;
    public int RolId { get; set; }
    public bool Activo { get; set; } = true;
}

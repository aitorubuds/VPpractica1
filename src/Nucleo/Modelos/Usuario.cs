using Nucleo.Seguridad;

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

    // bloqueo por intentos fallidos
    public int IntentosFallidos { get; set; } = 0;
    public bool Bloqueado { get; set; } = false;

    // asignar la contraseña por primera vez
    public void EstablecerContrasena(string contrasena)
    {
        ResultadoHash datos = ServicioHash.Generar(contrasena);
        HashContrasena = datos.Hash;
        Sal = datos.Sal;
    }

    // Compruebar si contraseña introducida es correcta
    public bool ComprobarContrasena(string contrasena)
    {
        // Si el usuario está bloqueado o inactivo no entra
        if (Bloqueado || !Activo)
        {
            return false;
        }

        // Comprobamos el hash con la sal guardada
        bool esCorrecta = ServicioHash.Verificar(contrasena, HashContrasena, Sal);

        if (esCorrecta)
        {
            // Si acierta, reseteamos el contador
            IntentosFallidos = 0;
            return true;
        }
        else
        {
            // Si falla, sumamos fallo
            IntentosFallidos++;

            // Si llega a 3 fallos seguidos, bloqueamos la cuenta
            if (IntentosFallidos >= 3)
            {
                Bloqueado = true;
            }

            return false;
        }
    }

    // Permite al usuario cambiar su contraseña
    public bool CambiarContrasena(string contrasenaActual, string nuevaContrasena)
    {
        // Si la cuenta está bloqueada no se puede cambiar
        if (Bloqueado)
        {
            return false;
        }

        // 1. Comprobar que sabe la contraseña vieja
        bool claveActualOk = ServicioHash.Verificar(contrasenaActual, HashContrasena, Sal);
        if (!claveActualOk)
        {
            return false;
        }

        // 2. Comprobar que la nueva cumple los requisitos de seguridad
        bool claveNuevaOk = ServicioContrasenas.EsValida(nuevaContrasena);
        if (!claveNuevaOk)
        {
            return false;
        }

        // 3. Si todo está bien, generamos el nuevo hash y guardamos
        ResultadoHash nuevoHash = ServicioHash.Generar(nuevaContrasena);
        HashContrasena = nuevoHash.Hash;
        Sal = nuevoHash.Sal;

        return true;
    }

    // El administrador puede desbloquear la cuenta
    public void Desbloquear()
    {
        Bloqueado = false;
        IntentosFallidos = 0;
    }
}

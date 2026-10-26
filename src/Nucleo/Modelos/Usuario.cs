using Nucleo.Seguridad;

namespace Nucleo.Modelos;

public class Usuario
{
    private static readonly ServicioHash _servicioHash = new();

    public int Id { get; set; } = -1;
    public int IdUsuario { get => Id; set => Id = value; }
    public string Cuenta { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string EMail { get; set; } = string.Empty;
    public string HashContrasena { get; set; } = string.Empty;
    public string Sal { get; set; } = string.Empty;
    public int RolId { get; set; }
    public bool Activo { get; set; } = true;
    public bool Activa { get => Activo; set => Activo = value; }

    // Indica si el usuario es administrador
    public bool EsAdministrador { get; set; } = false;

    // El teléfono es un dato sensible, por eso se guarda cifrado y nunca en claro
    public string TelefonoCifrado { get; set; } = string.Empty;

    // bloqueo por intentos fallidos
    public int IntentosFallidos { get; set; } = 0;
    public int IntentosErroneos { get => IntentosFallidos; set => IntentosFallidos = value; }
    public bool Bloqueado { get; set; } = false;

    // propiedades para auditoría y perfil web
    public bool TieneCredencialesConfiguradas => !string.IsNullOrEmpty(HashContrasena);
    public int ListaPasswordsCount { get; set; } = 1;
    public DateTime FechaUltimoAcceso { get; set; } = DateTime.Now;
    public DateTime UltimoCambioPassword { get; set; } = DateTime.Now;
    public DateTime BloquearAccesoHasta { get; set; } = default;
    public DateTime CaducidadCuenta { get; set; } = default;

    // asignar la contraseña por primera vez
    public void EstablecerContrasena(string contrasena)
    {
        ResultadoHash datos = _servicioHash.Generar(contrasena);
        HashContrasena = datos.Hash;
        Sal = datos.Sal;
    }

    // Guardar el teléfono cifrado con la clave
    public void EstablecerTelefono(string telefono, string clave)
    {
        // Si viene vacío, borramos el teléfono que hubiera
        if (string.IsNullOrWhiteSpace(telefono))
        {
            TelefonoCifrado = string.Empty;
        }
        else
        {
            // Quitamos los espacios de los lados y lo ciframos
            TelefonoCifrado = ServicioCifrado.Cifrar(telefono.Trim(), clave);
        }
    }

    // Devolver el teléfono descifrado
    public string ObtenerTelefono(string clave)
    {
        // Si no tiene teléfono devolvemos vacío
        if (string.IsNullOrEmpty(TelefonoCifrado))
        {
            return string.Empty;
        }

        return ServicioCifrado.Descifrar(TelefonoCifrado, clave);
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
        bool esCorrecta = _servicioHash.Verificar(contrasena, HashContrasena, Sal);

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
        bool claveActualOk = _servicioHash.Verificar(contrasenaActual, HashContrasena, Sal);
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
        ResultadoHash nuevoHash = _servicioHash.Generar(nuevaContrasena);
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

    // Alias para que funcione con la web del profesor: hace lo mismo que ComprobarContrasena
    public bool CompruebaPassword(string contrasena)
    {
        return ComprobarContrasena(contrasena);
    }

    // Alias para que funcione con la web del profesor: hace lo mismo que CambiarContrasena
    public bool CambiaPassword(string contrasenaActual, string nuevaContrasena)
    {
        return CambiarContrasena(contrasenaActual, nuevaContrasena);
    }

    // Devolver el nombre completo del usuario
    public override string ToString()
    {
        // Si no tiene apellidos, solo el nombre
        if (string.IsNullOrWhiteSpace(Apellidos))
        {
            return Nombre;
        }

        return $"{Nombre} {Apellidos}".Trim();
    }
}

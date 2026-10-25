using System.Security.Cryptography;
using System.Text;

namespace Nucleo.Seguridad;

/// <summary>
/// Proporciona servicios de cifrado y descifrado de información utilizando
/// el algoritmo AES-GCM (Advanced Encryption Standard en modo Galois/Counter Mode).
/// </summary>
/// <remarks>
/// Esta clase permite:
/// - Generar claves criptográficas compatibles con AES-256.
/// - Cifrar texto plano en formato UTF-8.
/// - Descifrar contenido previamente cifrado.
/// - Validar la integridad y autenticidad de los datos mediante etiquetas de autenticación.
///
/// El contenido cifrado generado contiene:
/// 1. Nonce.
/// 2. Etiqueta de autenticación.
/// 3. Datos cifrados.
///
/// Todo el resultado se devuelve codificado en Base64.
/// </remarks>
public static class ServicioCifrado
{
    /// <summary>
    /// Tamaño del nonce utilizado por AES-GCM en bytes.
    /// </summary>
    private const int TamanoNonce = 12;

    /// <summary>
    /// Tamaño de la etiqueta de autenticación en bytes.
    /// </summary>
    private const int TamanoEtiqueta = 16;

    /// <summary>
    /// Tamaño de la clave AES-256 en bytes.
    /// </summary>
    private const int TamanoClave = 32;

    /// <summary>
    /// Genera una nueva clave criptográfica aleatoria compatible con AES-256.
    /// </summary>
    /// <returns>
    /// Clave codificada en formato Base64.
    /// </returns>
    /// <remarks>
    /// La clave generada contiene 32 bytes (256 bits),
    /// que constituyen la longitud requerida para AES-256.
    /// </remarks>
    public static string GenerarClave()
    {
        byte[] clave =
            RandomNumberGenerator.GetBytes(TamanoClave);

        return Convert.ToBase64String(clave);
    }

    /// <summary>
    /// Cifra un texto utilizando AES-GCM.
    /// </summary>
    /// <param name="texto">
    /// Texto plano que se desea cifrar.
    /// </param>
    /// <param name="claveBase64">
    /// Clave de cifrado codificada en Base64.
    /// </param>
    /// <returns>
    /// Contenido cifrado codificado en Base64.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Se produce cuando el texto está vacío o cuando la clave no es válida.
    /// </exception>
    /// <remarks>
    /// El resultado incluye el nonce, la etiqueta de autenticación
    /// y el contenido cifrado.
    /// </remarks>
    public static string Cifrar(
        string texto,
        string claveBase64)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            throw new ArgumentException(
                "El texto no puede estar vacío.",
                nameof(texto));
        }

        byte[] clave = ObtenerClave(claveBase64);
        byte[] textoPlano = Encoding.UTF8.GetBytes(texto);
        byte[] textoCifrado = new byte[textoPlano.Length];

        byte[] nonce =
            RandomNumberGenerator.GetBytes(TamanoNonce);

        byte[] etiqueta = new byte[TamanoEtiqueta];

        using var aes = new AesGcm(
            clave,
            TamanoEtiqueta);

        aes.Encrypt(
            nonce,
            textoPlano,
            textoCifrado,
            etiqueta);

        byte[] resultado = new byte[
            nonce.Length +
            etiqueta.Length +
            textoCifrado.Length];

        Buffer.BlockCopy(
            nonce,
            0,
            resultado,
            0,
            nonce.Length);

        Buffer.BlockCopy(
            etiqueta,
            0,
            resultado,
            nonce.Length,
            etiqueta.Length);

        Buffer.BlockCopy(
            textoCifrado,
            0,
            resultado,
            nonce.Length + etiqueta.Length,
            textoCifrado.Length);

        return Convert.ToBase64String(resultado);
    }

    /// <summary>
    /// Descifra contenido previamente generado mediante el método Cifrar.
    /// </summary>
    /// <param name="contenidoCifradoBase64">
    /// Contenido cifrado codificado en Base64.
    /// </param>
    /// <param name="claveBase64">
    /// Clave de descifrado codificada en Base64.
    /// </param>
    /// <returns>
    /// Texto plano original.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Se produce cuando el contenido cifrado o la clave tienen un formato inválido.
    /// </exception>
    /// <exception cref="CryptographicException">
    /// Se produce cuando la información no puede validarse o descifrarse correctamente.
    /// </exception>
    /// <remarks>
    /// Durante el proceso se valida la integridad de los datos mediante
    /// la etiqueta de autenticación almacenada junto al contenido cifrado.
    /// </remarks>
    public static string Descifrar(
        string contenidoCifradoBase64,
        string claveBase64)
    {
        if (string.IsNullOrWhiteSpace(
            contenidoCifradoBase64))
        {
            throw new ArgumentException(
                "El contenido cifrado no puede estar vacío.",
                nameof(contenidoCifradoBase64));
        }

        byte[] clave = ObtenerClave(claveBase64);

        byte[] contenido =
            Convert.FromBase64String(
                contenidoCifradoBase64);

        int tamanoMinimo =
            TamanoNonce + TamanoEtiqueta;

        if (contenido.Length <= tamanoMinimo)
        {
            throw new ArgumentException(
                "El contenido cifrado no tiene un formato válido.",
                nameof(contenidoCifradoBase64));
        }

        byte[] nonce = contenido[..TamanoNonce];

        byte[] etiqueta = contenido[
            TamanoNonce..(TamanoNonce + TamanoEtiqueta)];

        byte[] textoCifrado = contenido[
            (TamanoNonce + TamanoEtiqueta)..];

        byte[] textoPlano =
            new byte[textoCifrado.Length];

        using var aes = new AesGcm(
            clave,
            TamanoEtiqueta);

        aes.Decrypt(
            nonce,
            textoCifrado,
            etiqueta,
            textoPlano);

        return Encoding.UTF8.GetString(textoPlano);
    }

    /// <summary>
    /// Obtiene y valida una clave criptográfica codificada en Base64.
    /// </summary>
    /// <param name="claveBase64">
    /// Clave codificada en Base64.
    /// </param>
    /// <returns>
    /// Clave decodificada en formato binario.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Se produce cuando la clave está vacía, no es Base64 válido
    /// o no tiene la longitud requerida.
    /// </exception>
    /// <remarks>
    /// La implementación exige una longitud de 32 bytes para garantizar
    /// la compatibilidad con AES-256.
    /// </remarks>
    private static byte[] ObtenerClave(
        string claveBase64)
    {
        if (string.IsNullOrWhiteSpace(claveBase64))
        {
            throw new ArgumentException(
                "La clave no puede estar vacía.",
                nameof(claveBase64));
        }

        byte[] clave;

        try
        {
            clave = Convert.FromBase64String(
                claveBase64);
        }
        catch (FormatException excepcion)
        {
            throw new ArgumentException(
                "La clave no tiene un formato Base64 válido.",
                nameof(claveBase64),
                excepcion);
        }

        if (clave.Length != TamanoClave)
        {
            throw new ArgumentException(
                "La clave debe tener 32 bytes.",
                nameof(claveBase64));
        }

        return clave;
    }
}
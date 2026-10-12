using System.Security.Cryptography;
using System.Text;

namespace Nucleo.Seguridad;

public static class ServicioCifrado
{
    private const int TamanoNonce = 12;
    private const int TamanoEtiqueta = 16;
    private const int TamanoClave = 32;

    public static string GenerarClave()
    {
        byte[] clave = RandomNumberGenerator.GetBytes(TamanoClave);
        return Convert.ToBase64String(clave);
    }

    public static string Cifrar(string texto, string claveBase64)
    {
        if (string.IsNullOrWhiteSpace(texto))
            throw new ArgumentException("El texto no puede estar vacío.", nameof(texto));

        byte[] clave = Convert.FromBase64String(claveBase64);
        byte[] textoPlano = Encoding.UTF8.GetBytes(texto);
        byte[] textoCifrado = new byte[textoPlano.Length];
        byte[] nonce = RandomNumberGenerator.GetBytes(TamanoNonce);
        byte[] etiqueta = new byte[TamanoEtiqueta];

        using var aes = new AesGcm(clave, TamanoEtiqueta);
        aes.Encrypt(nonce, textoPlano, textoCifrado, etiqueta);

        byte[] resultado = new byte[nonce.Length + etiqueta.Length + textoCifrado.Length];
        Buffer.BlockCopy(nonce, 0, resultado, 0, nonce.Length);
        Buffer.BlockCopy(etiqueta, 0, resultado, nonce.Length, etiqueta.Length);
        Buffer.BlockCopy(textoCifrado, 0, resultado, nonce.Length + etiqueta.Length, textoCifrado.Length);

        return Convert.ToBase64String(resultado);
    }

    public static string Descifrar(string contenidoCifradoBase64, string claveBase64)
    {
        byte[] contenidoCifrado = Convert.FromBase64String(contenidoCifradoBase64);
        byte[] clave = Convert.FromBase64String(claveBase64);
        
        int offset = 0;
        byte[] nonce = new byte[TamanoNonce];
        Buffer.BlockCopy(contenidoCifrado, offset, nonce, 0, TamanoNonce);
        offset += TamanoNonce;

        byte[] etiqueta = new byte[TamanoEtiqueta];
        Buffer.BlockCopy(contenidoCifrado, offset, etiqueta, 0, TamanoEtiqueta);
        offset += TamanoEtiqueta;

        byte[] textoCifrado = new byte[contenidoCifrado.Length - offset];
        Buffer.BlockCopy(contenidoCifrado, offset, textoCifrado, 0, textoCifrado.Length);

        using var aes = new AesGcm(clave, TamanoEtiqueta);
        byte[] textoPlano = new byte[textoCifrado.Length];
        aes.Decrypt(nonce, textoCifrado, etiqueta, textoPlano);

        return Encoding.UTF8.GetString(textoPlano);
    }
}

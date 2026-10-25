using System;
using System.Security.Cryptography;

namespace Nucleo.Seguridad;

public class ServicioHash
{
    private const int TamanoSal = 16;
    private const int TamanoHash = 32;
    private const int NumeroIteraciones = 100_000;

    public ResultadoHash Generar(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            throw new InvalidOperationException("El texto no puede estar vacío.");
        }

        byte[] sal = RandomNumberGenerator.GetBytes(TamanoSal);
        byte[] hash = Calcular(texto, sal);

        return new ResultadoHash
        {
            Hash = Convert.ToBase64String(hash),
            Sal = Convert.ToBase64String(sal)
        };
    }

    public bool Verificar(
        string? texto,
        string? hashAlmacenado,
        string? salAlmacenada)
    {
        if (string.IsNullOrWhiteSpace(texto) ||
            string.IsNullOrWhiteSpace(hashAlmacenado) ||
            string.IsNullOrWhiteSpace(salAlmacenada))
        {
            return false;
        }

        try
        {
            byte[] sal = Convert.FromBase64String(salAlmacenada);
            byte[] hashEsperado = Convert.FromBase64String(hashAlmacenado);
            byte[] hashCalculado = Calcular(texto, sal);

            return CryptographicOperations.FixedTimeEquals(hashCalculado, hashEsperado);
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException or CryptographicException)
        {
            return false;
        }
    }

    public bool Verificar(
        string? texto,
        ResultadoHash? hashAlmacenado)
    {
        if (hashAlmacenado is null)
        {
            return false;
        }

        return Verificar(texto, hashAlmacenado.Hash, hashAlmacenado.Sal);
    }

    private static byte[] Calcular(
        string texto,
        byte[] sal)
    {
        return Rfc2898DeriveBytes.Pbkdf2(
            texto,
            sal,
            NumeroIteraciones,
            HashAlgorithmName.SHA256,
            TamanoHash);
    }
}

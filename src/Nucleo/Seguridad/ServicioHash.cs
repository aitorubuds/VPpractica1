using Nucleo.Modelos;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Nucleo.Seguridad;

public static class ServicioHash
{
    private const int TamanoSal = 16;
    private const int TamanoHash = 32;
    private const int NumeroIteraciones = 100_000;

    public static ResultadoHash Generar(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            throw new ArgumentException("El texto no puede estar vacío.", nameof(texto));
        }

        byte[] sal = RandomNumberGenerator.GetBytes(TamanoSal);
        byte[] hash = Calcular(texto, sal);

        return new ResultadoHash
        {
            Hash = Convert.ToBase64String(hash),
            Sal = Convert.ToBase64String(sal)
        };
    }

    public static bool Verificar(
        string texto,
        string hashAlmacenado,
        string salAlmacenada)
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
        catch (FormatException)
        {
            return false;
        }
    }

    public static bool Verificar(
        string texto,
        ResultadoHash hashAlmacenado)
    {
        if (string.IsNullOrWhiteSpace(texto) ||
            string.IsNullOrWhiteSpace(hashAlmacenado.Hash) ||
            string.IsNullOrWhiteSpace(hashAlmacenado.Sal))
        {
            return false;
        }

        try
        {
            byte[] sal = Convert.FromBase64String(hashAlmacenado.Sal);
            byte[] hashEsperado = Convert.FromBase64String(hashAlmacenado.Hash);
            byte[] hashCalculado = Calcular(texto, sal);

            return CryptographicOperations.FixedTimeEquals(hashCalculado, hashEsperado);
        }
        catch (FormatException)
        {
            return false;
        }
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

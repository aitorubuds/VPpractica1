using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nucleo.Seguridad;
using System;
using System.Security.Cryptography;

namespace Nucleo.Tests.Seguridad;

// Pruebas de caja blanca de ServicioCifrado (AES-GCM), mirando cada rama del código
[TestClass]
public class ServicioCifradoTests
{
    private string _clave = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _clave = ServicioCifrado.GenerarClave();
    }

    #region GenerarClave

    // La clave es Base64 de 32 bytes (AES-256)
    [TestMethod]
    public void GenerarClave_Devuelve32BytesEnBase64()
    {
        byte[] bytes = Convert.FromBase64String(_clave);

        Assert.HasCount(32, bytes);
    }

    // Cada llamada da una clave distinta
    [TestMethod]
    public void GenerarClave_ClavesDiferentes()
    {
        Assert.AreNotEqual(ServicioCifrado.GenerarClave(), ServicioCifrado.GenerarClave());
    }

    #endregion

    #region Cifrar y Descifrar

    // Lo que se cifra se recupera igual al descifrar
    [TestMethod]
    [DataRow("12345678Z")]
    [DataRow("a")]
    [DataRow("Teléfono: 600 123 456 ñ€")]
    public void CifrarDescifrar_RecuperaTextoOriginal(string texto)
    {
        string cifrado = ServicioCifrado.Cifrar(texto, _clave);

        Assert.AreNotEqual(texto, cifrado);
        Assert.AreEqual(texto, ServicioCifrado.Descifrar(cifrado, _clave));
    }

    // El mismo texto cifrado dos veces da resultados distintos (nonce aleatorio)
    [TestMethod]
    public void Cifrar_MismoTexto_ResultadosDiferentes()
    {
        string cifrado1 = ServicioCifrado.Cifrar("12345678Z", _clave);
        string cifrado2 = ServicioCifrado.Cifrar("12345678Z", _clave);

        Assert.AreNotEqual(cifrado1, cifrado2);
    }

    // Texto nulo, vacío o con espacios -> ArgumentException
    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void Cifrar_TextoNoValido_LanzaExcepcion(string? texto)
    {
        Assert.ThrowsExactly<ArgumentException>(() => ServicioCifrado.Cifrar(texto!, _clave));
    }

    // Clave nula, vacía, no Base64 o de tamaño incorrecto -> ArgumentException
    [TestMethod]
    [DataRow(null, DisplayName = "Clave nula")]
    [DataRow("", DisplayName = "Clave vacía")]
    [DataRow("no-es-base64!!!", DisplayName = "Clave no Base64")]
    [DataRow("AAAAAAAAAAAAAAAAAAAAAA==", DisplayName = "Clave de 16 bytes")]
    public void Cifrar_ClaveNoValida_LanzaExcepcion(string? clave)
    {
        Assert.ThrowsExactly<ArgumentException>(() => ServicioCifrado.Cifrar("12345678Z", clave!));
    }

    // Contenido nulo, vacío o con espacios -> ArgumentException
    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void Descifrar_ContenidoVacio_LanzaExcepcion(string? contenido)
    {
        Assert.ThrowsExactly<ArgumentException>(() => ServicioCifrado.Descifrar(contenido!, _clave));
    }

    // Contenido demasiado corto (solo nonce + etiqueta o menos) -> ArgumentException
    [TestMethod]
    public void Descifrar_ContenidoCorto_LanzaExcepcion()
    {
        string corto = Convert.ToBase64String(new byte[28]);

        Assert.ThrowsExactly<ArgumentException>(() => ServicioCifrado.Descifrar(corto, _clave));
    }

    // Con otra clave no se puede descifrar
    [TestMethod]
    public void Descifrar_OtraClave_LanzaCryptographicException()
    {
        string cifrado = ServicioCifrado.Cifrar("12345678Z", _clave);
        string otraClave = ServicioCifrado.GenerarClave();

        Assert.Throws<CryptographicException>(() => ServicioCifrado.Descifrar(cifrado, otraClave));
    }

    // Si se modifica un byte del contenido cifrado se detecta (integridad de AES-GCM)
    [TestMethod]
    public void Descifrar_ContenidoManipulado_LanzaCryptographicException()
    {
        byte[] bytes = Convert.FromBase64String(ServicioCifrado.Cifrar("12345678Z", _clave));
        bytes[^1] ^= 0xFF;
        string manipulado = Convert.ToBase64String(bytes);

        Assert.Throws<CryptographicException>(() => ServicioCifrado.Descifrar(manipulado, _clave));
    }

    // Con una clave no válida tampoco se descifra
    [TestMethod]
    public void Descifrar_ClaveNoValida_LanzaExcepcion()
    {
        string cifrado = ServicioCifrado.Cifrar("12345678Z", _clave);

        Assert.ThrowsExactly<ArgumentException>(() => ServicioCifrado.Descifrar(cifrado, "corta"));
    }

    #endregion
}

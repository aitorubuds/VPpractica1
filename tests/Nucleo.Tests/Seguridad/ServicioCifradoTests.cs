using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nucleo.Seguridad;
using System;
using System.Security.Cryptography;

namespace Nucleo.Tests.Seguridad;

// Pruebas de caja blanca de ServicioCifrado, probando todos los casos del código
[TestClass]
public class ServicioCifradoTests
{
    private string _clave = null!;

    // Generamos una clave nueva antes de cada test
    [TestInitialize]
    public void Inicializar()
    {
        _clave = ServicioCifrado.GenerarClave();
    }

    // La clave tiene que ser Base64 y de 32 bytes
    [TestMethod]
    public void GenerarClave_Devuelve32BytesEnBase64()
    {
        byte[] bytes = Convert.FromBase64String(_clave);

        Assert.AreEqual(32, bytes.Length);
    }

    // Cada llamada da una clave distinta
    [TestMethod]
    public void GenerarClave_ClavesDiferentes()
    {
        string clave1 = ServicioCifrado.GenerarClave();
        string clave2 = ServicioCifrado.GenerarClave();

        Assert.AreNotEqual(clave1, clave2);
    }

    // Un DNI cifrado se recupera igual al descifrar
    [TestMethod]
    public void CifrarDescifrar_Dni()
    {
        string cifrado = ServicioCifrado.Cifrar("12345678Z", _clave);

        string descifrado = ServicioCifrado.Descifrar(cifrado, _clave);

        Assert.AreNotEqual("12345678Z", cifrado);
        Assert.AreEqual("12345678Z", descifrado);
    }

    // Un texto de una sola letra se recupera igual al descifrar
    [TestMethod]
    public void CifrarDescifrar_UnaLetra()
    {
        string cifrado = ServicioCifrado.Cifrar("a", _clave);

        string descifrado = ServicioCifrado.Descifrar(cifrado, _clave);

        Assert.AreNotEqual("a", cifrado);
        Assert.AreEqual("a", descifrado);
    }

    // Un texto con espacios, tildes y símbolos se recupera igual al descifrar
    [TestMethod]
    public void CifrarDescifrar_TextoConSimbolos()
    {
        string cifrado = ServicioCifrado.Cifrar("Teléfono: 600 123 456 ñ€", _clave);

        string descifrado = ServicioCifrado.Descifrar(cifrado, _clave);

        Assert.AreNotEqual("Teléfono: 600 123 456 ñ€", cifrado);
        Assert.AreEqual("Teléfono: 600 123 456 ñ€", descifrado);
    }

    // Si ciframos dos veces el mismo texto salen resultados distintos
    [TestMethod]
    public void Cifrar_MismoTexto_ResultadosDiferentes()
    {
        string cifrado1 = ServicioCifrado.Cifrar("12345678Z", _clave);
        string cifrado2 = ServicioCifrado.Cifrar("12345678Z", _clave);

        Assert.AreNotEqual(cifrado1, cifrado2);
    }

    // Cifrar un texto nulo lanza ArgumentException
    [TestMethod]
    public void Cifrar_TextoNulo()
    {
        Assert.ThrowsExactly<ArgumentException>(() => ServicioCifrado.Cifrar(null!, _clave));
    }

    // Cifrar un texto vacío lanza ArgumentException
    [TestMethod]
    public void Cifrar_TextoVacio()
    {
        Assert.ThrowsExactly<ArgumentException>(() => ServicioCifrado.Cifrar("", _clave));
    }

    // Cifrar un texto con solo espacios lanza ArgumentException
    [TestMethod]
    public void Cifrar_TextoEspacios()
    {
        Assert.ThrowsExactly<ArgumentException>(() => ServicioCifrado.Cifrar("   ", _clave));
    }

    // Cifrar con una clave nula lanza ArgumentException
    [TestMethod]
    public void Cifrar_ClaveNula()
    {
        Assert.ThrowsExactly<ArgumentException>(() => ServicioCifrado.Cifrar("12345678Z", null!));
    }

    // Cifrar con una clave vacía lanza ArgumentException
    [TestMethod]
    public void Cifrar_ClaveVacia()
    {
        Assert.ThrowsExactly<ArgumentException>(() => ServicioCifrado.Cifrar("12345678Z", ""));
    }

    // Cifrar con una clave que no es Base64 lanza ArgumentException
    [TestMethod]
    public void Cifrar_ClaveNoBase64()
    {
        Assert.ThrowsExactly<ArgumentException>(() => ServicioCifrado.Cifrar("12345678Z", "no-es-base64!!!"));
    }

    // Cifrar con una clave de 16 bytes (tamaño incorrecto) lanza ArgumentException
    [TestMethod]
    public void Cifrar_Clave16Bytes()
    {
        Assert.ThrowsExactly<ArgumentException>(() => ServicioCifrado.Cifrar("12345678Z", "AAAAAAAAAAAAAAAAAAAAAA=="));
    }

    // Descifrar un contenido nulo lanza ArgumentException
    [TestMethod]
    public void Descifrar_ContenidoNulo()
    {
        Assert.ThrowsExactly<ArgumentException>(() => ServicioCifrado.Descifrar(null!, _clave));
    }

    // Descifrar un contenido vacío lanza ArgumentException
    [TestMethod]
    public void Descifrar_ContenidoVacio()
    {
        Assert.ThrowsExactly<ArgumentException>(() => ServicioCifrado.Descifrar("", _clave));
    }

    // Descifrar un contenido con solo espacios lanza ArgumentException
    [TestMethod]
    public void Descifrar_ContenidoEspacios()
    {
        Assert.ThrowsExactly<ArgumentException>(() => ServicioCifrado.Descifrar("   ", _clave));
    }

    // Si lo cifrado es demasiado corto lanza ArgumentException
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

    // Si cambiamos un byte de lo cifrado ya no se puede descifrar
    [TestMethod]
    public void Descifrar_ContenidoManipulado_LanzaCryptographicException()
    {
        // Ciframos y cambiamos el último byte
        byte[] bytes = Convert.FromBase64String(ServicioCifrado.Cifrar("12345678Z", _clave));
        bytes[bytes.Length - 1] ^= 0xFF;
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
}

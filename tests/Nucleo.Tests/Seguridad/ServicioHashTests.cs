using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nucleo.Seguridad;
using System;

namespace Nucleo.Tests.Seguridad;

[TestClass]
public class ServicioHashTests
{
    private ServicioHash _servicio = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _servicio = new ServicioHash();
    }

    // Generar con un texto normal debe devolver hash y sal válidos
    [TestMethod]
    public void Generar_TextoValido()
    {
        string texto = "MiPassword123";

        ResultadoHash resultado = _servicio.Generar(texto);

        Assert.IsNotNull(resultado);
        Assert.IsFalse(string.IsNullOrEmpty(resultado.Hash));
        Assert.IsFalse(string.IsNullOrEmpty(resultado.Sal));
    }

    // Si el texto es null debe saltar excepción
    [TestMethod]
    public void Generar_TextoNulo()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => _servicio.Generar(null));
    }

    // Si el texto está vacío debe saltar excepción
    [TestMethod]
    public void Generar_TextoVacio()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => _servicio.Generar(""));
    }

    // Si solo son espacios en blanco también debe saltar excepción
    [TestMethod]
    public void Generar_SoloEspacios()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => _servicio.Generar("   "));
    }

    // Si generamos dos veces con el mismo texto, las sales y hashes tienen que ser distintos
    [TestMethod]
    public void Generar_SalesDiferentes()
    {
        string texto = "MismaContrasena";

        ResultadoHash resultado1 = _servicio.Generar(texto);
        ResultadoHash resultado2 = _servicio.Generar(texto);

        // Comprobamos que no se repitan
        Assert.AreNotEqual(resultado1.Sal, resultado2.Sal);
        Assert.AreNotEqual(resultado1.Hash, resultado2.Hash);
    }

    // Verificar con el texto, hash y sal correctos
    [TestMethod]
    public void Verificar_Correcto()
    {
        string texto = "ClaveSecreta123";
        ResultadoHash datos = _servicio.Generar(texto);

        bool valido = _servicio.Verificar(texto, datos.Hash, datos.Sal);

        Assert.IsTrue(valido);
    }

    // Verificar cuando la contraseña no coincide
    [TestMethod]
    public void Verificar_TextoIncorrecto()
    {
        string textoReal = "ClaveCorrecta";
        string textoFalso = "ClaveErronea";
        ResultadoHash datos = _servicio.Generar(textoReal);

        bool valido = _servicio.Verificar(textoFalso, datos.Hash, datos.Sal);

        Assert.IsFalse(valido);
    }

    // Verificar con parámetros vacíos o nulos debe devolver false
    [TestMethod]
    public void Verificar_ParametrosVacios()
    {
        ResultadoHash datos = _servicio.Generar("CualquierCosa");

        Assert.IsFalse(_servicio.Verificar("", datos.Hash, datos.Sal));
        Assert.IsFalse(_servicio.Verificar("CualquierCosa", "", datos.Sal));
        Assert.IsFalse(_servicio.Verificar("CualquierCosa", datos.Hash, ""));
    }

    // Si el hash o la sal están corruptos no debe petar, solo dar false
    [TestMethod]
    public void Verificar_HashCorrupto()
    {
        string texto = "MiTexto";
        ResultadoHash datos = _servicio.Generar(texto);

        // Le metemos un hash con formato no válido
        bool resultado = _servicio.Verificar(texto, "no-es-un-hash-valido!!!", datos.Sal);

        Assert.IsFalse(resultado);
    }

    // Verificar pasando directamente el objeto ResultadoHash
    [TestMethod]
    public void Verificar_ConObjetoResultadoHash()
    {
        string texto = "ClaveSecreta123";
        ResultadoHash datos = _servicio.Generar(texto);

        // Probamos que funcione con el objeto directo tanto acierto como fallo
        Assert.IsTrue(_servicio.Verificar(texto, datos));
        Assert.IsFalse(_servicio.Verificar("ClaveIncorrecta", datos));
    }
}

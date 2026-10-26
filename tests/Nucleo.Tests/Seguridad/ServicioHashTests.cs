using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nucleo.Seguridad;
using System;
using System.Collections.Generic;

namespace Nucleo.Tests.Seguridad;

// Pruebas de caja negra de ServicioHash
[TestClass]
public class ServicioHashTests
{
    private ServicioHash _servicio = null!;

    // Creamos el servicio antes de cada test
    [TestInitialize]
    public void Inicializar()
    {
        _servicio = new ServicioHash();
    }

    // Generar con texto válido devuelve un ResultadoHash
    [TestMethod]
    [DataRow("MiPassword123")]
    [DataRow("a")]
    [DataRow("texto con espacios")]
    [DataRow("ñáéíóú€@#")]
    public void CP01_Generar_TextoValido_DevuelveResultado(string texto)
    {
        ResultadoHash resultado = _servicio.Generar(texto);

        Assert.IsNotNull(resultado);
    }

    // Generar con null lanza InvalidOperationException
    [TestMethod]
    public void CP02_Generar_TextoNulo_LanzaExcepcion()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => _servicio.Generar(null));
    }

    // Generar con texto vacío o solo espacios también lanza InvalidOperationException
    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    public void CP03_Generar_TextoVacioOEspacios_LanzaExcepcion(string texto)
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => _servicio.Generar(texto));
    }

    // El Hash y la Sal vienen rellenos
    [TestMethod]
    public void CP04_Generar_HashYSalNoVacios()
    {
        ResultadoHash resultado = _servicio.Generar("MiPassword123");

        Assert.IsFalse(string.IsNullOrWhiteSpace(resultado.Hash));
        Assert.IsFalse(string.IsNullOrWhiteSpace(resultado.Sal));
    }

    // Lo que devuelve Generar sirve para Verificar
    [TestMethod]
    public void CP05_Generar_ResultadoValidoParaVerificar()
    {
        ResultadoHash datos = _servicio.Generar("MiPassword123");

        bool resultado = _servicio.Verificar("MiPassword123", datos.Hash, datos.Sal);

        Assert.IsTrue(resultado);
    }

    // Dos llamadas con el mismo texto dan sales distintas
    [TestMethod]
    public void CP06_Generar_MismoTexto_SalesDiferentes()
    {
        ResultadoHash resultado1 = _servicio.Generar("MismaContrasena");
        ResultadoHash resultado2 = _servicio.Generar("MismaContrasena");

        Assert.AreNotEqual(resultado1.Sal, resultado2.Sal);
    }

    // Dos llamadas con el mismo texto dan hashes distintos
    [TestMethod]
    public void CP07_Generar_MismoTexto_HashesDiferentes()
    {
        ResultadoHash resultado1 = _servicio.Generar("MismaContrasena");
        ResultadoHash resultado2 = _servicio.Generar("MismaContrasena");

        Assert.AreNotEqual(resultado1.Hash, resultado2.Hash);
    }

    // En 20 llamadas no se repite ninguna sal
    [TestMethod]
    public void CP08_Generar_VariasLlamadas_SalesUnicas()
    {
        HashSet<string> sales = new HashSet<string>();

        // Generamos 20 veces con el mismo texto
        for (int i = 0; i < 20; i++)
        {
            sales.Add(_servicio.Generar("MismaContrasena").Sal);
        }

        // Si alguna sal se repitiera habría menos de 20
        Assert.AreEqual(20, sales.Count);
    }

    // Verificar el texto correcto con su hash y su sal devuelve true
    [TestMethod]
    [DataRow("ClaveSecreta123")]
    [DataRow("a")]
    [DataRow("ñáéíóú€@#")]
    public void CP09_Verificar_TextoCorrecto_DevuelveTrue(string texto)
    {
        ResultadoHash datos = _servicio.Generar(texto);

        bool resultado = _servicio.Verificar(texto, datos.Hash, datos.Sal);

        Assert.IsTrue(resultado);
    }

    // Verificar un texto distinto al original devuelve false
    [TestMethod]
    [DataRow("ClaveCorrecta", "ClaveErronea")]
    [DataRow("ClaveCorrecta", "clavecorrecta")]
    [DataRow("ClaveCorrecta", "ClaveCorrecta ")]
    [DataRow("ClaveCorrecta", "ClaveCorrect")]
    public void CP10_Verificar_TextoIncorrecto_DevuelveFalse(string textoReal, string textoProbado)
    {
        ResultadoHash datos = _servicio.Generar(textoReal);

        bool resultado = _servicio.Verificar(textoProbado, datos.Hash, datos.Sal);

        Assert.IsFalse(resultado);
    }

    // Verificar con el hash de otro texto devuelve false
    [TestMethod]
    public void CP11_Verificar_HashIncorrecto_DevuelveFalse()
    {
        ResultadoHash datos = _servicio.Generar("ClaveCorrecta");
        ResultadoHash otro = _servicio.Generar("OtraClave");

        // Texto y sal buenos, pero el hash es del otro
        bool resultado = _servicio.Verificar("ClaveCorrecta", otro.Hash, datos.Sal);

        Assert.IsFalse(resultado);
    }

    // Verificar con la sal de otra generación devuelve false
    [TestMethod]
    public void CP12_Verificar_SalIncorrecta_DevuelveFalse()
    {
        ResultadoHash datos = _servicio.Generar("ClaveCorrecta");
        ResultadoHash otro = _servicio.Generar("ClaveCorrecta");

        // Texto y hash buenos, pero la sal es de la otra generación
        bool resultado = _servicio.Verificar("ClaveCorrecta", datos.Hash, otro.Sal);

        Assert.IsFalse(resultado);
    }

    // Si algún parámetro es nulo, vacío o con espacios devuelve false
    // posicion dice cuál estropeamos: 0 el texto, 1 el hash y 2 la sal
    [TestMethod]
    [DataRow(0, null, DisplayName = "CP-13 texto nulo")]
    [DataRow(1, null, DisplayName = "CP-13 hash nulo")]
    [DataRow(2, null, DisplayName = "CP-13 sal nula")]
    [DataRow(0, "", DisplayName = "CP-14 texto vacío")]
    [DataRow(1, "", DisplayName = "CP-14 hash vacío")]
    [DataRow(2, "", DisplayName = "CP-14 sal vacía")]
    [DataRow(0, "   ", DisplayName = "CP-15 texto con espacios")]
    [DataRow(1, "   ", DisplayName = "CP-15 hash con espacios")]
    [DataRow(2, "   ", DisplayName = "CP-15 sal con espacios")]
    public void CP13_15_Verificar_ParametroNoValido_DevuelveFalse(int posicion, string? valor)
    {
        string texto = "ClaveSecreta123";
        ResultadoHash datos = _servicio.Generar(texto);

        // Partimos de los tres parámetros buenos y estropeamos uno
        string?[] parametros = { texto, datos.Hash, datos.Sal };
        parametros[posicion] = valor;

        bool resultado = _servicio.Verificar(parametros[0], parametros[1], parametros[2]);

        Assert.IsFalse(resultado);
    }

    // Un hash con formato no válido devuelve false sin excepción
    [TestMethod]
    [DataRow("no-es-un-hash-valido!!!")]
    [DataRow("abc")]
    [DataRow("====")]
    public void CP16_Verificar_HashFormatoNoValido_DevuelveFalse(string hash)
    {
        ResultadoHash datos = _servicio.Generar("MiTexto");

        bool resultado = _servicio.Verificar("MiTexto", hash, datos.Sal);

        Assert.IsFalse(resultado);
    }

    // Una sal con formato no válido devuelve false sin excepción
    [TestMethod]
    [DataRow("no-es-una-sal-valida!!!")]
    [DataRow("abc")]
    [DataRow("====")]
    public void CP17_Verificar_SalFormatoNoValido_DevuelveFalse(string sal)
    {
        ResultadoHash datos = _servicio.Generar("MiTexto");

        bool resultado = _servicio.Verificar("MiTexto", datos.Hash, sal);

        Assert.IsFalse(resultado);
    }

    // Si el Base64 es correcto pero no tiene el tamaño bueno devuelve false sin excepción
    [TestMethod]
    [DataRow("AAAA", "AAAA")]
    [DataRow("AA==", "AA==")]
    [DataRow("QUJD", null)]
    public void CP18_Verificar_DatosAlmacenadosIncorrectos_NoLanzaExcepcion(string hash, string? sal)
    {
        ResultadoHash datos = _servicio.Generar("MiTexto");

        // Si no nos pasan sal usamos la buena
        bool resultado = _servicio.Verificar("MiTexto", hash, sal ?? datos.Sal);

        Assert.IsFalse(resultado);
    }

    // Sobrecarga de Verificar con el objeto ResultadoHash
    [TestMethod]
    public void Extra_Verificar_ConObjetoResultadoHash()
    {
        ResultadoHash datos = _servicio.Generar("ClaveSecreta123");

        bool correcta = _servicio.Verificar("ClaveSecreta123", datos);
        bool incorrecta = _servicio.Verificar("ClaveIncorrecta", datos);
        // Con null no debe romper
        bool sinDatos = _servicio.Verificar("ClaveSecreta123", (ResultadoHash?)null);

        Assert.IsTrue(correcta);
        Assert.IsFalse(incorrecta);
        Assert.IsFalse(sinDatos);
    }
}

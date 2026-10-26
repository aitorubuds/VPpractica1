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

    [TestInitialize]
    public void Inicializar()
    {
        _servicio = new ServicioHash();
    }

    #region Generar

    //con texto válido devuelve un ResultadoHash
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

    //  con null salta InvalidOperationException
    [TestMethod]
    public void CP02_Generar_TextoNulo_LanzaExcepcion()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => _servicio.Generar(null));
    }

    // vacío o solo espacios también salta InvalidOperationException
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

    //Hash y Sal vienen rellenos
    [TestMethod]
    public void CP04_Generar_HashYSalNoVacios()
    {
        ResultadoHash resultado = _servicio.Generar("MiPassword123");

        Assert.IsFalse(string.IsNullOrWhiteSpace(resultado.Hash));
        Assert.IsFalse(string.IsNullOrWhiteSpace(resultado.Sal));
    }

    //  lo que devuelve Generar sirve para Verificar
    [TestMethod]
    public void CP05_Generar_ResultadoValidoParaVerificar()
    {
        ResultadoHash resultado = _servicio.Generar("MiPassword123");

        Assert.IsTrue(_servicio.Verificar("MiPassword123", resultado.Hash, resultado.Sal));
    }

    //  dos llamadas con el mismo texto dan sales distintas
    [TestMethod]
    public void CP06_Generar_MismoTexto_SalesDiferentes()
    {
        ResultadoHash resultado1 = _servicio.Generar("MismaContrasena");
        ResultadoHash resultado2 = _servicio.Generar("MismaContrasena");

        Assert.AreNotEqual(resultado1.Sal, resultado2.Sal);
    }

    //  dos llamadas con el mismo texto dan hashes distintos
    [TestMethod]
    public void CP07_Generar_MismoTexto_HashesDiferentes()
    {
        ResultadoHash resultado1 = _servicio.Generar("MismaContrasena");
        ResultadoHash resultado2 = _servicio.Generar("MismaContrasena");

        Assert.AreNotEqual(resultado1.Hash, resultado2.Hash);
    }

    // en 20 llamadas no se repite ninguna sal
    [TestMethod]
    public void CP08_Generar_VariasLlamadas_SalesUnicas()
    {
        HashSet<string> sales = new HashSet<string>();

        for (int i = 0; i < 20; i++)
        {
            sales.Add(_servicio.Generar("MismaContrasena").Sal);
        }

        Assert.HasCount(20, sales);
    }

    #endregion

    #region Verificar

    //texto correcto con su hash y sal -> true
    [TestMethod]
    [DataRow("ClaveSecreta123")]
    [DataRow("a")]
    [DataRow("ñáéíóú€@#")]
    public void CP09_Verificar_TextoCorrecto_DevuelveTrue(string texto)
    {
        ResultadoHash datos = _servicio.Generar(texto);

        Assert.IsTrue(_servicio.Verificar(texto, datos.Hash, datos.Sal));
    }

    //texto distinto al original -> false
    [TestMethod]
    [DataRow("ClaveCorrecta", "ClaveErronea")]
    [DataRow("ClaveCorrecta", "clavecorrecta")]
    [DataRow("ClaveCorrecta", "ClaveCorrecta ")]
    [DataRow("ClaveCorrecta", "ClaveCorrect")]
    public void CP10_Verificar_TextoIncorrecto_DevuelveFalse(string textoReal, string textoProbado)
    {
        ResultadoHash datos = _servicio.Generar(textoReal);

        Assert.IsFalse(_servicio.Verificar(textoProbado, datos.Hash, datos.Sal));
    }

    //hash de otro texto -> false
    [TestMethod]
    public void CP11_Verificar_HashIncorrecto_DevuelveFalse()
    {
        ResultadoHash datos = _servicio.Generar("ClaveCorrecta");
        ResultadoHash otro = _servicio.Generar("OtraClave");

        Assert.IsFalse(_servicio.Verificar("ClaveCorrecta", otro.Hash, datos.Sal));
    }

    //sal de otra generación -> false
    [TestMethod]
    public void CP12_Verificar_SalIncorrecta_DevuelveFalse()
    {
        ResultadoHash datos = _servicio.Generar("ClaveCorrecta");
        ResultadoHash otro = _servicio.Generar("ClaveCorrecta");

        Assert.IsFalse(_servicio.Verificar("ClaveCorrecta", datos.Hash, otro.Sal));
    }

    // algún parámetro nulo, vacío o con espacios -> false
    // posicion: 0 = texto, 1 = hash, 2 = sal
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
        string?[] parametros = { texto, datos.Hash, datos.Sal };
        parametros[posicion] = valor;

        bool resultado = _servicio.Verificar(parametros[0], parametros[1], parametros[2]);

        Assert.IsFalse(resultado);
    }

    //hash con formato no válido -> false sin excepción
    [TestMethod]
    [DataRow("no-es-un-hash-valido!!!")]
    [DataRow("abc")]
    [DataRow("====")]
    public void CP16_Verificar_HashFormatoNoValido_DevuelveFalse(string hash)
    {
        ResultadoHash datos = _servicio.Generar("MiTexto");

        Assert.IsFalse(_servicio.Verificar("MiTexto", hash, datos.Sal));
    }

    //sal con formato no válido -> false sin excepción
    [TestMethod]
    [DataRow("no-es-una-sal-valida!!!")]
    [DataRow("abc")]
    [DataRow("====")]
    public void CP17_Verificar_SalFormatoNoValido_DevuelveFalse(string sal)
    {
        ResultadoHash datos = _servicio.Generar("MiTexto");

        Assert.IsFalse(_servicio.Verificar("MiTexto", datos.Hash, sal));
    }

    // Base64 correcto pero de tamaño que no toca -> false sin excepción
    [TestMethod]
    [DataRow("AAAA", "AAAA")]
    [DataRow("AA==", "AA==")]
    [DataRow("QUJD", null)]
    public void CP18_Verificar_DatosAlmacenadosIncorrectos_NoLanzaExcepcion(string hash, string? sal)
    {
        ResultadoHash datos = _servicio.Generar("MiTexto");

        bool resultado = _servicio.Verificar("MiTexto", hash, sal ?? datos.Sal);

        Assert.IsFalse(resultado);
    }

    //sobrecarga con el objeto ResultadoHash
    [TestMethod]
    public void Extra_Verificar_ConObjetoResultadoHash()
    {
        ResultadoHash datos = _servicio.Generar("ClaveSecreta123");

        Assert.IsTrue(_servicio.Verificar("ClaveSecreta123", datos));
        Assert.IsFalse(_servicio.Verificar("ClaveIncorrecta", datos));
        Assert.IsFalse(_servicio.Verificar("ClaveSecreta123", (ResultadoHash?)null));
    }

    #endregion
}

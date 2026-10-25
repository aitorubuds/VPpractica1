using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nucleo.Seguridad;

namespace Nucleo.Tests.Seguridad;

// Pruebas de caja negra de ResultadoHash según el E01 (ED-18 a ED-20)
[TestClass]
public class ResultadoHashTests
{
    // CP-19 (ED-18, ED-19): tiene las propiedades Hash y Sal y guardan lo que se les da
    [TestMethod]
    public void CP19_ResultadoHash_GuardaHashYSal()
    {
        ResultadoHash resultado = new ResultadoHash { Hash = "hash-de-prueba", Sal = "sal-de-prueba" };

        Assert.AreEqual("hash-de-prueba", resultado.Hash);
        Assert.AreEqual("sal-de-prueba", resultado.Sal);
    }

    // CP-20 (ED-20): todo ResultadoHash que devuelve el servicio tiene Hash y Sal no nulos ni vacíos
    [TestMethod]
    [DataRow("MiPassword123")]
    [DataRow("a")]
    [DataRow("texto con espacios")]
    [DataRow("ñáéíóú€@#")]
    public void CP20_ResultadoHash_DelServicio_SinValoresVacios(string texto)
    {
        ResultadoHash resultado = new ServicioHash().Generar(texto);

        Assert.IsNotNull(resultado.Hash);
        Assert.IsNotNull(resultado.Sal);
        Assert.AreNotEqual(string.Empty, resultado.Hash);
        Assert.AreNotEqual(string.Empty, resultado.Sal);
    }
}

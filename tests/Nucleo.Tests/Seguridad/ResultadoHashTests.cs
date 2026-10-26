using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nucleo.Seguridad;

namespace Nucleo.Tests.Seguridad;

// Pruebas de caja negra de ResultadoHash
[TestClass]
public class ResultadoHashTests
{
    //tiene las propiedades Hash y Sal y guardan lo que se les da
    [TestMethod]
    public void CP19_ResultadoHash_GuardaHashYSal()
    {
        ResultadoHash resultado = new ResultadoHash { Hash = "hash-de-prueba", Sal = "sal-de-prueba" };

        Assert.AreEqual("hash-de-prueba", resultado.Hash);
        Assert.AreEqual("sal-de-prueba", resultado.Sal);
    }

    //todo ResultadoHash que devuelve el servicio tiene Hash y Sal no nulos ni vacíos
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

    #region Caja blanca: igualdad (no está en el E01, pero la clase lo implementa)

    private static ResultadoHash Crear(string hash, string sal) => new ResultadoHash { Hash = hash, Sal = sal };

    // Mismo Hash y Sal -> iguales, con el mismo GetHashCode
    [TestMethod]
    public void Igualdad_MismosValores_SonIguales()
    {
        ResultadoHash a = Crear("h", "s");
        ResultadoHash b = Crear("h", "s");

        Assert.IsTrue(a.Equals(b));
        Assert.IsTrue(a.Equals((object)b));
        Assert.IsTrue(a == b);
        Assert.IsFalse(a != b);
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }

    // Cambia el Hash o la Sal -> distintos
    [TestMethod]
    [DataRow("otro", "s")]
    [DataRow("h", "otra")]
    public void Igualdad_ValoresDistintos_NoSonIguales(string hash, string sal)
    {
        ResultadoHash a = Crear("h", "s");
        ResultadoHash b = Crear(hash, sal);

        Assert.IsFalse(a.Equals(b));
        Assert.IsFalse(a == b);
        Assert.IsTrue(a != b);
    }

    // Comparar consigo mismo, con null o con otro tipo
    [TestMethod]
    public void Igualdad_CasosLimite()
    {
        ResultadoHash a = Crear("h", "s");
        ResultadoHash? nulo = null;

        Assert.IsTrue(a.Equals(a));
        Assert.IsFalse(a.Equals(nulo));
        Assert.IsFalse(a.Equals("h"));
        Assert.IsFalse(a == nulo);
        Assert.IsFalse(nulo == a);
        Assert.IsTrue(nulo == null);
        Assert.IsTrue(a != nulo);
    }

    #endregion
}

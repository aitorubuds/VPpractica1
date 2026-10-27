using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nucleo.Seguridad;

namespace Nucleo.Tests.Seguridad;

// Pruebas de caja negra de ResultadoHash
[TestClass]
public class ResultadoHashTests
{
    // Tiene las propiedades Hash y Sal y guardan lo que se les da
    [TestMethod]
    public void CP19_ResultadoHash_GuardaHashYSal()
    {
        ResultadoHash resultado = new ResultadoHash { Hash = "hash-de-prueba", Sal = "sal-de-prueba" };

        Assert.AreEqual("hash-de-prueba", resultado.Hash);
        Assert.AreEqual("sal-de-prueba", resultado.Sal);
    }

    // Todo ResultadoHash que devuelve el servicio tiene Hash y Sal no nulos ni vacíos
    [TestMethod]
    [DataRow("MiPassword123")]
    [DataRow("a")]
    [DataRow("texto con espacios")]
    [DataRow("ñáéíóú€@#")]
    public void CP20_ResultadoHash_DelServicio_SinValoresVacios(string texto)
    {
        ServicioHash servicio = new ServicioHash();

        ResultadoHash resultado = servicio.Generar(texto);

        Assert.IsNotNull(resultado.Hash);
        Assert.IsNotNull(resultado.Sal);
        Assert.AreNotEqual(string.Empty, resultado.Hash);
        Assert.AreNotEqual(string.Empty, resultado.Sal);
    }

    // Pruebas de la igualdad entre dos ResultadoHash

    // Función auxiliar para crear un ResultadoHash con el hash y la sal que digamos
    private static ResultadoHash Crear(string hash, string sal)
    {
        return new ResultadoHash { Hash = hash, Sal = sal };
    }

    // Con el mismo Hash y la misma Sal son iguales y tienen el mismo GetHashCode
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

    // Si cambia el Hash ya no son iguales
    [TestMethod]
    public void Igualdad_HashDistinto()
    {
        ResultadoHash a = Crear("h", "s");
        ResultadoHash b = Crear("otro", "s");

        Assert.IsFalse(a.Equals(b));
        Assert.IsFalse(a == b);
        Assert.IsTrue(a != b);
    }

    // Si cambia la Sal ya no son iguales
    [TestMethod]
    public void Igualdad_SalDistinta()
    {
        ResultadoHash a = Crear("h", "s");
        ResultadoHash b = Crear("h", "otra");

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
}

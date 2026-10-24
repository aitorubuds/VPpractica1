using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nucleo.Seguridad;

namespace Nucleo.Tests.Seguridad;

[TestClass]
public class ServicioContrasenasTests
{
    // Test 1: Contraseña que cumple todos los requisitos
    [TestMethod]
    public void ContrasenaValida()
    {
        // Tiene más de 8 caracteres, mayúscula, minúscula, número y símbolo
        string clave = "ClaveSegura123!";

        bool resultado = ServicioContrasenas.EsValida(clave);

        Assert.IsTrue(resultado);
    }

    // Test 2: Menos de 8 caracteres
    [TestMethod]
    public void ContrasenaCorta()
    {
        // Solo tiene 5 caracteres
        string clave = "Ab1!x";

        bool resultado = ServicioContrasenas.EsValida(clave);

        Assert.IsFalse(resultado);
    }

    // Test 3: Sin ninguna letra mayúscula
    [TestMethod]
    public void SinMayuscula()
    {
        string clave = "clavesegura123!";

        bool resultado = ServicioContrasenas.EsValida(clave);

        Assert.IsFalse(resultado);
    }

    // Test 4: Sin ninguna letra minúscula
    [TestMethod]
    public void SinMinuscula()
    {
        string clave = "CLAVESEGURA123!";

        bool resultado = ServicioContrasenas.EsValida(clave);

        Assert.IsFalse(resultado);
    }

    // Test 5: Sin números
    [TestMethod]
    public void SinNumero()
    {
        string clave = "ClaveSeguraSinNum!";

        bool resultado = ServicioContrasenas.EsValida(clave);

        Assert.IsFalse(resultado);
    }

    // Test 6: Sin ningún símbolo o carácter especial
    [TestMethod]
    public void SinCaracterEspecial()
    {
        string clave = "ClaveSegura123";

        bool resultado = ServicioContrasenas.EsValida(clave);

        Assert.IsFalse(resultado);
    }

    // Test 7: Cadena nula o vacía
    [TestMethod]
    public void NulaOVacia()
    {
        bool resultadoNull = ServicioContrasenas.EsValida(null!);
        bool resultadoVacia = ServicioContrasenas.EsValida("");

        Assert.IsFalse(resultadoNull);
        Assert.IsFalse(resultadoVacia);
    }

    // Test 8: Contraseña con solo espacios en blanco
    [TestMethod]
    public void SoloEspacios()
    {
        // Una cadena compuesta solo por espacios en blanco debe rechazarse
        string clave = "   ";

        bool resultado = ServicioContrasenas.EsValida(clave);

        Assert.IsFalse(resultado);
    }
}

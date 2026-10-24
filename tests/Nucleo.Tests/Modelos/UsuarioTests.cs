using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nucleo.Modelos;

namespace Nucleo.Tests.Modelos;

[TestClass]
public class UsuarioTests
{
    // Función auxiliar para crear un usuario de prueba rápido
    private Usuario CrearUsuarioPrueba()
    {
        Usuario u = new Usuario();
        u.Cuenta = "alex";
        u.Nombre = "Alejandro";
        u.EMail = "alex@ubu.es";
        u.EstablecerContrasena("ContraseñaSegura123!");
        return u;
    }

    // Comprobar contraseña con la clave correcta
    [TestMethod]
    public void Comprobar_Correcto()
    {
        Usuario u = CrearUsuarioPrueba();

        bool resultado = u.ComprobarContrasena("ContraseñaSegura123!");

        Assert.IsTrue(resultado);
        Assert.AreEqual(0, u.IntentosFallidos);
    }

    // Comprobar contraseña con una clave errónea suma intento
    [TestMethod]
    public void Comprobar_Incorrecto()
    {
        Usuario u = CrearUsuarioPrueba();

        bool resultado = u.ComprobarContrasena("ClaveMala999!");

        Assert.IsFalse(resultado);
        Assert.AreEqual(1, u.IntentosFallidos);
        Assert.IsFalse(u.Bloqueado);
    }

    // Al fallar 3 veces seguidas la cuenta se bloquea
    [TestMethod]
    public void Bloqueo_TresFallos()
    {
        Usuario u = CrearUsuarioPrueba();

        u.ComprobarContrasena("Fallo1");
        u.ComprobarContrasena("Fallo2");
        u.ComprobarContrasena("Fallo3");

        Assert.AreEqual(3, u.IntentosFallidos);
        Assert.IsTrue(u.Bloqueado);
    }

    // Cuando está bloqueado, aunque pongas la clave buena no te deja pasar
    [TestMethod]
    public void Bloqueo_NoPermiteEntrar()
    {
        Usuario u = CrearUsuarioPrueba();

        // Provocamos el bloqueo
        u.ComprobarContrasena("Fallo1");
        u.ComprobarContrasena("Fallo2");
        u.ComprobarContrasena("Fallo3");

        // Intentamos entrar con la clave real
        bool resultado = u.ComprobarContrasena("ContraseñaSegura123!");

        Assert.IsFalse(resultado);
    }

    // El administrador desbloquea la cuenta y el usuario ya puede entrar
    [TestMethod]
    public void Desbloquear_PermiteEntrar()
    {
        Usuario u = CrearUsuarioPrueba();

        // Bloqueamos
        u.ComprobarContrasena("Fallo1");
        u.ComprobarContrasena("Fallo2");
        u.ComprobarContrasena("Fallo3");
        Assert.IsTrue(u.Bloqueado);

        // Desbloqueamos
        u.Desbloquear();

        Assert.IsFalse(u.Bloqueado);
        Assert.AreEqual(0, u.IntentosFallidos);

        // Ahora debe dejar entrar con la clave buena
        bool resultado = u.ComprobarContrasena("ContraseñaSegura123!");
        Assert.IsTrue(resultado);
    }

    // Cambio de contraseña válido
    [TestMethod]
    public void CambiarContrasena_Correcto()
    {
        Usuario u = CrearUsuarioPrueba();

        bool cambioOk = u.CambiarContrasena("ContraseñaSegura123!", "NuevaClaveSuperSegura456!");

        Assert.IsTrue(cambioOk);
        // Comprobamos que la vieja ya no vale y la nueva sí
        Assert.IsFalse(u.ComprobarContrasena("ContraseñaSegura123!"));
        Assert.IsTrue(u.ComprobarContrasena("NuevaClaveSuperSegura456!"));
    }

    // Si la contraseña actual no es la suya no te deja cambiarla
    [TestMethod]
    public void CambiarContrasena_ViejaIncorrecta()
    {
        Usuario u = CrearUsuarioPrueba();

        bool cambioOk = u.CambiarContrasena("ClaveViejaMala!", "NuevaClaveSuperSegura456!");

        Assert.IsFalse(cambioOk);
    }

    // Si la contraseña nueva no cumple la complejidad (por ejemplo sin número) no deja cambiar
    [TestMethod]
    public void CambiarContrasena_NuevaDebil()
    {
        Usuario u = CrearUsuarioPrueba();

        // "ClaveDebilSinNumero!" no tiene dígitos
        bool cambioOk = u.CambiarContrasena("ContraseñaSegura123!", "ClaveDebilSinNumero!");

        Assert.IsFalse(cambioOk);
    }

    // Si el usuario está bloqueado no puede cambiar contraseña
    [TestMethod]
    public void CambiarContrasena_Bloqueado()
    {
        Usuario u = CrearUsuarioPrueba();
        u.Bloqueado = true;

        bool cambioOk = u.CambiarContrasena("ContraseñaSegura123!", "NuevaClaveSuperSegura456!");

        Assert.IsFalse(cambioOk);
    }
}

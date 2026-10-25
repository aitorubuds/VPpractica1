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

    // Los alias que usa la web del profesor hacen lo mismo que los métodos originales
    [TestMethod]
    public void Alias_FuncionanIgual()
    {
        Usuario u = CrearUsuarioPrueba();
        u.IdUsuario = 7;
        u.Activa = false;
        u.IntentosErroneos = 2;

        Assert.AreEqual(7, u.Id);
        Assert.IsFalse(u.Activo);
        Assert.AreEqual(2, u.IntentosFallidos);

        u.Activa = true;
        Assert.IsTrue(u.CompruebaPassword("ContraseñaSegura123!"));
        Assert.IsTrue(u.CambiaPassword("ContraseñaSegura123!", "NuevaClaveSuperSegura456!"));
        Assert.IsTrue(u.ComprobarContrasena("NuevaClaveSuperSegura456!"));
    }

    // Un usuario inactivo no entra aunque la contraseña sea buena
    [TestMethod]
    public void Comprobar_Inactivo_NoEntra()
    {
        Usuario u = CrearUsuarioPrueba();
        u.Activo = false;

        Assert.IsFalse(u.ComprobarContrasena("ContraseñaSegura123!"));
    }

    // Sin contraseña establecida no tiene credenciales; con ella sí
    [TestMethod]
    public void TieneCredenciales_SoloConContrasena()
    {
        Usuario sinClave = new Usuario { Cuenta = "nueva" };

        Assert.IsFalse(sinClave.TieneCredencialesConfiguradas);
        Assert.IsTrue(CrearUsuarioPrueba().TieneCredencialesConfiguradas);
    }

    // ToString: nombre solo o nombre y apellidos
    [TestMethod]
    [DataRow("Alejandro", "", "Alejandro")]
    [DataRow("Alejandro", "   ", "Alejandro")]
    [DataRow("Alejandro", "García", "Alejandro García")]
    public void ToString_NombreYApellidos(string nombre, string apellidos, string esperado)
    {
        Usuario u = new Usuario { Nombre = nombre, Apellidos = apellidos };

        Assert.AreEqual(esperado, u.ToString());
    }

    // El alias IdProyecto de Proyecto es el mismo que Id
    [TestMethod]
    public void Proyecto_AliasIdProyecto()
    {
        Proyecto p = new Proyecto { IdProyecto = 3 };

        Assert.AreEqual(3, p.Id);
        Assert.AreEqual(3, p.IdProyecto);
    }

    // El teléfono se guarda cifrado y solo se lee con la clave correcta
    [TestMethod]
    public void Telefono_SeGuardaCifrado()
    {
        string clave = Nucleo.Seguridad.ServicioCifrado.GenerarClave();
        Usuario u = CrearUsuarioPrueba();

        u.EstablecerTelefono(" 600123456 ", clave);

        Assert.DoesNotContain("600123456", u.TelefonoCifrado);
        Assert.AreEqual("600123456", u.ObtenerTelefono(clave));
    }

    // Teléfono vacío o con espacios borra el que hubiera
    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void Telefono_Vacio_SeBorra(string telefono)
    {
        string clave = Nucleo.Seguridad.ServicioCifrado.GenerarClave();
        Usuario u = CrearUsuarioPrueba();
        u.EstablecerTelefono("600123456", clave);

        u.EstablecerTelefono(telefono, clave);

        Assert.AreEqual(string.Empty, u.TelefonoCifrado);
        Assert.AreEqual(string.Empty, u.ObtenerTelefono(clave));
    }

    // Un usuario nuevo no es administrador
    [TestMethod]
    public void Usuario_PorDefecto_NoEsAdministrador()
    {
        Assert.IsFalse(new Usuario().EsAdministrador);
    }
}

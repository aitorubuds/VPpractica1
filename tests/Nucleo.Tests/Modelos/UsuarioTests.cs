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

        // Las propiedades alias devuelven lo mismo que las originales
        Assert.AreEqual(7, u.Id);
        Assert.IsFalse(u.Activo);
        Assert.AreEqual(2, u.IntentosFallidos);

        // Reactivamos la cuenta
        u.Activa = true;

        // Los métodos alias comprueban y cambian la contraseña igual que los originales
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

        bool resultado = u.ComprobarContrasena("ContraseñaSegura123!");

        Assert.IsFalse(resultado);
    }

    // Sin contraseña establecida no tiene credenciales
    [TestMethod]
    public void TieneCredenciales_SinContrasena()
    {
        Usuario u = new Usuario();
        u.Cuenta = "nueva";

        Assert.IsFalse(u.TieneCredencialesConfiguradas);
    }

    // Con contraseña establecida sí tiene credenciales
    [TestMethod]
    public void TieneCredenciales_ConContrasena()
    {
        Usuario u = CrearUsuarioPrueba();

        Assert.IsTrue(u.TieneCredencialesConfiguradas);
    }

    // ToString con solo nombre devuelve el nombre
    [TestMethod]
    public void ToString_SoloNombre()
    {
        Usuario u = new Usuario();
        u.Nombre = "Alejandro";
        u.Apellidos = "";

        Assert.AreEqual("Alejandro", u.ToString());
    }

    // ToString con apellidos en blanco devuelve solo el nombre
    [TestMethod]
    public void ToString_ApellidosEnBlanco()
    {
        Usuario u = new Usuario();
        u.Nombre = "Alejandro";
        u.Apellidos = "   ";

        Assert.AreEqual("Alejandro", u.ToString());
    }

    // ToString con nombre y apellidos devuelve ambos
    [TestMethod]
    public void ToString_NombreYApellidos()
    {
        Usuario u = new Usuario();
        u.Nombre = "Alejandro";
        u.Apellidos = "García";

        Assert.AreEqual("Alejandro García", u.ToString());
    }

    // El alias IdProyecto de Proyecto es el mismo que Id
    [TestMethod]
    public void Proyecto_AliasIdProyecto()
    {
        Proyecto p = new Proyecto();
        p.IdProyecto = 3;

        Assert.AreEqual(3, p.Id);
        Assert.AreEqual(3, p.IdProyecto);
    }

    // El teléfono se guarda cifrado y solo se lee con la clave correcta
    [TestMethod]
    public void Telefono_SeGuardaCifrado()
    {
        Usuario u = CrearUsuarioPrueba();
        string clave = Nucleo.Seguridad.ServicioCifrado.GenerarClave();

        // Guardamos el teléfono con espacios alrededor
        u.EstablecerTelefono(" 600123456 ", clave);

        // No se guarda en claro y se recupera sin espacios
        Assert.DoesNotContain("600123456", u.TelefonoCifrado);
        Assert.AreEqual("600123456", u.ObtenerTelefono(clave));
    }

    // Teléfono vacío borra el que hubiera
    [TestMethod]
    public void Telefono_Vacio_SeBorra()
    {
        Usuario u = CrearUsuarioPrueba();
        string clave = Nucleo.Seguridad.ServicioCifrado.GenerarClave();

        // Ponemos un teléfono
        u.EstablecerTelefono("600123456", clave);

        // Lo borramos con un teléfono vacío
        u.EstablecerTelefono("", clave);

        Assert.AreEqual(string.Empty, u.TelefonoCifrado);
        Assert.AreEqual(string.Empty, u.ObtenerTelefono(clave));
    }

    // Teléfono solo con espacios también borra el que hubiera
    [TestMethod]
    public void Telefono_SoloEspacios_SeBorra()
    {
        Usuario u = CrearUsuarioPrueba();
        string clave = Nucleo.Seguridad.ServicioCifrado.GenerarClave();

        // Ponemos un teléfono
        u.EstablecerTelefono("600123456", clave);

        // Lo borramos con un teléfono de solo espacios
        u.EstablecerTelefono("   ", clave);

        Assert.AreEqual(string.Empty, u.TelefonoCifrado);
        Assert.AreEqual(string.Empty, u.ObtenerTelefono(clave));
    }

    // Un usuario nuevo no es administrador
    [TestMethod]
    public void Usuario_PorDefecto_NoEsAdministrador()
    {
        Usuario u = new Usuario();

        Assert.IsFalse(u.EsAdministrador);
    }
}

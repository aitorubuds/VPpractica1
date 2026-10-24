using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nucleo.Modelos;
using Datos;

namespace Nucleo.Tests.Integracion;

[TestClass]
public class IntegracionTests
{
    private global::Datos.Datos _datos = null!;

    // vaciar la bd antes de probar el flujo
    [TestInitialize]
    public void Inicializar()
    {
        _datos = new global::Datos.Datos();
        _datos.Reiniciar();
    }

    // Probamos el flujo completo de registrar un usuario, guardarlo en bd y loguearse
    [TestMethod]
    public void Flujo_RegistroYLogin()
    {
        // 1. Creamos el usuario con su clave segura inicial
        Usuario nuevo = new Usuario();
        nuevo.Cuenta = "alex";
        nuevo.Nombre = "Alejandro";
        nuevo.EMail = "alex@ubu.es";
        nuevo.EstablecerContrasena("MiClaveSegura123!");

        // 2. Lo guardamos en la base de datos
        Usuario? guardado = _datos.GuardarUsuario(nuevo);

        Assert.IsNotNull(guardado);
        Assert.IsTrue(guardado.Id > 0);

        // 3. Lo recuperamos de la base de datos simulando un nuevo request
        Usuario? recuperado = _datos.LeerUsuario(guardado.Id);

        Assert.IsNotNull(recuperado);
        // 4. Comprobamos que el login con su clave funciona
        Assert.IsTrue(recuperado.ComprobarContrasena("MiClaveSegura123!"));
    }

    // Probamos el ciclo de fallo de login, bloqueo, registro en logs y cambio de clave
    [TestMethod]
    public void Flujo_BloqueoYCambioClave()
    {
        // 1. Damos de alta al usuario
        Usuario u = new Usuario { Cuenta = "aitor", Nombre = "Aitor" };
        u.EstablecerContrasena("ClaveSecreta123!");
        _datos.GuardarUsuario(u);

        // 2. Alguien intenta entrar y falla 3 veces
        u.ComprobarContrasena("Mala1");
        u.ComprobarContrasena("Mala2");
        u.ComprobarContrasena("Mala3");

        // Verificamos que se ha bloqueado
        Assert.IsTrue(u.Bloqueado);

        // 3. Registramos en el log que la cuenta ha sido bloqueada
        EntradaLog avisoLog = new EntradaLog
        {
            UsuarioId = u.Id,
            Accion = "Cuenta bloqueada por 3 intentos fallidos"
        };
        _datos.AgregarLog(avisoLog);

        Assert.AreEqual(1, _datos.Logs.Count);

        // 4. El administrador desbloquea la cuenta
        u.Desbloquear();
        Assert.IsFalse(u.Bloqueado);

        // 5. El usuario cambia su contraseña a una nueva
        bool cambioOk = u.CambiarContrasena("ClaveSecreta123!", "NuevaClaveSuperSegura999!");
        Assert.IsTrue(cambioOk);

        // Guardamos los cambios en la base de datos
        _datos.GuardarUsuario(u);

        // 6. Verificamos que ahora solo puede entrar con la contraseña nueva
        Usuario? usuarioActualizado = _datos.LeerUsuario(u.Id);
        Assert.IsNotNull(usuarioActualizado);
        Assert.IsFalse(usuarioActualizado.ComprobarContrasena("ClaveSecreta123!"));
        Assert.IsTrue(usuarioActualizado.ComprobarContrasena("NuevaClaveSuperSegura999!"));
    }
}

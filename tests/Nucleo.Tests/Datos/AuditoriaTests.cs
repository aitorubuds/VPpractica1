using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nucleo.Modelos;
using Datos;
using System;

namespace Nucleo.Tests.Datos;

// Pruebas del registro de accesos en el log
[TestClass]
public class AuditoriaTests
{
    private global::Datos.Datos _datos = null!;

    // Base de datos vacía antes de cada test
    [TestInitialize]
    public void Inicializar()
    {
        _datos = new global::Datos.Datos();
    }

    // Si hay usuario se guardan su Id, su email, el nivel, la acción y el mensaje
    [TestMethod]
    public void Registrar_ConUsuario_GuardaSusDatos()
    {
        Usuario pedro = new Usuario();
        pedro.Cuenta = "pedro";
        pedro.EMail = "pedro@ubu.es";
        _datos.GuardarUsuario(pedro);
        DateTime antes = DateTime.Now;

        EntradaLog log = Auditoria.Registrar(_datos, Auditoria.NivelInfo, Auditoria.LoginCorrecto,
            "Inicio de sesión correcto", pedro);

        Assert.AreEqual(1, _datos.Logs.Count);
        Assert.AreEqual(pedro.Id, log.UsuarioId);
        Assert.AreEqual("pedro@ubu.es", log.NombreUsuario);
        Assert.AreEqual(Auditoria.NivelInfo, log.Nivel);
        Assert.AreEqual(Auditoria.LoginCorrecto, log.Accion);
        Assert.AreEqual("Inicio de sesión correcto", log.Mensaje);
        // La fecha es la de ahora, no una anterior
        Assert.IsTrue(log.FechaHora >= antes);
    }

    // Si el email no existe se guarda UsuarioId 0 y el email que se ha intentado
    [TestMethod]
    public void Registrar_SinUsuario_GuardaEmailIntentado()
    {
        EntradaLog log = Auditoria.Registrar(_datos, Auditoria.NivelAviso, Auditoria.LoginFallido,
            "Email que no existe", null, "nadie@ubu.es");

        Assert.AreEqual(0, log.UsuarioId);
        Assert.AreEqual("nadie@ubu.es", log.NombreUsuario);
    }

    // Cada registro es una entrada nueva con Id consecutivo
    [TestMethod]
    public void Registrar_VariasVeces_IdsConsecutivos()
    {
        EntradaLog log1 = Auditoria.Registrar(_datos, Auditoria.NivelAviso, Auditoria.LoginFallido, "1", null);
        EntradaLog log2 = Auditoria.Registrar(_datos, Auditoria.NivelError, Auditoria.CuentaBloqueada, "2", null);

        Assert.AreEqual(1, log1.Id);
        Assert.AreEqual(2, log2.Id);
        Assert.AreEqual(2, _datos.Logs.Count);
    }
}

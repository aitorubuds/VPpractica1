using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nucleo.Modelos;
using Datos;
using System;

namespace Nucleo.Tests.Datos;

// Registro de auditoría de los accesos (acta del cliente)
[TestClass]
public class AuditoriaTests
{
    private global::Datos.Datos _datos = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _datos = new global::Datos.Datos();
    }

    // Con usuario: guarda su Id, su email, el nivel, la acción y el mensaje
    [TestMethod]
    public void Registrar_ConUsuario_GuardaSusDatos()
    {
        Usuario pedro = _datos.GuardarUsuario(new Usuario { Cuenta = "pedro", EMail = "pedro@ubu.es" })!;
        DateTime antes = DateTime.Now;

        EntradaLog log = Auditoria.Registrar(_datos, Auditoria.NivelInfo, Auditoria.LoginCorrecto,
            "Inicio de sesión correcto", pedro);

        Assert.HasCount(1, _datos.Logs);
        Assert.AreEqual(pedro.Id, log.UsuarioId);
        Assert.AreEqual("pedro@ubu.es", log.NombreUsuario);
        Assert.AreEqual(Auditoria.NivelInfo, log.Nivel);
        Assert.AreEqual(Auditoria.LoginCorrecto, log.Accion);
        Assert.AreEqual("Inicio de sesión correcto", log.Mensaje);
        Assert.IsGreaterThanOrEqualTo(antes, log.FechaHora);
    }

    // Sin usuario (email que no existe): UsuarioId 0 y se guarda el email intentado
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
        Assert.HasCount(2, _datos.Logs);
    }
}

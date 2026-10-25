using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nucleo.Servicios;
using System;

namespace Nucleo.Tests.Servicios;

[TestClass]
public class FormatosTests
{
    // Una fecha sin valor (default) se muestra como "No consta"
    [TestMethod]
    public void FormatearFecha_SinValor_NoConsta()
    {
        Assert.AreEqual("No consta", Formatos.FormatearFecha(default));
    }

    // Las fechas se muestran como dd/MM/yyyy HH:mm:ss
    [TestMethod]
    [DataRow(2026, 10, 25, 9, 5, 3, "25/10/2026 09:05:03")]
    [DataRow(2026, 1, 1, 0, 0, 0, "01/01/2026 00:00:00")]
    [DataRow(2026, 12, 31, 23, 59, 59, "31/12/2026 23:59:59")]
    public void FormatearFecha_ConValor_FormatoEspanol(
        int anio, int mes, int dia, int hora, int minuto, int segundo, string esperado)
    {
        DateTime fecha = new DateTime(anio, mes, dia, hora, minuto, segundo);

        Assert.AreEqual(esperado, Formatos.FormatearFecha(fecha));
    }
}

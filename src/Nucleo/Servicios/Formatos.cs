using System;

namespace Nucleo.Servicios;

public static class Formatos
{
    public static string FormatearFecha(DateTime fecha)
    {
        if (fecha == default)
        {
            return "No consta";
        }
        return fecha.ToString("dd/MM/yyyy HH:mm:ss");
    }
}

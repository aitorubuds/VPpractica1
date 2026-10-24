using System.Collections.Generic;
using Datos;
using Nucleo.Modelos;

namespace Datos;

public static class DatosExtensions
{
    public static IEnumerable<Proyecto> ProyectosTodos(this IDatos datos)
    {
        return datos.Proyectos.Values;
    }
}

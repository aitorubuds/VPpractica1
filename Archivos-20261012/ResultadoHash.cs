using System;
using System.Collections.Generic;
using System.Text;

namespace TestProjectManagementPlatform.Nucleo.Seguridad
{
    public class ResultadoHash
    {
        public string Hash { get; init; } = string.Empty;
        public string Sal { get; init; } = string.Empty;
        public DateTime Creacion {  get; init; } = DateTime.Now;
    }
}

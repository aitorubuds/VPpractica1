# Análisis de cobertura

Medido el 25/10/2026 con Microsoft Code Coverage (incluido en MSTest.Sdk):

```
dotnet test --coverage --coverage-output-format cobertura
```

En Visual Studio: **Prueba → Analizar cobertura de código para todas las pruebas**.

## Resultado

| Ensamblado | Clase | Líneas | Ramas | Pruebas |
|---|---|---|---|---|
| Nucleo | `Seguridad.ServicioHash` | 100 % | 100 % | `ServicioHashTests` (CP-01 a CP-18, caja negra E01) |
| Nucleo | `Seguridad.ResultadoHash` | 100 % | 100 % | `ResultadoHashTests` (CP-19, CP-20 y pruebas de igualdad) |
| Nucleo | `Seguridad.ServicioContrasenas` | 100 % | 100 % | `ServicioContrasenasTests` |
| Nucleo | `Seguridad.ServicioCifrado` | 100 % | 100 % | `ServicioCifradoTests` |
| Nucleo | `Servicios.Formatos` | 100 % | 100 % | `FormatosTests` |
| Nucleo | `Modelos.Usuario` | 100 % | 100 % | `UsuarioTests`, `IntegracionTests` |
| Nucleo | `Modelos.Proyecto`, `Rol`, `EntradaLog` | 100 % | 100 % | `UsuarioTests`, `DatosTests` |
| Datos | `Datos` | 100 % | 100 % | `DatosTests` (33 casos, incluidos los de error) |
| Datos | `DatosPrueba` | 100 % | 100 % | `DatosPruebaTests` |
| **Total** | **Nucleo** | **100 %** | **100 %** | |
| **Total** | **Datos** | **100 %** | **100 %** | |

Antes de esta fase, `Nucleo` estaba al **48,6 %** de líneas: `ServicioCifrado` y `Formatos` al 0 %, y `ResultadoHash` al 17 %.

## Qué se añadió para llegar al 100 %

| Clase | Qué faltaba | Pruebas añadidas |
|---|---|---|
| `ServicioCifrado` | Nada probado | Clave de 32 bytes y distinta en cada llamada; cifrar y descifrar recupera el texto; mismo texto da cifrados distintos; texto, clave o contenido no válidos lanzan `ArgumentException`; contenido corto; otra clave o contenido manipulado lanzan `CryptographicException` |
| `Formatos` | Nada probado | Fecha sin valor → «No consta»; formato `dd/MM/yyyy HH:mm:ss` |
| `ResultadoHash` | `Equals`, `GetHashCode`, `==` y `!=` | Igualdad con mismos valores, valores distintos, consigo mismo, con `null` y con otro tipo |
| `Usuario` | Alias para la web, usuario inactivo, `ToString`, `TieneCredencialesConfiguradas` | Pruebas de cada uno |
| `Proyecto` | Alias `IdProyecto` | Una prueba |

## Partes insuficientemente probadas

Un 100 % de líneas y ramas no significa que todo esté bien probado:

1. **La web (`src/Web`) no tiene pruebas unitarias.** Las páginas Razor dependen de la sesión y de HTTP; se prueban con integración y con Selenium y Katalon.
2. **`ServicioCifrado` aún no se usa** en la aplicación. Está probado de forma aislada, pero falta cifrar algún dato sensible, como pide el acta.
3. **`ServicioCifrado.Descifrar` con contenido que no es Base64** lanza `FormatException` y no `ArgumentException`, a diferencia del resto de errores de formato. No es un fallo de cobertura, pero sí una inconsistencia de diseño: queda anotada y no se cambia, porque el código es del profesor.
4. **Aleatoriedad.** Que las sales, claves y nonces no se repitan se comprueba con pocas llamadas (2 o 20). Es una muestra, no una demostración.
5. **Concurrencia.** `Datos` se registra como Singleton y no se prueba con varias peticiones a la vez. Para esta práctica se acepta, porque es una base de datos simulada.

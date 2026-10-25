# E01 – Casos de prueba de ServicioHash y ResultadoHash

Pruebas de caja negra hechas con MSTest.Sdk a partir de las especificaciones ED-01 a ED-20 del E01.
Código: `tests/Nucleo.Tests/Seguridad/ServicioHashTests.cs` y `ResultadoHashTests.cs`.
Las pruebas parametrizadas (`[DataRow]`) cuentan como un solo caso con todos sus datos.

| ID | REQUISITO | NOMBRE DE LA PRUEBA | ENTRADA | RESULTADO ESPERADO |
|---|---|---|---|---|
| CP-01 | ED-01, ED-02, ED-05 | `CP01_Generar_TextoValido_DevuelveResultado` | `"MiPassword123"`, `"a"`, `"texto con espacios"`, `"ñáéíóú€@#"` | Devuelve una instancia de `ResultadoHash` (no nula) |
| CP-02 | ED-03, ED-04 | `CP02_Generar_TextoNulo_LanzaExcepcion` | `null` | Lanza `InvalidOperationException` |
| CP-03 | ED-03, ED-04 | `CP03_Generar_TextoVacioOEspacios_LanzaExcepcion` | `""`, `" "`, `"   "`, `"\t"`, `"\n"` | Lanza `InvalidOperationException` |
| CP-04 | ED-09 | `CP04_Generar_HashYSalNoVacios` | `"MiPassword123"` | `Hash` y `Sal` no nulos, no vacíos y sin ser solo espacios |
| CP-05 | ED-09, ED-12 | `CP05_Generar_ResultadoValidoParaVerificar` | `"MiPassword123"` y su `Hash`/`Sal` | `Verificar` devuelve `true` con lo generado |
| CP-06 | ED-06, ED-07 | `CP06_Generar_MismoTexto_SalesDiferentes` | `"MismaContrasena"` dos veces | Las dos `Sal` son distintas |
| CP-07 | ED-08 | `CP07_Generar_MismoTexto_HashesDiferentes` | `"MismaContrasena"` dos veces | Los dos `Hash` son distintos |
| CP-08 | ED-06, ED-07 | `CP08_Generar_VariasLlamadas_SalesUnicas` | `"MismaContrasena"` 20 veces | 20 sales distintas |
| CP-09 | ED-10, ED-11, ED-12 | `CP09_Verificar_TextoCorrecto_DevuelveTrue` | `"ClaveSecreta123"`, `"a"`, `"ñáéíóú€@#"` con su hash y sal | `true` |
| CP-10 | ED-13 | `CP10_Verificar_TextoIncorrecto_DevuelveFalse` | Original `"ClaveCorrecta"`; probado `"ClaveErronea"`, `"clavecorrecta"`, `"ClaveCorrecta "`, `"ClaveCorrect"` | `false` |
| CP-11 | ED-13 | `CP11_Verificar_HashIncorrecto_DevuelveFalse` | Texto y sal correctos, hash de `"OtraClave"` | `false` |
| CP-12 | ED-13 | `CP12_Verificar_SalIncorrecta_DevuelveFalse` | Texto y hash correctos, sal de otra generación | `false` |
| CP-13 | ED-14 | `CP13_15_Verificar_ParametroNoValido_DevuelveFalse` | `null` en texto, en hash y en sal (por separado) | `false` |
| CP-14 | ED-14 | `CP13_15_Verificar_ParametroNoValido_DevuelveFalse` | `""` en texto, en hash y en sal (por separado) | `false` |
| CP-15 | ED-14 | `CP13_15_Verificar_ParametroNoValido_DevuelveFalse` | `"   "` en texto, en hash y en sal (por separado) | `false` |
| CP-16 | ED-15, ED-16 | `CP16_Verificar_HashFormatoNoValido_DevuelveFalse` | Hash `"no-es-un-hash-valido!!!"`, `"abc"`, `"===="` | `false`, sin excepción |
| CP-17 | ED-15, ED-16 | `CP17_Verificar_SalFormatoNoValido_DevuelveFalse` | Sal `"no-es-una-sal-valida!!!"`, `"abc"`, `"===="` | `false`, sin excepción |
| CP-18 | ED-17 | `CP18_Verificar_DatosAlmacenadosIncorrectos_NoLanzaExcepcion` | Base64 válido de tamaño incorrecto: (`"AAAA"`, `"AAAA"`), (`"AA=="`, `"AA=="`), (`"QUJD"`, sal real) | `false`, sin excepción |
| CP-19 | ED-18, ED-19 | `CP19_ResultadoHash_GuardaHashYSal` | `Hash = "hash-de-prueba"`, `Sal = "sal-de-prueba"` | Las propiedades devuelven esos valores |
| CP-20 | ED-20 | `CP20_ResultadoHash_DelServicio_SinValoresVacios` | `ResultadoHash` generado con `"MiPassword123"`, `"a"`, `"texto con espacios"`, `"ñáéíóú€@#"` | `Hash` y `Sal` no nulos y no vacíos |

Prueba adicional, fuera del E01: `Extra_Verificar_ConObjetoResultadoHash` (sobrecarga `Verificar(texto, ResultadoHash)`, también con `null` → `false`).

## Cobertura de las especificaciones

| ED | Casos |
|---|---|
| ED-01, ED-02 | CP-01 |
| ED-03, ED-04 | CP-02, CP-03 |
| ED-05 | CP-01 |
| ED-06, ED-07 | CP-06, CP-08 |
| ED-08 | CP-07 |
| ED-09 | CP-04, CP-05 |
| ED-10, ED-11 | CP-09 |
| ED-12 | CP-05, CP-09 |
| ED-13 | CP-10, CP-11, CP-12 |
| ED-14 | CP-13, CP-14, CP-15 |
| ED-15, ED-16 | CP-16, CP-17 |
| ED-17 | CP-16, CP-17, CP-18 |
| ED-18, ED-19 | CP-19 |
| ED-20 | CP-20 |

## Defectos encontrados en la implementación proporcionada

| ID | Defecto | Caso que lo detecta |
|---|---|---|
| DEF-01 | `ServicioHash` era `static`; el E01 define `public class ServicioHash` | No compilan las pruebas que crean `new ServicioHash()` |
| DEF-02 | `Generar` lanzaba `ArgumentException` en vez de `InvalidOperationException` | CP-02, CP-03 (fallan 6 ejecuciones) |
| DEF-03 | `Verificar(texto, ResultadoHash)` con `null` daba `NullReferenceException` | `Extra_Verificar_ConObjetoResultadoHash` |

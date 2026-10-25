# Plan de Trabajo Oficial: UBU_TPMP (Alineado con AT1 y PR1B-W)

> **Proyecto:** UBU_TPMP (UBUTestProjectManagementPlatform)  
> **Asignatura:** Validación y Pruebas (6376 / 8828) — Universidad de Burgos  
> **Fecha Límite:** 28 de octubre de 2026 (23:59 h)  
> **Defensa Presencial:** 07 al 14 de noviembre de 2026  
> **Objetivo de Calificación:** 10,00 / 10,00 pts (+ 0,50 pts extra)

Este documento sirve como lista de control (*checklist*) operativa del equipo (Alex & Aitor), sincronizada estrictamente con los criterios de evaluación de **`AT1`**, el enunciado **`PR1`** y la guía web **`PR1B-W`**.

---

## Desglose de Puntuación Oficial (`AT1`)

| Bloque Evaluado | Puntos Máx. | Criterios Específicos |
| :--- | :---: | :--- |
| **Documentación Técnica** | **3,00 pts** | Formato (0,25) · Plan de pruebas (0,50) · Requisitos (0,75) · Modelos UML (0,50) · Definición detallada de Casos de Prueba con datos/pasos/resultados (1,00) |
| **Pruebas Unitarias** | **1,00 pts** | Baterías MSTest sobre lógica de negocio, caja blanca y caja negra (100% de éxito) |
| **Pruebas de Integración** | **1,00 pts** | Verificación de interacción entre capas (Dominio, Datos en memoria y Web) |
| **Aceptación / Validación Web** | **1,50 pts** | Automatización C# + MSTest + **Selenium WebDriver** con **Google Chrome** |
| **Automatización con Katalon** | **1,50 pts** | Suites y casos grabados con **Katalon Recorder** y exportados en ZIP |
| **Recubrimiento de Código** | **1,00 pts** | Análisis de cobertura ($\ge 90\%$, actualmente **100%**) e identificación de puntos débiles |
| **Defensa Oral** | **1,00 pts** | Dominio de requisitos, diseño de pruebas y justificación técnica ante el profesor |
| **Aspectos Adicionales** | **0,50 pts** | Subida de nota (Bootstrap local, datos semilla de prueba, defensas adicionales) |

---

## Bloque I: Configuración del Entorno .NET 10 (Fases 1 - 4)

- [X] **Fase 01 — Creación de la Solución Visual Studio**
  - Crear el archivo de solución `TestProjectManagementPlatform.sln` en la raíz de `VPpractica1`.
  - Configurar las carpetas de solución (`src/` y `tests/`).
  - *Criterio de éxito:* Apertura limpia de la solución en Visual Studio Community 2026.

- [X] **Fase 02 — Regularización de la Capa de Datos (`src/Datos`)**
  - Incorporar `IDatos.cs` con el namespace `Datos`.
  - Crear `src/Datos/Datos.csproj` configurado para `net10.0` con referencia a `Nucleo`.
  - *Criterio de éxito:* El proyecto `Datos` compila sin errores de tipos o referencias.

- [X] **Fase 03 — Creación del Proyecto de Pruebas Unitarias (`tests/Nucleo.Tests`)**
  - Crear `tests/Nucleo.Tests/Nucleo.Tests.csproj` con `MSTest.Sdk` (4.5.1) sobre `net10.0`.
  - Configurar `global.json` con `Microsoft.Testing.Platform` y cobertura nativa (`dotnet test --coverage`).
  - Referenciar `src/Nucleo/Nucleo.csproj` y `src/Datos/Datos.csproj`.
  - *Criterio de éxito:* El Explorador de Pruebas de Visual Studio detecta y ejecuta el proyecto.

- [X] **Fase 04 — Enlace de Proyectos y Primera Compilación Limpia**
  - Añadir todos los proyectos (`Nucleo`, `Datos`, `Web`, `Nucleo.Tests`) a la solución.
  - Ejecutar compilación completa en `.NET 10`.
  - *Criterio de éxito:* `dotnet build` completa con 0 errores.

---

## Bloque II: Servicios Criptográficos y Pruebas Base (Fases 5 - 8)

- [X] **Fase 05 — Verificación de Servicios Criptográficos en `Nucleo`**
  - Revisar que `ServicioHash.cs`, `ResultadoHash.cs`, `ServicioCifrado.cs` y `ServicioContrasenas.cs` compilan en `Nucleo/Seguridad/`.
  - *Criterio de éxito:* Clases operativas y disponibles para `Nucleo.Tests`.

- [X] **Fase 06 — Pruebas de Caja Blanca de `ServicioContrasenas`**
  - Crear `tests/Nucleo.Tests/Seguridad/ServicioContrasenasTests.cs`.
  - Implementar 8 casos de prueba cubriendo el 100% de la complejidad ciclomática ($V(G)=8$).
  - *Criterio de éxito:* 8 tests en verde y 100% de recubrimiento en `ServicioContrasenas`.

- [X] **Fase 07 — Pruebas de Caja Negra de `ServicioHash` (Conforme a E01)**
  - Implementar `ServicioHash` como clase de instancia con parámetros `string?` y lanzamiento de `InvalidOperationException` ante entradas nulas o vacías según `E01`.
  - Crear `tests/Nucleo.Tests/Seguridad/ServicioHashTests.cs` (CP-01 a CP-20, cubriendo ED-01 a ED-20 con 78 ejecuciones).
  - Crear `tests/Nucleo.Tests/Seguridad/ResultadoHashTests.cs` (ED-18 a ED-20).
  - Documentar la matriz de casos de prueba en `docs/E01_Casos_de_Prueba.md`.
  - *Criterio de éxito:* 100% de cobertura y aserciones en verde en `ServicioHash`.

- [X] **Fase 08 — Pruebas de `ServicioCifrado` y `Formatos`**
  - Crear `tests/Nucleo.Tests/Seguridad/ServicioCifradoTests.cs` (cifrado/descifrado AES-GCM con claves de 256 bits y manejo de excepciones).
  - Crear `tests/Nucleo.Tests/Servicios/FormatosTests.cs` para el formateo de fechas.
  - *Criterio de éxito:* 100% de cobertura en todos los servicios auxiliares de `Nucleo`.

---

## Bloque III: Modelo de Dominio y Funcionalidad de Grupo (Fases 9 - 11)

- [X] **Fase 09 — Diseño de la Funcionalidad Exclusiva de Grupo**
  - *Opción elegida:* Bloqueo automático de cuenta tras 3 intentos fallidos consecutivos de contraseña, reseteo tras login exitoso y desbloqueo administrativo.
  - Propiedades en `Usuario`: `IntentosFallidos` (`IntentosErroneos`), `Bloqueado`, `Activo` (`Activa`), `FechaUltimoAcceso`, etc.
  - *Criterio de éxito:* Especificación aprobada y documentada.

- [X] **Fase 10 — Implementación Segura de `Usuario.cs`**
  - Métodos `ComprobarContrasena(string)`, `CambiarContrasena(string, string)` y `Desbloquear()`.
  - Alias de compatibilidad web: `CompruebaPassword` y `CambiaPassword`.
  - *Criterio de éxito:* La contraseña en texto plano nunca se expone y la seguridad está encapsulada.

- [X] **Fase 11 — Pruebas Unitarias de `Usuario`**
  - Crear `tests/Nucleo.Tests/Modelos/UsuarioTests.cs`.
  - Pruebas de comprobación, bloqueo a los 3 fallos, rechazo de acceso bloqueado y desbloqueo.
  - *Criterio de éxito:* 100% de cobertura en `Usuario.cs`.

---

## Bloque IV: Capa de Persistencia en Memoria (`Datos`) (Fases 12 - 13)

- [X] **Fase 12 — Consolidación de `Datos.cs` (Encapsulación y Consistencia)**
  - Retorno de copias defensivas en `Usuarios`, `Roles`, `Proyectos` y `Logs` para evitar corrupción externa.
  - Control de consistencia: email único sin distinguir mayúsculas, desasignación en cascada de proyectos al borrar usuario, reseteo de roles y preservación de auditoría en log.
  - Creación de `src/Datos/DatosPrueba.cs` con estado inicial conocido (4 usuarios, 3 roles, 3 proyectos, 2 logs).
  - *Criterio de éxito:* Repositorio robusto y protegido contra efectos colaterales.

- [X] **Fase 13 — Pruebas Unitarias y Escenarios de Error de `Datos.cs`**
  - Crear `tests/Nucleo.Tests/Datos/DatosTests.cs` (33 pruebas: 11 escenarios base de `PR1B` + 22 escenarios de error).
  - Crear `tests/Nucleo.Tests/Datos/DatosPruebaTests.cs`.
  - *Criterio de éxito:* 100% de líneas y ramas cubiertas en la capa de datos.

---

## Bloque V: Cobertura de Código e Integración (Fases 14 - 15)

- [X] **Fase 14 — Pruebas de Integración entre Capas**
  - En `tests/Nucleo.Tests/Integracion/IntegracionTests.cs`:
    - `Flujo_RegistroYLogin`: ciclo completo entre `Usuario` y `Datos`.
    - `Flujo_BloqueoYCambioClave`: ciclo de 3 intentos erróneos, bloqueo en repositorio, desbloqueo y cambio seguro.
  - *Criterio de éxito:* Integración validada end-to-end con aserciones en todas las capas.

- [X] **Fase 15 — Auditoría y Verificación de Cobertura Total**
  - Ejecución con `dotnet test --coverage` en Visual Studio.
  - Redacción del informe formal en `docs/Analisis_Cobertura.md`.
  - *Criterio de éxito:* **154 de 154 pruebas superadas (0 fallos)** con **100% de cobertura** en `Nucleo` y `Datos`.

---

## Bloque VI: Aplicación Web ASP.NET Core Razor Pages (Fases 16 - 17)

- [X] **Fase 16 — Configuración del Pipeline Web e Inyección de Dependencias**
  - Configurar `src/Web/Program.cs`: Razor Pages, sesiones HTTP en memoria y singleton `IDatos` cargado con `DatosPrueba.Crear()`.
  - Incorporar Bootstrap 5.3.8 local en `wwwroot/lib/bootstrap/dist/css/bootstrap.css`.
  - Crear `_ViewImports.cshtml` y `_ViewStart.cshtml` para habilitar TagHelpers, tokens CSRF y layout compartido.
  - *Criterio de éxito:* La web compila sin advertencias y arranca en local (`F5`).

- [X] **Fase 17 — Vistas y Flujos Web Funcionales**
  - Login en dos pasos: `Pages/Login/Index.cshtml` (Email) e `Index2.cshtml` (Password).
  - Selección y alta de proyectos: `Pages/Proyectos/Seleccionar/Index.cshtml`.
  - Perfil de usuario y cambio de contraseña: `Pages/Usuarios/Perfil/Index.cshtml`.
  - Visor de auditoría: `Pages/Log/Index.cshtml`.
  - Protección de sesión: redirección automática a `/Login/Index` si no hay sesión activa.
  - *Criterio de éxito:* Navegación fluida y 0 errores en consola.

---

## Bloque VII: Aceptación Web y Automatización (Fases 18 - 19) — [PENDIENTE]

- [ ] **Fase 18 — Pruebas de Aceptación con C#, MSTest y Selenium WebDriver (1,50 pts)**
  - Crear el proyecto de pruebas de aceptación web (o suite Selenium con ChromeDriver).
  - Automatizar los escenarios clave:
    1. Login exitoso con credenciales válidas y redirección a Principal.
    2. Login fallido por contraseña incorrecta y visualización de mensaje de error.
    3. Bloqueo de cuenta tras 3 intentos sucesivos desde el navegador.
    4. Navegación a Proyectos, creación de un nuevo proyecto y selección en sesión.
    5. Cierre de sesión y verificación de redirección protegida.
  - *Criterio de éxito:* Pruebas ejecutadas automáticamente sobre Google Chrome en verde.

- [ ] **Fase 19 — Pruebas Automatizadas con Katalon Recorder (1,50 pts)**
  - Instalar la extensión Katalon Recorder en Chrome/Edge.
  - Grabar la suite de pruebas sobre `https://localhost:51612`:
    - Comandos: `open`, `type`, `click`, `assertText`, `verifyElementPresent`.
  - Ejecutar la suite completa y verificar resultado en verde.
  - Exportar el conjunto de pruebas en formato HTML/ZIP para el entregable.
  - *Criterio de éxito:* Archivo `Katalon_Recorder_Suites.zip` generado y listo.

---

## Bloque VIII: Documentación, Trazabilidad y Entrega Final (Fases 20 - 22) — [PENDIENTE]

- [ ] **Fase 20 — Configuración Final de Microsoft Planner (Apartado a)**
  - Tablero nombrado obligatoriamente: `[DniAlumno1]-[DniAlumno2]`.
  - Los 4 depósitos estructurados: *Por hacer*, *En progreso*, *En pruebas*, *Completado*.
  - Tarjetas con responsables asignados (Alex y Aitor), fechas reales y etiquetas.
  - Invitar al profesor Pedro Renedo (`prenedo@ubu.es`).
  - *Criterio de éxito:* Captura 1 lista para la memoria.

- [ ] **Fase 21 — Plan de Pruebas, Trazabilidad Completa y Modelado UML (Apartados b - e)**
  - Redactar el **Plan de pruebas formal** (0,50 pts): alcance, tipos, herramientas y criterios.
  - Tablas formales del Apartado b:
    - Tabla de Actores.
    - Tabla de Casos de Uso.
    - Tabla de Requisitos / Historias de Usuario.
    - Tabla de Criterios de Aceptación.
    - **Tabla de Casos de Prueba con atributos, condiciones, datos, acciones y resultados esperados** (1,00 pt).
  - Diagramas UML (0,50 pts): Paquetes, Clases de diseño (con clases de test), Casos de Uso y Secuencia.
  - *Criterio de éxito:* Trazabilidad total sin fisuras y 0 diagramas E-R.

- [ ] **Fase 22 — Memoria PDF Final y Empaquetado del Triple Entregable**
  - Compilar la memoria técnica a PDF con el formato institucional UBU y las 8 capturas incrustadas.
  - Limpieza rigurosa de la solución (`bin/`, `obj/`, `.vs/`, `.idea/`, temporales).
  - Empaquetar el archivo único **`[DniAlumno1]-[DniAlumno2].zip`** con:
    1. `Memoria_[Dni1]-[Dni2].pdf`
    2. `Proyecto_Visual_Studio.zip` (Solución limpia en .NET 10)
    3. `Katalon_Recorder_Suites.zip`
  - Validar descompresión y compilación en una carpeta limpia.
  - *Criterio de éxito:* Entrega completada en UBUVirtual antes del 28/10 a las 23:59.

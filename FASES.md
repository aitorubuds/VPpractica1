# Plan de Trabajo en 20 Fases: UBU_TPMP

> **Proyecto:** UBU_TPMP (UBUTestProjectManagementPlatform)  
> **Asignatura:** Validación y Pruebas — Universidad de Burgos  
> **Fecha Límite:** 28 de octubre de 2026 (23:59 h)  
> **Defensa Presencial:** 07 al 14 de noviembre de 2026  

Este documento sirve como lista de control (*checklist*) operativa para el seguimiento del desarrollo y de las pruebas. Conforme completes cada hito, marca la casilla correspondiente `[X]` y actualiza la tarjeta correspondiente en Microsoft Planner.

---

## Bloque I: Configuración del Entorno y Solución .NET (Fases 1 - 4)

- [X] **Fase 01 — Creación de la Solución Visual Studio**
  - Crear el archivo de solución `TestProjectManagementPlatform.sln` en la raíz de `VPpractica1`.
  - Configurar las carpetas de solución (`src/` y `tests/`).
  - *Criterio de éxito:* Se puede abrir la solución haciendo doble clic en `.sln` desde Visual Studio.

- [X] **Fase 02 — Regularización de la Capa de Datos (`src/Datos`)**
  - Mover/incorporar `IDatos.cs` con el namespace `Datos`.
  - Crear `src/Datos/Datos.csproj` configurado para `net10.0` con referencia a `Nucleo`.
  - *Criterio de éxito:* El proyecto `Datos` compila sin errores de tipos o referencias.

- [X] **Fase 03 — Creación del Proyecto de Pruebas Unitarias (`tests/Nucleo.Tests`)**
  - Crear la carpeta `tests/Nucleo.Tests/`.
  - Crear `Nucleo.Tests.csproj` con `MSTest.Sdk` (4.5.1) sobre `net10.0` y `global.json` con Microsoft.Testing.Platform (cobertura con `dotnet test --coverage`).
  - Referenciar `src/Nucleo/Nucleo.csproj` y `src/Datos/Datos.csproj`.
  - *Criterio de éxito:* El Explorador de Pruebas de Visual Studio detecta el proyecto.

- [X] **Fase 04 — Enlace de Proyectos y Primera Compilación Limpia**
  - Añadir todos los proyectos (`Nucleo`, `Datos`, `Web`, `Nucleo.Tests`) al archivo `.sln`.
  - Ejecutar compilación completa de la solución.
  - *Criterio de éxito:* `dotnet build` completa con 0 errores y 0 advertencias críticas.

---

## Bloque II: Servicios Criptográficos y Pruebas Base (Fases 5 - 8)

- [X] **Fase 05 — Verificación de Servicios Criptográficos en `Nucleo`**
  - Revisar que `ServicioHash.cs`, `ResultadoHash.cs`, `ServicioCifrado.cs` y `ServicioContrasenas.cs` compilan en `Nucleo/Seguridad/`.
  - Comprobar que no hay dependencias circulares ni código incompleto.
  - *Criterio de éxito:* Las 4 clases compilan y sus métodos públicos están disponibles para `Nucleo.Tests`.

- [X] **Fase 06 — Pruebas de Caja Blanca de `ServicioContrasenas`**
  - Crear `tests/Nucleo.Tests/Seguridad/ServicioContrasenasTests.cs`.
  - Implementar mínimo 8 casos de prueba para cubrir el 100% de la complejidad ciclomática:
    1. Contraseña válida completa.
    2. Longitud < 8 caracteres.
    3. Sin mayúsculas.
    4. Sin minúsculas.
    5. Sin números.
    6. Sin caracteres especiales / símbolos.
    7. Nula o vacía.
    8. Con espacios en blanco.
  - *Criterio de éxito:* 8 tests pasan en verde (PASS) y la cobertura en `ServicioContrasenas` es del 100%.

- [X] **Fase 07 — Pruebas de Caja Negra de `ServicioHash` (Parte 1: Entradas Inválidas)**
  - Crear `tests/Nucleo.Tests/Seguridad/ServicioHashTests.cs`.
  - Implementar las pruebas para las especificaciones ED-01 a ED-04:
    - `Generar(null)` $\to$ lanza excepción.
    - `Generar("")` $\to$ lanza excepción.
    - `Generar("   ")` $\to$ lanza excepción.
  - *Criterio de éxito:* Tests verificados con `Assert.ThrowsExactly<InvalidOperationException>`.

- [X] **Fase 08 — Pruebas de Caja Negra de `ServicioHash` (Parte 2: Generación y Verificación)**
  - Implementar las especificaciones ED-05 a ED-20 en `ServicioHashTests.cs`:
    - `Generar`: devuelve `ResultadoHash` válido, no nulo.
    - Dos ejecuciones sucesivas con el mismo texto generan `Sal` distinta y `Hash` distinto (unicidad y variabilidad).
    - `Verificar`: devuelve `true` con texto, hash y sal legítimos.
    - `Verificar`: devuelve `false` con texto erróneo, hash corrupto, sal corrupta o argumentos nulos/vacíos.
    - `Verificar`: robustez absoluta ante datos malformados (no propaga excepciones no controladas).
  - Tabla de casos CP-01 a CP-20 en `docs/E01_Casos_de_Prueba.md` (más `ResultadoHashTests.cs` para ED-18 a ED-20).
  - *Criterio de éxito:* Batería completa de caja negra pasa al 100% en verde y `ServicioHash` con 100% de cobertura.

---

## Bloque III: Modelo `Usuario` y Funcionalidad de Grupo (Fases 9 - 11)

- [X] **Fase 09 — Diseño de la Funcionalidad Exclusiva de Grupo**
  - Acordar y documentar la funcionalidad adicional de seguridad de `Usuario.cs`:
    - *Opción elegida:* Bloqueo de cuenta tras 3 intentos fallidos consecutivos de contraseña, reseteo del contador tras autenticación exitosa y método administrativo de desbloqueo.
  - Definir las propiedades en `Usuario`: `IntentosFallidos`, `Bloqueado`.
  - *Criterio de éxito:* Especificación funcional aprobada para codificar.

- [X] **Fase 10 — Implementación Segura de `Usuario.cs`**
  - Encapsular las propiedades sensibles: la contraseña en texto plano nunca se expone.
  - Implementar `bool ComprobarContrasena(string contrasena)`:
    - Si la cuenta está bloqueada, retorna `false`.
    - Si la contraseña es correcta, reinicia `IntentosFallidos = 0` y retorna `true`.
    - Si es incorrecta, incrementa `IntentosFallidos`; si llega a 3, marca `Bloqueado = true` y retorna `false`.
  - Implementar `bool CambiarContrasena(string contrasenaActual, string nuevaContrasena)`:
    - Verifica la contraseña actual.
    - Comprueba complejidad con `ServicioContrasenas.EsValida`.
    - Genera nuevo hash y sal con `ServicioHash.Generar` y actualiza.
  - Implementar `void Desbloquear()`.
  - *Criterio de éxito:* `Usuario.cs` compila y satisface los requisitos de seguridad y la funcionalidad de grupo.

- [X] **Fase 11 — Batería de Pruebas Unitarias de `Usuario`**
  - Crear `tests/Nucleo.Tests/Modelos/UsuarioTests.cs`.
  - Pruebas para:
    - Comprobación exitosa y fallida de contraseña.
    - Cambio de contraseña válido e inválido (clave actual errónea o nueva clave sin complejidad).
    - Bloqueo automático al tercer intento fallido consecutivo.
    - Rechazo de acceso cuando el usuario está bloqueado incluso con contraseña correcta.
    - Desbloqueo de cuenta por método administrativo.
  - *Criterio de éxito:* Todos los tests pasan y cubren el 98% de la lógica de `Usuario`.

---

## Bloque IV: Capa de Persistencia en Memoria (`Datos`) (Fases 12 - 13)

- [X] **Fase 12 — Consolidación de `Datos.cs`**
  - Verificar la lógica de asignación autoincremental de identificadores:
    - Si `Id < 0` $\to$ asigna `++_ultimoId` e inserta.
    - Si `Id >= 0` y existe $\to$ actualiza la entidad; si no existe $\to$ devuelve `null`.
  - Verificar operaciones CRUD de `Usuario`, `Rol`, `Proyecto` y registro de `EntradaLog`.
  - Verificar que `Reiniciar()` vacía todas las colecciones y restablece los contadores a 0.
  - *Criterio de éxito:* `Datos.cs` listo para pruebas sin persistencia real.

- [X] **Fase 13 — Pruebas Unitarias de `Datos.cs`**
  - Crear `tests/Nucleo.Tests/Datos/DatosTests.cs`.
  - Incorporar método anotado con `[TestInitialize]` que invoque `_datos.Reiniciar()`.
  - Pruebas para:
    - Inserción y auto-asignación de ID positivo.
    - Actualización de entidad existente.
    - Búsqueda por ID existente y por ID inexistente (debe devolver `null`).
    - Búsqueda de usuario por correo.
    - Borrado de entidades existentes y no existentes.
    - Registro de logs y limpieza de auditoría (`LimpiarLog()`).
    - Comprobación de restablecimiento total con `Reiniciar()`.
  - *Criterio de éxito:* Todos los casos de prueba de datos pasan en verde (cobertura 98.8%).

---

## Bloque V: Cobertura de Código e Integración (Fases 14 - 15)

- [X] **Fase 14 — Pruebas de Integración entre Capas**
  - Crear `tests/Nucleo.Tests/Integracion/IntegracionTests.cs`.
  - Probar el ciclo completo del sistema:
    1. Creación de un usuario con contraseña segura.
    2. Guardado en `Datos` con asignación de ID.
    3. Simulación de 3 intentos fallidos de login $\to$ verificar bloqueo en persistencia.
    4. Registro automático de la auditoría en `EntradaLog`.
    5. Desbloqueo y cambio de contraseña exitoso.
  - *Criterio de éxito:* Flujo integral superado con aserciones en todas las capas.

- [X] **Fase 15 — Auditoría y Verificación de Cobertura ($\ge 90\%$)**
  - En Visual Studio: Menú `Prueba` $\to$ `Analizar cobertura de código para todas las pruebas`.
  - Verificar que el recubrimiento de líneas y ramas en la librería `Nucleo` supera el **90%** (requisito del profesor para la máxima nota).
  - Identificar líneas o ramas no cubiertas y añadir los casos de prueba necesarios.
  - Documentar el análisis y lo insuficientemente probado en `docs/Analisis_Cobertura.md`.
  - *Criterio de éxito:* Cobertura medida el 25/10: **100 %** de líneas y ramas en `Nucleo` y `Datos` (154 pruebas).

---

## Bloque VI: Aplicación Web ASP.NET Core (Fases 16 - 17)

- [X] **Fase 16 — Configuración Web e Inyección de Dependencias**
  - Configurar `Program.cs` en `src/Web/`:
    - Registrar `IDatos` como Singleton en memoria.
    - Cargar los datos de prueba con `DatosPrueba.Crear()` (`src/Datos/DatosPrueba.cs`): estado conocido de 4 usuarios, 3 roles, 3 proyectos y 2 logs.
  - *Criterio de éxito:* La aplicación web arranca en local (`dotnet run` o F5 en Visual Studio).

- [X] **Fase 17 — Vistas y Flujos Web de Usabilidad**
  - Integrar las plantillas Razor Pages oficiales del profesor:
    - Redirección de inicio a login (`Pages/Index.cshtml`).
    - Flujo de login en dos pasos (`Pages/Login/Index.cshtml` e `Index2.cshtml`).
    - Listado, alta y selección de proyectos (`Pages/Proyectos/Seleccionar/Index.cshtml`).
    - Perfil de usuario y cambio seguro de contraseña (`Pages/Usuarios/Perfil/Index.cshtml`).
    - Auditoría y consulta de registros de eventos (`Pages/Log/Index.cshtml`).
  - *Criterio de éxito:* La interfaz compila sin errores y queda totalmente vinculada a la capa `Datos` y `Nucleo`.

---

## Bloque VII: Documentación, Trazabilidad y Entrega (Fases 18 - 20)

- [ ] **Fase 18 — Configuración Final del Tablero Kanban en Microsoft Planner**
  - Crear el tablero `DniAlumno1-DniAlumno2` en Microsoft Planner.
  - Crear los 4 depósitos (*Por hacer*, *En progreso*, *En pruebas*, *Completado*).
  - Asignar miembros y registrar fechas reales de ejecución.
  - Asegurar el acceso compartido al profesor Pedro Renedo (`prenedo@ubu.es`).
  - *Criterio de éxito:* Tablero completamente estructurado y con actividad registrada.

- [ ] **Fase 19 — Matriz de Trazabilidad y Modelado UML**
  - Elaborar la tabla de trazabilidad completa:
    $$\text{Actores (ACT)} \longrightarrow \text{Casos de Uso (CU)} \longrightarrow \text{Requisitos / HU (RE)} \longrightarrow \text{Criterios Aceptación (CA)} \longrightarrow \text{Casos de Prueba (CP)}$$
  - Generar los diagramas UML (formato estándar, recordando no incluir diagramas E-R):
    - Diagrama de Paquetes de alto nivel.
    - Diagrama de Clases de diseño (incluyendo clases de pruebas unitarias).
    - Diagrama de Casos de Uso.
  - *Criterio de éxito:* Trazabilidad sin huecos y modelos UML coherentes con el código.

- [ ] **Fase 20 — Memoria PDF, Empaquetado en ZIP y Preparación de la Defensa**
  - Redactar la memoria final en PDF integrando todos los apartados (a - f) con capturas de Planner y de cobertura de Visual Studio.
  - Limpiar la solución (`bin/` y `obj/`) para no exceder el tamaño límite de UBUVirtual.
  - Comprimir todo en un único fichero `DniAlumno1-DniAlumno2.zip`.
  - Ensayar la ejecución de las pruebas y la apertura de cobertura en Visual Studio de cara a la defensa del 07 al 14 de noviembre.
  - *Criterio de éxito:* Archivo ZIP validado, dentro del límite de peso y listo para entregar antes del 28 de octubre.

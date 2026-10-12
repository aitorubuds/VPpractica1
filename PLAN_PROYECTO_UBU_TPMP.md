# Plan de Proyecto Maestro: UBU_TPMP (Test Project Management Platform)
> **Asignatura:** Validación y Pruebas (6376 / 8828) — Grado en Ingeniería Informática (UBU)  
> **Profesor:** Pedro Renedo Fernández  
> **Curso:** 2025-2026 / 2026-2027  
> **Fecha Límite de Entrega:** 28 de octubre de 2026 (23:59 h)  
> **Periodo de Defensa:** Del 07 al 14 de noviembre de 2026 (Obligatoria)  
> **Peso en la Calificación:** 60% de la nota de prácticas  

---

## 1. Resumen y Objetivos del Proyecto

El proyecto **UBU_TPMP** (*UBUTestProjectManagementPlatform*) es una plataforma web para la gestión integral de proyectos de pruebas, inspirada funcionalmente en la herramienta **TestLink** y construida con tecnologías Microsoft (.NET 8, C#, ASP.NET Core y Visual Studio Community 2026).

El objetivo principal de la práctica no radica en desarrollar una interfaz gráfica sofisticada ni funcionalidades exhaustivas fuera del alcance, sino en **demostrar el dominio riguroso de la metodología de validación y verificación de software**:
- Trazabilidad bidireccional completa desde requisitos hasta casos de prueba.
- Modelado formal exclusivamente en UML.
- Diseño e implementación de pruebas unitarias de caja negra y caja blanca mediante **MSTest.Sdk**.
- Recubrimiento/cobertura de código **$\ge 90\%$** verificado en Visual Studio.
- Planificación y seguimiento ágil en equipo mediante un tablero Kanban digital en Microsoft Planner.

---

## 2. Normas Críticas y Criterios del Profesor

Las siguientes pautas fueron remarcadas expresamente por el profesor Pedro Renedo durante las sesiones prácticas y en el enunciado oficial:

| Aspecto | Regla / Directriz Obligatoria |
| :--- | :--- |
| **Modelado de Datos** | **Prohibido el uso de diagramas Entidad-Relación (E-R)**. Toda la documentación del sistema debe realizarse exclusivamente en **UML** (Diagramas de Clases, Paquetes y Casos de Uso). |
| **Uso de Inteligencia Artificial** | Se permite el uso de IA para generar código auxiliar o esqueletos de clases (ej. `Usuario.cs`), pero está **estrictamente prohibido generar el código de las pruebas unitarias con IA** (objeto principal de evaluación). |
| **Cobertura de Código (Recubrimiento)** | **$\ge 90\%$ en la librería `Núcleo`** para aspirar a la calificación máxima de 10. Una cobertura de **80% o inferior acarreará penalizaciones directas** en la defensa. |
| **Defensa en Visual Studio** | El profesor comprobará in situ la ejecución de las pruebas y la ventana de *Analizar cobertura de código* en Visual Studio. |
| **Encapsulación de Contraseñas** | Ninguna contraseña debe almacenarse ni ser accesible en texto plano. Las propiedades hash y sal no deben exponerse públicamente sin control. |
| **Tablero Kanban Digital** | Debe crearse obligatoriamente en **Microsoft Planner** (Office 365) con el nombre exacto `DniAlumno1-DniAlumno2` y dar acceso al profesor. |
| **Política de Retrasos** | Se admiten entregas con retraso de hasta 10 días naturales con una penalización acumulativa del **-2% diario** (máx. -20%). Ante discrepancias en fechas, siempre aplica la más favorable (finales de octubre). |

---

## 3. Arquitectura de la Solución (.NET 8)

La solución de Visual Studio se estructura de forma desacoplada y modular para garantizar la testabilidad:

```text
TestProjectManagementPlatform.sln
├── src/
│   ├── Nucleo/                                 # Librería de Clases (.NET 8) - Núcleo del dominio
│   │   ├── Modelos/                            # Clases de dominio del sistema
│   │   │   ├── Usuario.cs                      # Entidad Usuario con seguridad y funcionalidad de grupo
│   │   │   ├── Rol.cs                          # Roles y asignación de permisos
│   │   │   ├── Proyecto.cs                     # Proyectos de pruebas gestionados
│   │   │   ├── EntradaLog.cs                   # Auditoría de acciones del sistema
│   │   │   └── EntidadesTestLink/              # (Opcional) SuitePruebas, CasoPrueba, PlanPruebas, etc.
│   │   ├── Seguridad/                          # Servicios criptográficos oficiales
│   │   │   ├── ServicioHash.cs                 # Hashing seguro con sal (PBKDF2/HMAC-SHA256)
│   │   │   ├── ResultadoHash.cs                # DTO inmutable con Hash y Sal
│   │   │   ├── ServicioCifrado.cs              # Cifrado simétrico autenticado (AES-256-GCM)
│   │   │   └── ServicioContrasenas.cs          # Validación de reglas de complejidad
│   │   └── Nucleo.csproj
│   │
│   ├── Datos/                                  # Librería de Clases (.NET 8) - Capa de persistencia mock
│   │   ├── IDatos.cs                           # Contrato de operaciones CRUD y gestión
│   │   ├── Datos.cs                            # Implementación en memoria DB_TPMP (Diccionarios y Listas)
│   │   └── Datos.csproj
│   │
│   └── Web/                                    # Aplicación Web ASP.NET Core
│       ├── Controllers / Pages                 # Lógica de presentación y control multiusuario
│       ├── Views / Shared                      # Interfaz web enfocada en usabilidad
│       └── Web.csproj
│
└── tests/
    └── Nucleo.Tests/                           # Proyecto de Pruebas Unitarias (MSTest.Sdk)
        ├── Seguridad/
        │   ├── ServicioHashTests.cs            # Pruebas de caja negra (ED-01 a ED-20)
        │   ├── ServicioContrasenasTests.cs     # Pruebas de caja blanca (8+ casos, 100% ramas)
        │   └── ServicioCifradoTests.cs         # Pruebas de cifrado simétrico AES
        ├── Modelos/
        │   └── UsuarioTests.cs                 # Pruebas de cambio/comprobación y funcionalidad única
        ├── Datos/
        │   └── DatosTests.cs                   # Pruebas CRUD, autoincrementales, logs y Reiniciar()
        └── Nucleo.Tests.csproj
```

---

## 4. Especificaciones Funcionales y de Dominio

### 4.1. Entidad `Usuario.cs` (Requisito Personalizado del Grupo)
1. **Atributos y Propiedades Base:**
   - `Id` (entero, autoincremental).
   - `Cuenta` (nombre de usuario identificativo único, no un correo electrónico).
   - `Nombre` y `EMail`.
   - `HashContrasena` y `Sal` (almacenamiento seguro, sin acceso en texto plano).
   - `RolId` y `Activo` (o `Estado`).
2. **Métodos Obligatorios de Interfaz:**
   - `public bool ComprobarContrasena(string contrasena)`: Verifica si el texto plano recibido coincide con el hash y sal almacenados usando `ServicioHash.Verificar`.
   - `public bool CambiarContrasena(string contrasenaActual, string nuevaContrasena)`:
     - Valida la contraseña actual.
     - Valida la complejidad de la nueva contraseña con `ServicioContrasenas.EsContrasenaSegura`.
     - Genera nuevo hash y sal con `ServicioHash.Generar` y actualiza la entidad.
3. **Funcionalidad Exclusiva de Grupo (A elegir e implementar):**
   * *Opción recomendada:* **Bloqueo tras 3 intentos fallidos consecutivos** con método para desbloqueo por administrador o tras tiempo límite.
   * *Opciones alternativas:* Historial de las 3 últimas contraseñas para evitar su reutilización, caducidad temporal de la contraseña (ej. 90 días), o generación de token temporal para reseteo.

### 4.2. Capa de Datos en Memoria (`IDatos.cs` y `Datos.cs`)
- **Simulación en memoria:** Uso de colecciones en memoria (`Dictionary<int, Usuario>`, `Dictionary<int, Rol>`, `Dictionary<int, Proyecto>` y `List<EntradaLog>`).
- **Comportamiento autoincremental:**
  - Si una entidad se guarda con `Id < 0`, se le asigna un identificador positivo secuencial único y se inserta.
  - Si una entidad posee `Id >= 0` existente, se actualiza en el diccionario.
- **Método `Reiniciar()`:** Limpia las colecciones y restablece los contadores a 0. Permite instanciar la base de datos en un estado conocido (*seed data*) en cada petición HTTP o antes de cada batería de pruebas (`[TestInitialize]`).

### 4.3. Servicios Criptográficos (`Nucleo.Seguridad`)
- **`ServicioHash`:** Algoritmo PBKDF2 (`Rfc2898DeriveBytes`) con HMAC-SHA256 y sal independiente de 16 bytes.
- **`ServicioCifrado`:** Cifrado simétrico autenticado **AES-256-GCM** (Nonce de 12 bytes, Tag de autenticación de 16 bytes, clave en Base64).
- **`ServicioContrasenas`:** Requisitos mínimos: $\ge 8$ caracteres, al menos 1 mayúscula, 1 minúscula, 1 número y 1 carácter especial o símbolo.

---

## 5. Plan Detallado de Pruebas Unitarias (MSTest.Sdk)

### 5.1. Batería de Caja Negra: `ServicioHash` (Especificación ED-01 a ED-20)
Basada en el documento oficial `GINF-6376-2026-XX-E01 Pruebas Unitarias 1.pdf`:

| ID | Método | Condición / Escenario de Entrada | Resultado Esperado |
| :--- | :--- | :--- | :--- |
| **ED-01 - ED-04** | `Generar(texto)` | Texto `null`, cadena vacía (`""`) o solo espacios (`"   "`) | Lanza `InvalidOperationException` |
| **ED-05 - ED-06** | `Generar(texto)` | Texto válido (`"MiClave123!"`) | Retorna `ResultadoHash` con `Hash` y `Sal` no nulos ni vacíos |
| **ED-07 - ED-08** | `Generar(texto)` | Dos llamadas consecutivas con el **mismo texto** | Sal 1 $\ne$ Sal 2 y Hash 1 $\ne$ Hash 2 (unicidad y variabilidad) |
| **ED-09** | `Generar(texto)` | Entrada válida | Todas las propiedades del resultado son íntegras y válidas |
| **ED-10 - ED-12** | `Verificar(...)` | Texto original, hash y sal correctos | Retorna `true` |
| **ED-13** | `Verificar(...)` | Texto diferente al utilizado para generar el hash | Retorna `false` |
| **ED-14** | `Verificar(...)` | Parámetros nulos, vacíos o solo espacios en cualquiera de los 3 argumentos | Retorna `false` |
| **ED-15 - ED-16** | `Verificar(...)` | Hash o sal corruptos o con formato Base64 incorrecto | Retorna `false` |
| **ED-17** | `Verificar(...)` | Datos almacenados incorrectos o malformados | **Robustez:** Nunca propaga excepciones no controladas, retorna `false` |

### 5.2. Batería de Caja Blanca: `ServicioContrasenas` (Complejidad Ciclomática)
Para cubrir el 100% de ramas y condiciones lógicas (mínimo 8 casos de prueba):
1. Contraseña válida completa (longitud, mayúscula, minúscula, dígito, carácter especial).
2. Menor de 8 caracteres.
3. Sin letras mayúsculas.
4. Sin letras minúsculas.
5. Sin números.
6. Sin caracteres especiales / símbolos.
7. Cadena nula o vacía.
8. Cadena que contiene espacios en blanco.

### 5.3. Batería de la Capa de Datos (`Datos.cs`)
1. Guardar nueva entidad con `Id = -1` (comprobar asignación de ID positivo).
2. Actualizar entidad existente con `Id >= 0`.
3. Lectura de entidad por ID existente y por ID inexistente (`null`).
4. Búsqueda de usuario por email existente y no existente.
5. Eliminación de entidad existente y no existente.
6. Registro de logs y comprobación del método `LimpiarLog()`.
7. Ejecución de `Reiniciar()` y verificación de vaciado de todas las colecciones.

---

## 6. Documentación y Trazabilidad Obligatoria (Memoria PDF)

La memoria PDF a entregar debe estructurarse conforme a los apartados del enunciado:

### 6.1. Apartado a: Planificación en Microsoft Planner
- Capturas del tablero digital `DniAlumno1-DniAlumno2` con depósitos por fases (Análisis, Diseño, Implementación, Pruebas, Documentación).
- Tareas con fechas límites asignadas a cada miembro del grupo y estados de avance.

### 6.2. Apartado b: Estructura de Información y Cadena de Trazabilidad
Se deben incluir las tablas completas con identificadores únicos:
$$\text{Actores (ACT)} \longrightarrow \text{Casos de Uso (CU)} \longrightarrow \text{Requisitos / HU (RE/HU)} \longrightarrow \text{Criterios de Aceptación (CA)} \longrightarrow \text{Casos de Prueba (CP)}$$

*Ejemplo de convención de identificadores:*
- `ACT-01`: Administrador, `ACT-02`: Tester/Tester Líder.
- `CU-01`: Autenticación en la plataforma, `CU-02`: Gestión de Proyectos, `CU-03`: Cambio seguro de contraseña.
- `RE-01`: Política de seguridad de contraseñas, `RE-02`: Auditoría de accesos.
- `CA-01.1`: La contraseña rechaza cadenas menores de 8 caracteres.
- `CP-01`: Caso de prueba unitario correspondiente en MSTest.

### 6.3. Apartados c, d, e: Modelado UML
- **Diagrama de Paquetes de Alto Nivel:** Representación de capas `Nucleo`, `Datos`, `Web` y `Nucleo.Tests`.
- **Diagrama de Clases de Diseño:** Incluye modelos, servicios criptográficos, capa de datos e **incluye las clases de pruebas unitarias**.
- **Diagramas UML Adicionales:** Diagrama de Casos de Uso del sistema y diagrama de secuencia de autenticación/bloqueo de usuario.

### 6.4. Apartado f: Informe de Pruebas y Cobertura
- Resumen de ejecución de pruebas unitarias, de integración, seguridad, sistema (navegadores) y usabilidad.
- Evidencias gráficas de la ventana de cobertura de Visual Studio certificando **$\ge 90\%$ de recubrimiento en `Nucleo`**.

---

## 7. Entregables Finales y Calendario

| Hito / Entregable | Detalle | Fecha / Plazo |
| :--- | :--- | :--- |
| **Tablero Kanban** | Microsoft Planner `DniAlumno1-DniAlumno2` | Durante todo el ciclo de trabajo |
| **Entrega de la Práctica** | Un único fichero `DniAlumno1-DniAlumno2.zip` en UBUVirtual con:<br>1. Memoria en PDF requerida.<br>2. Solución completa de Visual Studio con código y tests. | **28 de octubre de 2026** (23:59 h) |
| **Defensa Presencial** | Ejecución de la aplicación, paso de pruebas y revisión de cobertura $\ge 90\%$ ante el profesor. | **07 al 14 de noviembre de 2026** |

---

## 8. Checklist de Trabajo Inmediato (Paso a Paso)

- [ ] **1. Solución Visual Studio:**
  - [ ] Crear el archivo de solución `TestProjectManagementPlatform.sln`.
  - [ ] Incluir `IDatos.cs` y su archivo `Datos.csproj` en `src/Datos`.
  - [ ] Crear el proyecto `Nucleo.Tests` con `MSTest.Sdk`.
  - [ ] Configurar las referencias entre proyectos (`Nucleo.Tests` $\to$ `Nucleo` y `Datos`).
- [ ] **2. Dominio y Seguridad (`src/Nucleo`):**
  - [ ] Consensuar e implementar la funcionalidad única en `Usuario.cs` (ej. bloqueo tras 3 intentos fallidos).
  - [ ] Implementar `ComprobarContrasena` y `CambiarContrasena` sin exponer texto plano.
- [ ] **3. Batería de Pruebas Unitarias (`tests/Nucleo.Tests`):**
  - [ ] Implementar `ServicioHashTests.cs` cubriendo las especificaciones ED-01 a ED-20.
  - [ ] Implementar `ServicioContrasenasTests.cs` (8 casos para 100% cobertura).
  - [ ] Implementar `UsuarioTests.cs` validando el cambio de clave y la funcionalidad exclusiva.
  - [ ] Implementar `DatosTests.cs` verificando CRUD, auto-IDs y `Reiniciar()`.
- [ ] **4. Validación de Cobertura:**
  - [ ] Ejecutar *Analizar cobertura de código* en Visual Studio y constatar $\ge 90\%$.
- [ ] **5. Documentación y Memoria:**
  - [ ] Crear tablero en Microsoft Planner y volcar tareas.
  - [ ] Redactar la tabla de trazabilidad ACT $\to$ CU $\to$ RE $\to$ CA $\to$ CP.
  - [ ] Diseñar los diagramas UML (paquetes y clases con tests).
  - [ ] Generar la memoria final en PDF y empaquetar en `.zip`.

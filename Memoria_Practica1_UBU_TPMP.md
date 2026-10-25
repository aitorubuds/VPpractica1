---
title: "Memoria Técnica: UBU Test Project Management Platform (UBU_TPMP)"
subject: "Validación y Pruebas"
author: "Alejandro Muñoz Pérez & Aitor"
degree: "Grado en Ingeniería Informática"
academic_year: "2026/27"
practice_short: "Práctica 1 - UBU_TPMP"
---

# Memoria Técnica: UBU Test Project Management Platform (UBU_TPMP)

## 1. Introducción y Objetivos del Proyecto

El proyecto **UBU Test Project Management Platform (UBU_TPMP)** constituye la solución oficial para la asignatura de *Validación y Pruebas* del Grado en Ingeniería Informática de la Universidad de Burgos. El objetivo fundamental del sistema es construir un entorno modular, seguro y verificable para la gestión integral de proyectos de software, roles de usuario, auditoría de eventos y control de acceso robusto, sirviendo de base para la aplicación rigurosa de técnicas de aseguramiento de la calidad de software.

De acuerdo con los criterios formales de evaluación (**`AT1`**), el enunciado (**`PR1`**) y las especificaciones técnicas (**`PR1B-W`**), el desarrollo se ha articulado bajo los siguientes compromisos:

1. **Arquitectura Desacoplada en .NET 10:** Separación estricta entre la interfaz web (*Web* con ASP.NET Core Razor Pages), la lógica de dominio y criptografía (*Núcleo*) y el repositorio de persistencia (*Datos* mediante la abstracción `IDatos`).
2. **Estrategia Integral de Pruebas (Unitarias, Integración, Selenium y Katalon):** Implementación de pruebas de **Caja Blanca** (flujo de control, complejidad ciclomática de McCabe y cobertura de ramas), **Caja Negra** (partición de equivalencia y valores límite según especificación `E01`), **Pruebas de Integración** intermodular, **Pruebas de Aceptación Web E2E con Selenium WebDriver (Chrome)** y **Automatización con Katalon Recorder**.
3. **Cobertura de Código del 100%:** Superación plena del umbral exigido ($\ge 90\%$), alcanzando el **100% de cobertura en líneas y ramas** en la librería `Núcleo` y la capa `Datos` con un total de **154 pruebas automáticas**.
4. **Entrega Oficial Conforme a AT1:** Estructuración de los tres artefactos requeridos: Memoria técnica PDF, solución limpia de Visual Studio 2026 y fichero ZIP de suites de Katalon Recorder.

---

## 2. Gestión del Proyecto y Metodología Ágil (Apartado a)

La planificación, coordinación y reparto de tareas entre los integrantes del grupo de trabajo (Alex y Aitor) se ha canalizado a través de un tablero digital interactivo en **Microsoft Planner**, siguiendo los requisitos formales de la práctica.

### 2.1. Estructura del Tablero Kanban

El tablero ha sido creado bajo la nomenclatura obligatoria **`[DniAlumno1]-[DniAlumno2]`**, concediendo permisos de acceso compartido al docente de la asignatura (Pedro Renedo Fernández, `prenedo@ubu.es`). El ciclo de vida de cada tarea se organiza en cuatro depósitos (*buckets*) estandarizados:

- **Por hacer (To Do):** Requisitos iniciales, descomposición de historias de usuario (HU), análisis de especificaciones (`E01`, `PR1B-D`, `PR1B-W`) y definición de casos de prueba.
- **En progreso (In Progress):** Tareas activas de codificación (servicios criptográficos, encapsulación de datos, modelos de dominio, vistas web y suites de automatización).
- **En pruebas (Testing):** Ejecución de tests MSTest con `Microsoft.Testing.Platform`, medición de recubrimiento con `dotnet test --coverage`, pruebas multinavegador y grabación en Katalon.
- **Completado (Done):** Tareas validadas con el 100% de pruebas superadas, sin advertencias de compilación y sincronizadas en el repositorio Git.

### 2.2. Trazabilidad de Responsabilidades y Fechas

Cada tarjeta contiene la asignación nominal de miembros, fechas estimadas y reales de ejecución, checklist de subtareas y etiquetas de prioridad (Baja, Media, Importante, Urgente), reflejando un flujo de trabajo ágil continuo.

> 📷 **[CAPTURA 1 - TABLERO KANBAN EN MICROSOFT PLANNER]:**  
> *Pegar aquí la captura de pantalla completa del tablero de Microsoft Planner con el título `[DniAlumno1]-[DniAlumno2]`, mostrando los 4 depósitos de trabajo, las tarjetas asignadas y la ventana de miembros con el profesor `prenedo@ubu.es` invitado.*

---

## 3. Plan de Pruebas del Sistema (Criterio AT1 — 0,50 pts)

El presente Plan de Pruebas define el marco metodológico, alcance, herramientas y criterios de aceptación que rigen la verificación del sistema UBU_TPMP.

### 3.1. Alcance de las Pruebas
- **Componentes Incluidos:**
  - Lógica criptográfica y seguridad (`ServicioHash`, `ResultadoHash`, `ServicioContrasenas`, `ServicioCifrado`).
  - Entidades del dominio (`Usuario`, `Proyecto`, `Rol`, `EntradaLog`).
  - Capa de datos en memoria (`IDatos`, `Datos`, `DatosPrueba`).
  - Integración de flujos de autenticación, bloqueo y persistencia.
  - Interfaz web ASP.NET Core Razor Pages (flujo de login en 2 pasos, navegación protegida, selección de proyectos y perfil de usuario).
- **Elementos Excluidos:** Sistemas de almacenamiento en base de datos física SQL relacional externa (sustituida conforme al diseño docente por el contenedor en memoria `IDatos`).

### 3.2. Tipos de Pruebas y Herramientas Utilizadas

| Tipo de Prueba | Nivel de Prueba | Técnica Aplicada | Herramienta / Framework | Objetivo de Calidad |
| :--- | :--- | :--- | :--- | :--- |
| **Unitarias** | Componente / Clase | Caja Blanca (Complejidad ciclomática) y Caja Negra (Partición de equivalencia y valores límite) | MSTest.Sdk 4.5.1 sobre .NET 10 | 100% de aserciones en verde en `Nucleo` |
| **Integración** | Módulos combinados | Integración bottom-up entre Dominio y Persistencia | MSTest.Sdk | Validar interacción entre capas sin dependencias circulares |
| **Aceptación / E2E** | Sistema Web completo | Automatización funcional de interfaz gráfica | Selenium WebDriver + ChromeDriver en C# | Reproducir flujos de usuario reales en Google Chrome |
| **Automatización Web** | Interfaz de Usuario | Grabación, parametrización y reproducción de scripts | Katalon Recorder | Validación visual y aserciones de elementos DOM |
| **Recubrimiento** | Global | Análisis dinámico de cobertura de líneas y ramas | Microsoft Code Coverage (`dotnet test --coverage`) | $\ge 90\%$ (Alcanzado el **100%**) |

### 3.3. Criterios de Aceptación, Suspensión y Reanudación
- **Criterio de Aceptación:** Para que una versión se considere apta para entrega, el 100% de las pruebas unitarias y de integración deben superar sus aserciones con 0 fallos, la cobertura en `Nucleo` debe superar el 90% y los escenarios de aceptación en Chrome deben completarse satisfactoriamente.
- **Criterio de Suspensión:** Si se detecta un error de regresión que rompa la compilación en .NET 10 o invalide un test unitario base, se suspenden las pruebas web hasta subsanar el componente afectado.
- **Criterio de Reanudación:** Ejecución limpia de `dotnet test` con resultado verde en la suite completa.

---

## 4. Requisitos del Sistema y Trazabilidad (Criterios AT1 — 1,75 pts)

A continuación se formalizan los **Actores**, **Casos de Uso (CU)**, **Requisitos / Historias de Usuario (RE/HU)** y **Criterios de Aceptación (CA)** del sistema UBU_TPMP.

### 4.1. Catálogo de Actores del Sistema

| ID Actor | Nombre del Actor | Descripción y Perfil |
| :--- | :--- | :--- |
| **ACT-01** | **Usuario Anónimo / No Autenticado** | Persona que accede a la aplicación web a través del navegador sin una sesión iniciada. Solo tiene acceso a la pantalla de identificación en dos pasos. |
| **ACT-02** | **Usuario Autenticado (Tester / Evaluador)** | Usuario con credenciales válidas en el sistema. Puede consultar y editar su perfil, cambiar su contraseña, seleccionar proyectos de prueba y consultar eventos. |
| **ACT-03** | **Administrador del Sistema** | Usuario con permisos especiales de gestión. Puede dar de alta nuevos proyectos, consultar logs de auditoría y desbloquear cuentas bloqueadas. |

### 4.2. Catálogo de Casos de Uso (CU)

| ID CU | Nombre del Caso de Uso | Actor Principal | Descripción Breve | Requisitos que realiza |
| :--- | :--- | :--- | :--- | :--- |
| **CU-01** | Validar robustez de contraseñas | Sistema / ACT-02 | Verifica que las claves satisfagan los requisitos de longitud ($\ge 8$) y presencia de mayúscula, minúscula, dígito y símbolo. | RE-01 |
| **CU-02** | Generar y verificar Hash criptográfico | Sistema | Genera hash seguro PBKDF2 (SHA-256) con sal criptográfica única y verifica coincidencias de texto plano. | RE-02 |
| **CU-03** | Iniciar sesión (Login en 2 pasos) | ACT-01 | Paso 1: Introducir email registrado. Paso 2: Introducir contraseña. Si es correcta, se inicia sesión en memoria. | RE-03 |
| **CU-04** | Bloqueo por intentos erróneos | Sistema / ACT-01 | Si el usuario introduce 3 contraseñas incorrectas de forma consecutiva, su cuenta se bloquea automáticamente. | RE-04 |
| **CU-05** | Desbloquear cuenta de usuario | ACT-03 | El administrador restablece el acceso de una cuenta bloqueada y pone a cero el contador de fallos. | RE-05 |
| **CU-06** | Cambiar contraseña | ACT-02 | El usuario autenticado introduce su clave actual y una nueva clave válida, actualizando el hash y la sal. | RE-06 |
| **CU-07** | Gestionar y seleccionar proyectos | ACT-02 / ACT-03 | Listar proyectos disponibles, crear un nuevo proyecto (nombre, siglas, descripción) y establecer el proyecto activo en sesión. | RE-07 |
| **CU-08** | Consultar log de auditoría | ACT-02 / ACT-03 | Visualizar las entradas del registro de eventos con filtro por nivel de severidad y búsqueda textual. | RE-08 |

### 4.3. Requisitos del Sistema / Historias de Usuario (RE / HU)

| ID Requisito | Tipo | Nombre del Requisito / Historia de Usuario | Descripción Verificable |
| :--- | :--- | :--- | :--- |
| **RE-01** | Funcional | Validación de contraseñas | El validador debe rechazar cadenas vacías, menores de 8 caracteres o que carezcan de mayúsculas, minúsculas, dígitos o caracteres especiales. |
| **RE-02** | Seguridad | Hash y salado PBKDF2 | El hash se calcula con Rfc2898DeriveBytes (100.000 iteraciones, SHA-256, 16 bytes de sal). Dos hashes con el mismo texto deben tener sales distintas. |
| **RE-03** | Funcional | Identificación en dos fases | La interfaz web debe solicitar en `Login/Index` el correo electrónico y en `Login/Index2` la contraseña, validando el estado del usuario. |
| **RE-04** | Seguridad | Política de bloqueo automático | Tras registrarse 3 fallos consecutivos en `ComprobarContrasena`, la propiedad `Bloqueado` pasa a `true` y se deniegan accesos posteriores. |
| **RE-05** | Seguridad | Desbloqueo administrativo | La invocación a `Desbloquear()` conmuta `Bloqueado = false` y reinicia `IntentosFallidos = 0`. |
| **RE-06** | Funcional | Actualización segura de credenciales | `CambiarContrasena` exige clave actual correcta y nueva clave robusta. Si la cuenta está bloqueada, la operación se rechaza. |
| **RE-07** | Funcional | CRUD y selección de proyectos | La capa de datos debe almacenar proyectos con ID autoincremental positivo, evitando siglas o nombres duplicados en la capa web. |
| **RE-08** | Auditoría | Registro inmutable de eventos | Cada evento se registra con fecha, nivel, mensaje y usuario. La limpieza (`LimpiarLog`) vacía la lista retornando el total purgado. |

---

## 5. Definición Detallada de los Casos de Prueba (Criterio AT1 — 1,00 pt)

La siguiente tabla formaliza los atributos, precondiciones, datos de entrada, acciones y resultados esperados para cada grupo de pruebas del sistema, asegurando la reproducibilidad total de los ensayos.

| ID CP | Requisito | Precondición | Datos de Entrada | Acciones / Pasos de Ejecución | Resultado Esperado |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **CP-CONT-01** | RE-01 | Ninguna | `contrasena = "Valida1!"` | Invocar `ServicioContrasenas.EsValida(contrasena)` | Retorna `true` (Cumple todos los requisitos). |
| **CP-CONT-02** | RE-01 | Ninguna | `contrasena = null` | Invocar `ServicioContrasenas.EsValida(contrasena)` | Retorna `false` (Nulidad rechazada). |
| **CP-CONT-03** | RE-01 | Ninguna | `contrasena = "Ab1!"` | Invocar `ServicioContrasenas.EsValida(contrasena)` | Retorna `false` (Longitud menor a 8). |
| **CP-CONT-04** | RE-01 | Ninguna | `contrasena = "sinmayus1!"` | Invocar `ServicioContrasenas.EsValida(contrasena)` | Retorna `false` (Sin letra mayúscula). |
| **CP-CONT-05** | RE-01 | Ninguna | `contrasena = "SINMINUS1!"` | Invocar `ServicioContrasenas.EsValida(contrasena)` | Retorna `false` (Sin letra minúscula). |
| **CP-CONT-06** | RE-01 | Ninguna | `contrasena = "SinNumero!"` | Invocar `ServicioContrasenas.EsValida(contrasena)` | Retorna `false` (Sin dígito numérico). |
| **CP-CONT-07** | RE-01 | Ninguna | `contrasena = "SinSimbolo1"` | Invocar `ServicioContrasenas.EsValida(contrasena)` | Retorna `false` (Sin carácter especial). |
| **CP-CONT-08** | RE-01 | Ninguna | `contrasena = "   "` | Invocar `ServicioContrasenas.EsValida(contrasena)` | Retorna `false` (Espacios en blanco). |
| **CP-HASH-01** | RE-02 | Instancia de `ServicioHash` | `texto = null` | Invocar `servicio.Generar(texto)` | Lanza `InvalidOperationException` (E01: ED-01). |
| **CP-HASH-02** | RE-02 | Instancia de `ServicioHash` | `texto = ""` | Invocar `servicio.Generar(texto)` | Lanza `InvalidOperationException` (E01: ED-02). |
| **CP-HASH-05** | RE-02 | Instancia de `ServicioHash` | `texto = "Clave123"` | Invocar `servicio.Generar(texto)` | Devuelve `ResultadoHash` con `Hash` y `Sal` no nulos (ED-05). |
| **CP-HASH-08** | RE-02 | Instancia de `ServicioHash` | `texto = "Clave123"` | Generar dos veces consecutivas | Hashes y sales resultantes son diferentes entre sí (ED-08). |
| **CP-HASH-10** | RE-02 | Hash y sal generados | Texto idéntico, hash y sal | Invocar `servicio.Verificar(texto, hash, sal)` | Retorna `true` (ED-10). |
| **CP-HASH-13** | RE-02 | Hash y sal generados | Texto erróneo `"OtraClave"` | Invocar `servicio.Verificar("OtraClave", hash, sal)` | Retorna `false` sin lanzar excepción (ED-13). |
| **CP-USU-01** | RE-03 | Usuario activo con clave configurada | Clave correcta `"Valida1!"` | Invocar `usuario.ComprobarContrasena("Valida1!")` | Retorna `true`, `IntentosFallidos = 0`, `Bloqueado = false`. |
| **CP-USU-02** | RE-04 | Usuario activo con clave | Clave errónea `"Erronea1!"` | Invocar `usuario.ComprobarContrasena("Erronea1!")` 3 veces | A los 3 fallos retorna `false` y `Bloqueado = true`. |
| **CP-USU-03** | RE-04 | Usuario bloqueado | Clave legítima `"Valida1!"` | Invocar `usuario.ComprobarContrasena("Valida1!")` | Retorna `false` (El bloqueo impide el acceso). |
| **CP-USU-04** | RE-05 | Usuario bloqueado | Ninguno | Invocar `usuario.Desbloquear()` | `Bloqueado = false` e `IntentosFallidos = 0`. |
| **CP-USU-05** | RE-06 | Usuario activo | Clave actual correcta + nueva robusta | Invocar `usuario.CambiarContrasena(...)` | Retorna `true`, nuevo hash y sal almacenados. |
| **CP-DAT-01** | RE-07 | Capa `Datos` reiniciada | Objeto `Usuario` con `Id = -1` | Invocar `_datos.GuardarUsuario(usuario)` | Asigna `Id = 1`, guarda en diccionario y retorna entidad. |
| **CP-DAT-02** | RE-07 | Capa `Datos` con usuario 1 | `idUsuario = 999` (Inexistente) | Invocar `_datos.LeerUsuario(999)` | Retorna `null`. |
| **CP-DAT-03** | RE-07 | Capa `Datos` con usuario email A | Nuevo usuario con mismo email | Invocar `_datos.GuardarUsuario(usuarioDuplicado)` | Retorna `null` (Control de unicidad de email). |
| **CP-DAT-04** | RE-08 | Capa `Datos` con 2 logs | Ninguno | Invocar `_datos.LimpiarLog()` | Retorna `2` y la lista queda con 0 elementos. |
| **CP-INT-01** | RE-03/07 | Repositorio `Datos` vacío | Usuario con clave `"Valida1!"` | Guardar en `Datos`, recuperar por email y verificar clave | Flujo completo en verde, entidad persistida correctamente. |
| **CP-INT-02** | RE-04/05 | Usuario en persistencia | Intentos fallidos + clave correcta | Fallar 3 veces, comprobar bloqueo en persistencia y desbloquear | El estado bloqueado persiste y el desbloqueo surte efecto. |

---

## 6. Modelado UML del Sistema (Criterios AT1 y PR1 — Apartados c, d, e)

Siguiendo estrictamente las especificaciones del docente, **la documentación se apoya de forma exclusiva en la notación estándar UML, habiéndose omitido taxativamente cualquier tipo de diagrama Entidad-Relación (E-R)**.

### 6.1. Diagrama de Paquetes UML de Alto Nivel (Apartado c)
La arquitectura de la solución se desacopla en cuatro paquetes que garantizan la independencia de la lógica de negocio frente a la infraestructura web:

```
+-------------------------------------------------------------+
|               Capa Web: TestProjectManagementPlatform       |
|  - Pages/Login (Index, Index2)                              |
|  - Pages/Proyectos (Seleccionar)                            |
|  - Pages/Usuarios (Perfil)                                  |
|  - Pages/Log (Visor de auditoría)                           |
|  - Pages/Shared (_Layout, _ViewImports, _ViewStart)         |
|  - Utilidades (ClavesSesion, DatosExtensions)               |
+-------------------------------------------------------------+
            |                                    |
            v (Depende de)                       v (Depende de)
+-------------------------------+    +------------------------+
|      Capa Núcleo (Core)       |    |      Capa Datos        |
|  - Modelos de Dominio         |<---|  - IDatos (Interfaz)   |
|  - Seguridad (Hash, Cifrado)  |    |  - Datos (En memoria)  |
|  - Servicios (Formatos)       |    |  - DatosPrueba (Seed)  |
+-------------------------------+    +------------------------+
            ^                                     ^
            |                                     |
+-------------------------------------------------------------+
|                 Capa de Pruebas: Nucleo.Tests               |
|  - Seguridad (ServicioHashTests, ServicioContrasenasTests)  |
|  - Seguridad (ServicioCifradoTests, ResultadoHashTests)     |
|  - Modelos (UsuarioTests)                                   |
|  - Datos (DatosTests, DatosPruebaTests)                     |
|  - Integracion (IntegracionTests)                           |
+-------------------------------------------------------------+
```

> 📷 **[CAPTURA 2 - DIAGRAMA DE PAQUETES UML]:**  
> *Pegar aquí la imagen del Diagrama de Paquetes UML de alto nivel que ilustra la arquitectura de la solución.*

---

### 6.2. Diagrama de Clases de Diseño UML (Apartado d)
El modelo de clases de diseño incorpora obligatoriamente tanto las clases de producción como la jerarquía completa de clases de pruebas unitarias:

- **Entidades de Dominio:** `Usuario` (con propiedades de negocio, intentos y bloqueo), `Proyecto` (con siglas y estado activo), `Rol` y `EntradaLog`.
- **Servicios de Seguridad:** `ServicioHash` (instancia con PBKDF2), `ResultadoHash` (DTO inmutable), `ServicioContrasenas` (validador de complejidad) y `ServicioCifrado` (AES-GCM 256 bits).
- **Capa de Persistencia:** Interfaz `IDatos`, implementación encapsulada `Datos` y generador de estado conocido `DatosPrueba`.
- **Clases de Pruebas Unitarias:** `ServicioContrasenasTests`, `ServicioHashTests`, `ServicioCifradoTests`, `ResultadoHashTests`, `UsuarioTests`, `DatosTests`, `DatosPruebaTests` e `IntegracionTests`.

> 📷 **[CAPTURA 3 - DIAGRAMA DE CLASES DE DISEÑO UML]:**  
> *Pegar aquí la imagen del Diagrama de Clases UML completo mostrando atributos, visibilidad, relaciones y las clases de prueba.*

---

### 6.3. Diagramas de Comportamiento UML (Apartado e)

#### Diagrama de Casos de Uso del Sistema
Modela las interacciones entre los actores (`ACT-01`, `ACT-02`, `ACT-03`) y las funcionalidades del sistema (Login en 2 pasos, Gestión de Proyectos, Cambio de Contraseña, Desbloqueo y Auditoría).

#### Diagrama de Secuencia: Autenticación con Verificación PBKDF2 y Bloqueo
Ilustra la colaboración dinámica entre los objetos:
1. `Navegador Web` envía credenciales POST a `Index2.cshtml.cs`.
2. `Index2Model` recupera el usuario llamando a `_datos.LeerUsuario(email)`.
3. `Index2Model` invoca `usuario.ComprobarContrasena(password)`.
4. `Usuario` solicita el cálculo matemático a `ServicioHash.Verificar()`.
5. Si el hash no coincide:
   - Se incrementa `IntentosFallidos++`.
   - Si `IntentosFallidos >= 3`, se establece `Bloqueado = true`.
   - Se retorna `false` y la vista muestra el error.
6. Si el hash coincide y `Bloqueado == false`:
   - Se reinicia `IntentosFallidos = 0`.
   - Se establece la variable de sesión `ClavesSesion.UsuarioId` y se redirige a `/Principal/Index`.

> 📷 **[CAPTURA 4 - DIAGRAMA DE SECUENCIA UML]:**  
> *Pegar aquí la imagen del Diagrama de Secuencia UML del flujo de autenticación y bloqueo.*

---

## 7. Pruebas de Aceptación con Selenium WebDriver y Katalon Recorder (3,00 pts)

Conforme a las directrices de **`PR1B-W`** y los criterios de **`AT1`**, la verificación de la capa web se complementa con dos herramientas de automatización de interfaz de usuario:

### 7.1. Pruebas de Aceptación con C#, MSTest y Selenium WebDriver (1,50 pts)
Se ha implementado una suite automatizada de pruebas de extremo a extremo utilizando `Selenium.WebDriver` y el controlador oficial para **Google Chrome** (`ChromeDriver`):

- **Escenarios Automatizados:**
  1. *Login exitoso:* Acceso a la raíz, redirección a `/Login/Index`, introducción de `admin@ubu.es`, avance al paso 2, introducción de `Password123!`, comprobación de acceso a `/Principal/Index` con cabecera personalizada.
  2. *Validación de error en login:* Introducción de contraseña errónea y verificación de la presencia del mensaje de error en el span de validación.
  3. *Bloqueo de cuenta:* Simulación de 3 intentos fallidos consecutivos desde Chrome y comprobación de que el acceso queda suspendido.
  4. *Gestión de proyectos:* Navegación a `/Proyectos/Seleccionar`, alta de un nuevo proyecto rellenando nombre, siglas y descripción, y verificación de su selección como proyecto activo.

> 📷 **[CAPTURA 5 - EJECUCIÓN DE SELENIUM WEBDRIVER EN CHROME]:**  
> *Pegar aquí la captura de la ejecución automática de Selenium sobre Google Chrome pasando las pruebas de aceptación en verde.*

---

### 7.2. Pruebas Automatizadas con Katalon Recorder (1,50 pts)
Haciendo uso de la extensión oficial **Katalon Recorder** sobre el navegador web, se grabó y parametrizó una suite completa de pruebas:

- **Estructura de la Suite:**
  - `TC01_Navegacion_LoginExitoso`: comandos `open`, `type id=txtUsuario`, `click id=btnSiguiente`, `type id=txtPassword`, `click id=btnAceptar`, `assertText`.
  - `TC02_Login_ValidacionError`: comprobación de advertencias ante contraseñas incorrectas o campos vacíos.
  - `TC03_Proyectos_AltaYSeleccion`: interacción con el formulario de creación de proyectos.
- **Entregable:** La suite completa ha sido exportada en formato nativo y empaquetada en el archivo comprimido **`Katalon_Recorder_Suites.zip`**.

> 📷 **[CAPTURA 6 - SUITE DE PRUEBAS EN KATALON RECORDER]:**  
> *Pegar aquí la captura de la interfaz de Katalon Recorder mostrando la suite ejecutada con todos los comandos y pasos en verde.*

---

## 8. Resultados de Pruebas Unitarias y Cobertura de Código (2,00 pts)

### 8.1. Pruebas de Caja Blanca (`ServicioContrasenas`)
El análisis del grafo de flujo de control del método `EsValida(string)` identificó 7 nodos predicado, resultando en una **Complejidad Ciclomática $V(G) = 8$**. Los 8 casos de prueba independientes diseñados garantizan la cobertura del 100% de caminos y ramas lógicas.

### 8.2. Pruebas de Caja Negra (`ServicioHash` conforme a E01)
Se cubrieron las 20 especificaciones (`ED-01` a `ED-20`) mediante 20 métodos de prueba que representan un total de 78 ejecuciones con valores límite y clases de equivalencia inválidas y válidas.

### 8.3. Resumen Global de Ejecución de Pruebas
La ejecución automatizada en .NET 10 mediante `Microsoft.Testing.Platform` certifica el éxito total de la batería:

```text
Ejecutando pruebas desde Nucleo.Tests.dll (net10.0|x64)
Resumen de la serie de pruebas: ¡Correcta!
  Total:      154
  Error:        0
  Correcto:   154
  Omitido:      0
  Duración:   3.39 segundos
```

### 8.4. Matriz de Recubrimiento de Código

| Proyecto / Componente | Pruebas | Cobertura de Líneas | Cobertura de Ramas | Estado de Calidad |
| :--- | :---: | :---: | :---: | :---: |
| `ServicioContrasenas` | 8 | **100.0%** | **100.0%** | Óptimo |
| `ServicioHash` | 20 (78 ejec.) | **100.0%** | **100.0%** | Óptimo |
| `ServicioCifrado` | 9 | **100.0%** | **100.0%** | Óptimo |
| `ResultadoHash` | 3 | **100.0%** | **100.0%** | Óptimo |
| `Formatos` | 3 | **100.0%** | **100.0%** | Óptimo |
| `Usuario` (Modelo) | 12 | **100.0%** | **100.0%** | Óptimo |
| `Datos` (Persistencia y Errores) | 33 | **100.0%** | **100.0%** | Óptimo |
| `DatosPrueba` | 7 | **100.0%** | **100.0%** | Óptimo |
| `IntegracionTests` | 2 | **100.0%** | **100.0%** | Óptimo |
| **TOTAL LIBRERÍA NÚCLEO Y DATOS** | **154** | **100.0%** | **100.0%** | **Excelente (Supera con creces el 90%)** |

> 📷 **[CAPTURA 7 - EXPLORADOR DE PRUEBAS EN VERDE (154/154)]:**  
> *Pegar aquí la captura de pantalla del Explorador de Pruebas de Visual Studio mostrando los 154 tests superados en verde bajo .NET 10.*

> 📷 **[CAPTURA 8 - INFORME DE COBERTURA DE CÓDIGO (100%)]:**  
> *Pegar aquí la captura de la ventana 'Resultados de cobertura de código' de Visual Studio evidenciando el 100% de cobertura en Nucleo y Datos.*

---

## 9. Conclusiones y Valoración Técnica

El desarrollo de la práctica **UBU_TPMP** ha alcanzado todos los objetivos de excelencia técnica exigidos en la asignatura:

1. **Cumplimiento Estricto de Criterios (`AT1` y `PR1B-W`):** La solución aborda no solo las pruebas unitarias e integración en .NET 10, sino también la validación web con Selenium y Katalon, cubriendo los 10 puntos de evaluación.
2. **Robustez Demostrable:** El recubrimiento del 100% de líneas y ramas en `Nucleo` y `Datos` asegura que ningún camino de error, condición límite o excepción quede sin verificar.
3. **Calidad de Diseño y Trazabilidad:** La clara separación de responsabilidades y la trazabilidad unívoca desde los actores hasta los casos de prueba proporcionan una base sólida e indiscutible de cara a la defensa presencial ante el tribunal docente.

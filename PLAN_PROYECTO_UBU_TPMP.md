# Plan de Proyecto: UBU_TPMP (Test Project Management Platform)

## 1. Resumen del Proyecto

El proyecto **UBU_TPMP** es una plataforma web para la gestión de proyectos de pruebas desarrollada en el marco de la asignatura *Validación y Pruebas* (Curso 2025-2026 / 2026-2027) de la Universidad de Burgos. 

El objetivo principal es implementar y validar una aplicación multiusuario basada en **ASP.NET** y **C#** (Visual Studio Community 2026 / .NET 8), acompañada de un conjunto integral de pruebas unitarias, de integración, seguridad, sistema y usabilidad construidas mediante **MSTest.Sdk** y **Microsoft Code Coverage**.

---

## 2. Estructura y Arquitectura de la Solución

La solución de Visual Studio se organizará según la siguiente estructura modular en capas:

```text
TestProjectManagementPlatform.sln
├── src/
│   ├── Nucleo/                                 # Biblioteca de clases (.NET 8)
│   │   ├── Modelos/                            # Entidades del dominio
│   │   │   ├── Usuario.cs
│   │   │   ├── Rol.cs
│   │   │   ├── Proyecto.cs
│   │   │   └── EntradaLog.cs
│   │   └── Seguridad/                          # Servicios criptográficos
│   │       ├── ServicioHash.cs
│   │       ├── ResultadoHash.cs
│   │       ├── ServicioCifrado.cs
│   │       └── ServicioContrasenas.cs
│   ├── Datos/                                  # Capa de datos en memoria (.NET 8)
│   │   ├── IDatos.cs                           # Interfaz de capa de datos
│   │   └── Datos.cs                            # Implementación DB_TPMP con diccionarios
│   └── Web/                                    # Sitio Web ASP.NET Core
│       ├── Controllers / Pages                 # Controladores y páginas de la app
│       └── Views / Shared                      # Vistas y componentes de interfaz
└── tests/
    └── Nucleo.Tests/                           # Proyecto de pruebas (MSTest.Sdk)
        ├── Seguridad/                          # Pruebas de ServicioHash, Cifrado y Contraseñas
        ├── Datos/                              # Pruebas de la clase Datos e IDatos
        └── Cobertura/                          # Análisis de cobertura con Microsoft Code Coverage
```

---

## 3. Especificaciones Funcionales y de Datos

### 3.1. Entidades del Sistema
- **Usuario:** Contiene `Id` (int), `Cuenta` (identificador único, no email), `Nombre`, `Email`, `HashContrasena`, `Sal`, `RolId` y `Estado`.
- **Rol:** Contiene `Id` (int), `Nombre` y `Permisos` asociados a cada área de la aplicación.
- **Proyecto:** Contiene `Id` (int), `Nombre`, `Descripcion`, `FechaCreacion` y la relación de usuarios asignados.
- **EntradaLog:** Contiene `Id` (int), `FechaHora`, `UsuarioId`, `Accion` y `Detalles` para auditoría de seguridad.

### 3.2. Capa de Datos en Memoria (`Datos.cs` e `IDatos.cs`)
- **Mecanismo de Almacenamiento:** Uso interno de diccionarios (`Dictionary<int, Usuario>`, `Dictionary<int, Rol>`, `Dictionary<int, Proyecto>`) y listas (`List<EntradaLog>`).
- **Gestión de Identificadores Autoincrementales:**
  - Cuando una entidad se almacena con un `Id < 0`, se le asigna automáticamente un nuevo identificador único autoincremental positivo y se inserta.
  - Cuando la entidad cuenta con un `Id >= 0` existente, la operación actualiza la información almacenada.
- **Simulación Dinámica de BD:** La base de datos es estática y creada dinámicamente en cada request HTTP del servidor web mediante la función `Reiniciar()`, la cual restablece el estado con datos conocidos (*seed data*).

### 3.3. Seguridad y Cifrado
- **Generación y Verificación de Hashes (`ServicioHash`):**
  - Almacenamiento seguro de contraseñas mediante derivación de claves con PBKDF2 (`Rfc2898DeriveBytes`), HMAC-SHA256 y sal independiente de 16 bytes.
  - Cada generación para un mismo texto genera una sal y hash únicos.
- **Cifrado Simétrico (`ServicioCifrado`):**
  - Cifrado y descifrado de información sensible utilizando **AES-256-GCM** (Nonce de 12 bytes, Etiqueta de autenticación de 16 bytes, clave de 32 bytes en Base64).
- **Políticas de Contraseñas (`ServicioContrasenas`):**
  - Validación de contraseña segura: mínimo 8 caracteres, al menos una mayúscula, una minúscula, un dígito y un carácter no alfanumérico.

---

## 4. Estrategia y Plan de Pruebas (MSTest.Sdk)

Las pruebas unitarias y de integración se desarrollarán en el proyecto `Nucleo.Tests` configurado con **MSTest.Sdk** (`3.10.2`+) e integrando **Microsoft Code Coverage**.

### 4.1. Escenarios de Pruebas a Cubrir (Requisitos ED-01 a ED-20 y Sección B)
1. **Pruebas de `ServicioHash`:**
   - Validación de entradas no válidas (lanzamiento de `InvalidOperationException` para nulos, cadenas vacías o espacios).
   - Generación de sal única y hashes distintos en ejecuciones sucesivas con el mismo texto.
   - Verificación correcta y manejo controlado de datos malformados sin propagación de excepciones no deseadas.
2. **Pruebas de la clase `Datos`:**
   - Inserción y actualización de Usuarios, Roles y Proyectos.
   - Verificación de la asignación automática de identificadores autoincrementales.
   - Recuperación de entidades por código y recuperación de colecciones completas.
   - Eliminación de entidades y registro de entradas de log.
   - Verificación del comportamiento ante búsquedas de elementos inexistentes (`null`).
   - Prueba del método `Reiniciar()` para comprobación del estado inicial.
3. **Tipologías de Pruebas Integradas:**
   - Pruebas unitarias de caja negra y caja blanca.
   - Pruebas de integración entre la capa de servicios de seguridad y la capa de datos.
   - Pruebas de seguridad (autenticación, auditoría y cifrado).
   - Pruebas de sistema y usabilidad.

---

## 5. Entregables del Proyecto

De acuerdo con las especificaciones del documento principal de la práctica, la entrega constará de:

1. **Tablero Kanban Digital:** Creado en *Planner* de Office 365 con el nombre `DniAlumno1-DniAlumno2`.
2. **Documentación del Análisis y Validación (PDFs):**
   - **Apartado b:** Definición de Actores, Casos de Uso, Requisitos/Historias de Usuario, Criterios de Aceptación, Casos de Prueba y Matriz de Trazabilidad con identificadores únicos (`CU1`, `RE7`, `CP-01`, etc.).
   - **Apartado c:** Modelo de alto nivel con diagrama de paquetes del sistema.
   - **Apartado d:** Modelo de clases de diseño incluyendo las clases de pruebas.
   - **Apartado e:** Diagramas UML adicionales (secuencia, estados, etc.).
   - **Apartado f:** Documentación detallada de las pruebas y resultados de ejecución de cobertura.
3. **Código Fuente del Proyecto:** Solución completa de Visual Studio con la aplicación web y el proyecto de pruebas unitarias.
4. **Fichero Comprimido Final:** Un único archivo llamado `DniAlumno1-DniAlumno2.zip`.

---

## 6. Calendario y Fechas Clave

- **Fecha límite de entrega:** 28/10/2026 a las 23:59.
- **Defensa de la práctica:** Del 07/11/2026 al 14/11/2026.

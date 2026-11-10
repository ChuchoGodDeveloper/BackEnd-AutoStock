---
trigger: model_decision
description: "Aplicar al trabajar con C#, ASP.NET Core, APIs, controladores o bases de datos PostgreSQL."
---

# Reglas de Backend, Clean Architecture, Endpoints, Seguridad y Base de Datos (AutoStock)

## 1. Arquitectura en Capas (.NET 10 Clean Architecture)
* **Stack:** .NET 10 con ASP.NET Core Web API.
* **Clean Architecture Estricta en 5 Proyectos Independientes:**
  1. `src/AutoStock.Domain`: Entidades de dominio, enums y constantes (`CatalogoMarcasModelos.cs`).
  2. `src/AutoStock.Application`: Interfaces, DTOs, excepciones de negocio y casos de uso/servicios (`AuthService`, `VehiculosService`, `UsuariosService`, `PermisosService`).
  3. `src/AutoStock.Infrastructure`: Persistencia con Entity Framework Core (`AutoStockDbContext`), repositorios, `UnitOfWork`, seguridad (`JwtTokenGenerator`, `PasswordHasher`), `EmailService` y `DbInitializer`.
  4. `src/AutoStock.API`: Controladores REST (`AuthController`, `VehiculosController`, `UsuariosController`, `PermisosController`, `MetricasController`), `ExceptionHandlingMiddleware` y configuración DI en `Program.cs`.
  5. `tests/AutoStock.UnitTests`: Pruebas unitarias con **xUnit** y **Moq** (110/110 pruebas en verde hasta HU-16).
* **Regla Inquebrantable:** Está prohibido usar un proyecto monolítico separado únicamente por carpetas. Todas las reglas de negocio deben contar con pruebas unitarias en `AutoStock.UnitTests`.
* **Calidad de Código:** Análisis estático integrado con `Roslynator.Analyzers` en `Directory.Build.props` y reglas en `.editorconfig` (0 errores y 0 advertencias en compilación `Release`).

---

## 2. Base de Datos y Persistencia (SQL Server + Entity Framework Core)
* **Motor Relacional:** SQL Server (Azure SQL en producción: `AutoStockDb` en `autostock-sql-srv-2026.database.windows.net`). Prohibido el uso de bases NoSQL.
* **Configuración Fluent API:** Relaciones, claves primarias numéricas (`Id` Identity), claves foráneas e índices definidos en `src/AutoStock.Infrastructure/Persistence/Configurations/`.
* **Versionamiento mediante Migraciones:** Todas las migraciones de EF Core se versionan en `src/AutoStock.Infrastructure/Persistence/Migrations/`. Prohibido realizar cambios estructurales manuales fuera de migraciones.
* **Modo Cero Destrucción:** Las nuevas columnas sobre tablas existentes deben ser aditivas y definir valores por defecto seguros (ej. `FechaNacimiento` `DateOnly` con default `2000-01-01` en `UsuarioConfiguration.cs`) sin romper claves foráneas ni historiales (`Sesiones`, `Vehiculos`, `HistorialEstadosVehiculo`, `HistorialUbicacionVehiculo`, `HistorialKilometrajeVehiculo`).

---

## 3. Seguridad, Roles, Permisos y Contrato de Errores HTTP
* **Autenticación y Hash:** Tokens **JWT** con vigencia de 8 horas y hash de contraseñas con **BCrypt**. Bloqueo temporal de cuenta por 15 minutos tras 5 intentos fallidos consecutivos.
* **Formato Estándar de Error HTTP (`ExceptionHandlingMiddleware`):** Devuelve siempre `{ "mensaje": "..." }`:
  * `ValidationAppException` -> `400 Bad Request`
  * `AuthenticationFailedException` -> `401 Unauthorized`
  * `AccountDisabledException` / `ForbiddenAppException` -> `403 Forbidden` (`{ "mensaje": "No tienes permiso para realizar esta acción" }`)
  * `NotFoundException` -> `404 Not Found`
  * `AccountLockedException` -> `423 Locked`
* **Roles Fijos del Sistema (`DbInitializer.cs`):**
  1. `Id = 1`: `"Administrador"` — Acceso total irrestricto y administración de usuarios/permisos (protegido contra auto-desactivación o cambio de propio rol).
  2. `Id = 2`: `"Gerente de Ventas"` — Gestión y supervisión de ventas, liberación de reservas y corrección de kilometraje a la baja con motivo.
  3. `Id = 3`: `"Encargado de Lote y Recepción"` — Alta de unidades, captura de fotografías, reubicación de cajón y actualización de estado/odómetro.
  4. `Id = 4`: `"Asesor de Ventas"` — Consulta de catálogo, ficha técnica, pruebas de manejo y apartado de unidades.
* **Usuarios Semilla (`DbInitializer.cs`):**
  * Administrador: `EMP-00001` | `admin@autostock.com` | `AdminAutoStock2026!` (`RolId = 1`, `Activo = true`)
  * Asesor de Ventas: `EMP-00421` | `asesor@autostock.com` | `AsesorAutoStock2026!` (`RolId = 4`, `Activo = true`)
  * Usuario Inactivo: `EMP-00999` | `inactivo@autostock.com` | `Inactivo2026!` (`RolId = 4`, `Activo = false`)

---

## 4. Reglas de Negocio y Endpoints Principales (HU-01 a HU-16)
* **Autenticación y Recuperación (`AuthController`):**
  * `POST /api/auth/login`, `GET /api/auth/perfil`, `POST /api/auth/cambiar-password`.
  * Recuperación de contraseña (`HU-15`): token de un solo uso con vigencia de 30 minutos, límite de 3 solicitudes por hora por correo, respuesta anti-enumeración y desbloqueo automático de cuenta al restablecer.
* **Usuarios y Permisos (`UsuariosController`, `PermisosController`):**
  * `UsuariosController` protegido con `[Authorize(Roles = "Administrador")]`: alta con password temporal de 12 caracteres (`RequiereCambioPassword = true`), generación determinista de `EmpleadoId` (`[INICIALES]-[AAMMDD]-[01]` en `GeneradorIdentificadorUsuario.cs`), validación de mayoría de edad (`>= 18` años), edición (`PUT /api/usuarios/{id}`), cambio de estatus activo/inactivo (nunca borrado físico) y restablecimiento de contraseña temporal.
  * Matriz de permisos por rol (`HU-11` y `HU-12`): validación dinámica en servidor mediante `IPermisosService.ValidarPermisoAsync`.
* **Vehículos y Métricas (`VehiculosController`, `MetricasController`):**
  * Consulta paginada (`GET /api/vehiculos` y `GET /api/vehiculos/catalogo`), búsqueda sin guiones/espacios (`HU-04`), filtros y conteos rápidos (`GET /api/vehiculos/conteos-rapidos`, `GET /api/metricas/resumen`).
  * Alta (`POST /api/vehiculos`, `HU-08`): validación de VIN (17 caracteres sin `I`, `O`, `Q`), correspondencia Marca-Modelo (`CatalogoMarcasModelos.cs`), reglas `Nuevo` (`Kilometraje = 0`, `Placa = null`) vs `Seminuevo` (`Placa` obligatoria única) y `NumeroStock` consecutivo.
  * Evidencia fotográfica (`POST /api/vehiculos/{id}/fotografias` y `DELETE .../{imagenId}`, `HU-09`): 4 tomas obligatorias + 2 adicionales (máx. 6 fotos), cálculo de `PorcentajeEvidencia` e `InspeccionCompleta`.
  * Estado operativo (`PATCH /api/vehiculos/{id}/estado`, `HU-10`): bitácora inmutable `HistorialEstadoVehiculo`, bloqueo de transición directa a `Vendido` y reglas de reserva/TestDrive.
  * Reubicación de cajón (`PATCH /api/vehiculos/{id}/ubicacion`, `HU-13`): validación de colisión de cajón ocupado por unidad activa y bitácora `HistorialUbicacionVehiculo`.
  * Kilometraje y notas (`PATCH /api/vehiculos/{id}/kilometraje-notas`, `HU-14`): bloqueo de reducción para Encargado, corrección a la baja con motivo obligatorio (`>= 5` caracteres) para Gerente/Admin, prohibición en unidades `Vendido` y bitácora `HistorialKilometrajeVehiculo`.

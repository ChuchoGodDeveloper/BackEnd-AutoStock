# AutoStock — Backend Web API (.NET 10)

API RESTful desarrollada con **Clean Architecture** (Arquitectura Limpia) en 5 proyectos independientes para el sistema de inventario de concesionarias **AutoStock**.

---

## 1. Arquitectura del Proyecto

El backend está estructurado siguiendo los principios de Clean Architecture:

* **`AutoStock.Domain`**: Entidades del dominio (`Vehiculo`, `Usuario`, `Rol`, `Sesion`, `ImagenVehiculo`, `HistorialEstadoVehiculo`), enumeraciones (`EstadoOperativo`, `TipoVehiculo`) e interfaces de repositorios. Sin dependencias externas.
* **`AutoStock.Application`**: Casos de uso, interfaces de servicios (`IAuthService`, `IVehiculosService`, `IUsuariosService`), DTOs y excepciones de negocio (`ValidationAppException`, `NotFoundException`, etc.).
* **`AutoStock.Infrastructure`**: Implementación de repositorios con Entity Framework Core, DbContext (`AutoStockDbContext`), configuraciones Fluent API, migraciones versionadas, autenticación JWT (`JwtTokenGenerator`) y hashing con BCrypt (`PasswordHasher`).
* **`AutoStock.API`**: Controladores REST (`AuthController`, `VehiculosController`, `UsuariosController`, `MetricasController`), middlewares (`ExceptionHandlingMiddleware`), configuración de CORS y Swagger/OpenAPI.
* **`AutoStock.UnitTests`**: Suite de 55 pruebas unitarias automatizadas con **xUnit** y **Moq** que validan reglas de negocio, autenticación, catálogos, registro de vehículos, evidencia fotográfica y cambio de estado operativo.

---

## 2. Herramientas de Calidad de Código y Pruebas

### 2.1 Análisis Estático (Roslynator + .editorconfig)
El análisis estático está integrado en el ciclo de compilación mediante `Roslynator.Analyzers` (versión 4.14.0) configurado a nivel global en `Directory.Build.props` y parametrizado con reglas de estilo y nomenclatura en `.editorconfig`.

Para ejecutar la compilación con análisis estático de calidad:
```bash
dotnet build AutoStock.sln --configuration Release
```

### 2.2 Pruebas Unitarias Automatizadas (xUnit)
Para ejecutar la suite completa de pruebas unitarias:
```bash
dotnet test AutoStock.sln --configuration Release --no-build --verbosity normal
```

### 2.3 Automatización en GitHub Actions
El flujo de integración continua está automatizado en `.github/workflows/ci-quality-tests.yml`, ejecutando restauración, compilación estricta y las 55 pruebas unitarias en cada push o pull request hacia las ramas `Sprint1`, `develop` y `main`.

---

## 3. Versionamiento de Base de Datos (Entity Framework Core Migrations)

El proyecto utiliza **Entity Framework Core Migrations** conectado a **SQL Server / Azure SQL Database**. Las migraciones se encuentran versionadas en `src/AutoStock.Infrastructure/Persistence/Migrations/`.

### 3.1 Prerrequisito
Asegúrate de contar con la herramienta global de EF Core instalada:
```bash
dotnet tool install --global dotnet-ef
```

### 3.2 Listar Migraciones (Historial de Cambios)
Para verificar las migraciones existentes y su estado:
```bash
dotnet ef migrations list --project src/AutoStock.Infrastructure --startup-project src/AutoStock.API
```

### 3.3 Aplicar Migraciones a una Base de Datos Limpia
Para aplicar todas las migraciones pendientes y actualizar el esquema hasta la última versión:
```bash
dotnet ef database update --project src/AutoStock.Infrastructure --startup-project src/AutoStock.API
```

*(Opcional: Si se requiere apuntar a una base de datos específica, pasar `--connection "<CadenaDeConexion>"`).*

### 3.4 Reversión Controlada de Cambios (Rollback)
Para revertir de forma controlada a una migración anterior específica:
```bash
dotnet ef database update <NombreMigracionDestino> --project src/AutoStock.Infrastructure --startup-project src/AutoStock.API
```

Ejemplo de reversión hacia HU-09 (deshaciendo HU-10):
```bash
dotnet ef database update 20261022062801_UpdateImagenesVehiculo_AddEvidenciaFotografica_HU09 --project src/AutoStock.Infrastructure --startup-project src/AutoStock.API
```

### 3.5 Reaplicación de Migraciones
Para volver a aplicar los cambios tras un rollback:
```bash
dotnet ef database update --project src/AutoStock.Infrastructure --startup-project src/AutoStock.API
```

### 3.6 Generar Scripts SQL para Auditoría o Despliegue en Servidores
Para generar el script SQL entre dos migraciones específicas (por ejemplo, para generar el script de rollback):
```bash
# Script de Rollback (de HU-10 a HU-09):
dotnet ef migrations script 20261022181300_UpdateVehiculos_AddHistorialYCambioEstado_HU10 20261022062801_UpdateImagenesVehiculo_AddEvidenciaFotografica_HU09 --project src/AutoStock.Infrastructure --startup-project src/AutoStock.API

# Script de Reaplicación (de HU-09 a HU-10):
dotnet ef migrations script 20261022062801_UpdateImagenesVehiculo_AddEvidenciaFotografica_HU09 20261022181300_UpdateVehiculos_AddHistorialYCambioEstado_HU10 --project src/AutoStock.Infrastructure --startup-project src/AutoStock.API
```

---

## 4. Configuración de Entornos y Despliegue en Azure

* **Variables de Entorno Clave:**
  * `ConnectionStrings:DefaultConnection`: Cadena de conexión hacia SQL Server o Azure SQL.
  * `JwtSettings:SecretKey`: Llave simétrica de al menos 32 caracteres para firma de tokens JWT.
  * `JwtSettings:Issuer`: `AutoStock`.
  * `JwtSettings:Audience`: `AutoStock`.
* **Despliegue Continuo (CI/CD):** `.github/workflows/main_autostock-backend.yml` compila, valida y publica `src/AutoStock.API/AutoStock.API.csproj` hacia **Azure App Service** (`https://autostock-backend-cyfwg9f4fbcgewfq.centralus-01.azurewebsites.net`).

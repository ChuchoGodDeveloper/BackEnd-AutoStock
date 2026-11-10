# AutoStock — Backend API (.NET 10)

Web API RESTful para el sistema de inventario automotriz **AutoStock**, desarrollada con **.NET 10**, **Entity Framework Core** y **SQL Server**.

## Arquitectura del Backend (Clean Architecture)
- **`AutoStock.Domain`:** Entidades de dominio, enums y catálogos base.
- **`AutoStock.Application`:** Casos de uso, servicios de negocio, DTOs, interfaces y excepciones.
- **`AutoStock.Infrastructure`:** Persistencia EF Core (`AutoStockDbContext`), migraciones, repositorios, JWT y BCrypt.
- **`AutoStock.API` / `AutoStock.UnitTests`:** Controladores REST, middlewares y suite de pruebas unitarias (**xUnit** + **Moq**).
- **Reglas detalladas:** `.agents/rules/backend.md` (Arquitectura/API/BD) y `.agents/rules/git-workflow.md` (Git/CI-CD).

## Comandos Básicos
```powershell
dotnet restore AutoStock.sln                                # Restaurar paquetes NuGet
dotnet build AutoStock.sln --configuration Release          # Compilar solución + Roslynator
dotnet test AutoStock.sln --configuration Release --no-build # Ejecutar pruebas unitarias (xUnit)
dotnet run --project src/AutoStock.API/AutoStock.API.csproj # Iniciar API localmente
```

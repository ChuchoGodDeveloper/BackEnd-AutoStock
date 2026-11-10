---
name: nuevo-endpoint
description: "Genera la estructura completa de un nuevo módulo o endpoint en ASP.NET Core (DTOs, Interfaz, Servicio y Controlador)."
---

# Instrucciones para el agente al ejecutar /nuevo-endpoint
Cuando el usuario invoque esta skill solicitando un nuevo módulo o endpoint, ejecuta estos pasos en orden sin escanear todo el proyecto:

1. **DTOs (`src/AutoStock.Application/DTOs`):** Crea las clases DTO de Request y Response necesarias en `src/AutoStock.Application/DTOs` con sus validaciones (`DataAnnotations`).
2. **Interfaz y Servicio (`src/AutoStock.Application/Interfaces` y `src/AutoStock.Application/Services`):** Crea o actualiza la interfaz en `src/AutoStock.Application/Interfaces` y su implementación en `src/AutoStock.Application/Services` usando inyección de dependencias, métodos asíncronos (`async/await`) y `.AsNoTracking()` en todas las consultas de lectura hacia PostgreSQL.
3. **Controlador (`src/AutoStock.API/Controllers`):** Crea el controlador `ApiController` en `src/AutoStock.API/Controllers` inyectando el servicio, aplicando autorización por roles (`[Authorize]`) y devolviendo códigos de estado HTTP estándar (`ActionResult`).
4. **Registro y Compilación (`src/AutoStock.API/Program.cs`):** Si es un servicio nuevo, regístralo en `src/AutoStock.API/Program.cs` y ejecuta `dotnet build` en la terminal para confirmar que hay 0 errores de compilación.

using AutoStock.Application.DTOs;
using AutoStock.Application.Exceptions;
using AutoStock.Application.Interfaces;
using AutoStock.Domain.Constants;
using AutoStock.Domain.Entities;
using AutoStock.Domain.Interfaces;

namespace AutoStock.Application.Services;

public class PermisosService : IPermisosService
{
    private readonly IPermisoRepository _permisoRepository;
    private readonly IRolRepository _rolRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PermisosService(
        IPermisoRepository permisoRepository,
        IRolRepository rolRepository,
        IUnitOfWork unitOfWork)
    {
        _permisoRepository = permisoRepository;
        _rolRepository = rolRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MatrizPermisosResponseDto> GetMatrizPermisosAsync()
    {
        var permisos = await _permisoRepository.GetAllPermisosAsync();
        var roles = await _rolRepository.GetAllAsync();
        var rolPermisos = await _permisoRepository.GetAllRolPermisosAsync();

        var rolPermisosMap = rolPermisos
            .GroupBy(rp => rp.RolId)
            .ToDictionary(g => g.Key, g => g.ToDictionary(rp => rp.Permiso.Codigo, rp => rp.Activo));

        var rolesDto = new List<RolPermisosDto>();

        foreach (var rol in roles)
        {
            var permisosRol = new Dictionary<string, bool>();

            foreach (var permiso in permisos)
            {
                if (rol.Nombre == "Administrador")
                {
                    permisosRol[permiso.Codigo] = true;
                }
                else if (rolPermisosMap.TryGetValue(rol.Id, out var dict) && dict.TryGetValue(permiso.Codigo, out var activo))
                {
                    permisosRol[permiso.Codigo] = activo;
                }
                else
                {
                    permisosRol[permiso.Codigo] = MatrizPermisosBase.EsPermisoActivoBase(rol.Nombre, permiso.Codigo);
                }
            }

            rolesDto.Add(new RolPermisosDto
            {
                RolId = rol.Id,
                NombreRol = rol.Nombre,
                Permisos = permisosRol,
            });
        }

        var ultimaModificacion = rolPermisos.Count > 0
            ? rolPermisos.Max(rp => rp.FechaModificacion)
            : (DateTime?)null;

        var modificadoPorNombre = rolPermisos
            .Where(rp => !string.IsNullOrEmpty(rp.ModificadoPorNombre))
            .OrderByDescending(rp => rp.FechaModificacion)
            .Select(rp => rp.ModificadoPorNombre)
            .FirstOrDefault();

        return new MatrizPermisosResponseDto
        {
            Acciones = permisos.Select(p => new PermisoDto
            {
                Id = p.Id,
                Codigo = p.Codigo,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Modulo = p.Modulo,
                EsFijo = p.EsFijo,
            }).ToList(),
            Roles = rolesDto,
            UltimaModificacion = ultimaModificacion,
            ModificadoPorNombre = modificadoPorNombre,
        };
    }

    public async Task<RespuestaOperacionDto> ActualizarMatrizPermisosAsync(
        ActualizarMatrizPermisosRequestDto dto,
        int usuarioId,
        string nombreUsuario)
    {
        if (dto is null || dto.Roles is null || dto.Roles.Count == 0)
        {
            throw new ValidationAppException("La configuración de permisos no puede estar vacía");
        }

        var roles = await _rolRepository.GetAllAsync();
        var rolesMap = roles.ToDictionary(r => r.Id);
        var permisos = await _permisoRepository.GetAllPermisosAsync();
        var permisosMap = permisos.ToDictionary(p => p.Codigo);
        var rolPermisosExistentes = await _permisoRepository.GetAllRolPermisosAsync();
        var rolPermisosMap = rolPermisosExistentes
            .ToDictionary(rp => (rp.RolId, rp.PermisoId));

        var ahora = DateTime.UtcNow;
        var nuevosRolPermisos = new List<RolPermiso>();

        foreach (var rolConfig in dto.Roles)
        {
            if (!rolesMap.TryGetValue(rolConfig.RolId, out var rol))
            {
                throw new NotFoundException($"El rol con ID {rolConfig.RolId} no existe");
            }

            foreach (var (codigoPermiso, activo) in rolConfig.Permisos)
            {
                if (!permisosMap.TryGetValue(codigoPermiso, out var permiso))
                {
                    throw new ValidationAppException($"El código de permiso '{codigoPermiso}' no es válido");
                }

                if (rol.Nombre == "Administrador" && !activo)
                {
                    throw new ValidationAppException("Los permisos del Administrador son inmutables y no se pueden desactivar");
                }

                if (codigoPermiso == CodigosPermiso.ConsultarCatalogo && !activo)
                {
                    throw new ValidationAppException("El permiso de consultar catálogo y ficha técnica es obligatorio para todos los roles y no se puede quitar");
                }

                var valorActivo = rol.Nombre == "Administrador" || activo;

                if (rolPermisosMap.TryGetValue((rol.Id, permiso.Id), out var rolPermiso))
                {
                    rolPermiso.Activo = valorActivo;
                    rolPermiso.FechaModificacion = ahora;
                    rolPermiso.ModificadoPorUsuarioId = usuarioId;
                    rolPermiso.ModificadoPorNombre = nombreUsuario;
                }
                else
                {
                    nuevosRolPermisos.Add(new RolPermiso
                    {
                        RolId = rol.Id,
                        PermisoId = permiso.Id,
                        Activo = valorActivo,
                        FechaModificacion = ahora,
                        ModificadoPorUsuarioId = usuarioId,
                        ModificadoPorNombre = nombreUsuario,
                    });
                }
            }
        }

        if (nuevosRolPermisos.Count > 0)
        {
            await _permisoRepository.AddRolPermisosAsync(nuevosRolPermisos);
        }

        await _unitOfWork.SaveChangesAsync();

        return new RespuestaOperacionDto
        {
            Mensaje = "Permisos actualizados",
        };
    }

    public async Task<RespuestaOperacionDto> RestablecerMatrizBaseAsync(
        int usuarioId,
        string nombreUsuario)
    {
        var roles = await _rolRepository.GetAllAsync();
        var permisos = await _permisoRepository.GetAllPermisosAsync();
        var rolPermisosExistentes = await _permisoRepository.GetAllRolPermisosAsync();
        var rolPermisosMap = rolPermisosExistentes
            .ToDictionary(rp => (rp.RolId, rp.PermisoId));

        var ahora = DateTime.UtcNow;
        var nuevosRolPermisos = new List<RolPermiso>();

        foreach (var rol in roles)
        {
            foreach (var permiso in permisos)
            {
                var activoBase = MatrizPermisosBase.EsPermisoActivoBase(rol.Nombre, permiso.Codigo);

                if (rolPermisosMap.TryGetValue((rol.Id, permiso.Id), out var rolPermiso))
                {
                    rolPermiso.Activo = activoBase;
                    rolPermiso.FechaModificacion = ahora;
                    rolPermiso.ModificadoPorUsuarioId = usuarioId;
                    rolPermiso.ModificadoPorNombre = nombreUsuario;
                }
                else
                {
                    nuevosRolPermisos.Add(new RolPermiso
                    {
                        RolId = rol.Id,
                        PermisoId = permiso.Id,
                        Activo = activoBase,
                        FechaModificacion = ahora,
                        ModificadoPorUsuarioId = usuarioId,
                        ModificadoPorNombre = nombreUsuario,
                    });
                }
            }
        }

        if (nuevosRolPermisos.Count > 0)
        {
            await _permisoRepository.AddRolPermisosAsync(nuevosRolPermisos);
        }

        await _unitOfWork.SaveChangesAsync();

        return new RespuestaOperacionDto
        {
            Mensaje = "Permisos restablecidos a los valores iniciales",
        };
    }
}

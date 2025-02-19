using EmployeePermissions.Application.DTOs;
using EmployeePermissions.Application.Interfaces;
using EmployeePermissions.Core.Entities;
using EmployeePermissions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EmployeePermissions.Infrastructure.Repositories;

public class PermissionRepository : IPermissionRepository
{
    private readonly ApplicationDbContext _context;

    public PermissionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Permission>> GetAllAsync()
    {
        return await _context.Permissions
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<IEnumerable<PermissionDto>> GetAllByEmployeeAsync()
    {
        return await _context.Employees
            .AsNoTracking()
            .Select(x => new PermissionDto
            {
                EmployeeId = x.Id,
                EmployeeName = x.Name,
                Permissions = x.Permissions.Select(p => new PermissionTypeDto
                {
                    Id = p.PermissionType.Id,
                    Name = p.PermissionType.Name
                }).ToList()
            })
            .ToListAsync();
    }

    public async Task<Permission?> GetByIdAsync(Guid employeeId, Guid permissionTypeId)
    {
        return await _context.Permissions
            .Include(x => x.Employee)
            .Include(x => x.PermissionType)
            .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.PermissionTypeId == permissionTypeId);
    }

    public async Task AddAsync(Permission entity)
    {
        await _context.Permissions.AddAsync(entity);
    }

    public void Update(Permission entity)
    {
        _context.Permissions.Update(entity);
    }

    public void Delete(Guid id)
    {
        var entityToDelete = new Employee { Id = id };
        _context.Employees.Attach(entityToDelete);
        _context.Employees.Remove(entityToDelete);
    }
}
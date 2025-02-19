using EmployeePermissions.Core.Entities;
using EmployeePermissions.Core.Interfaces;
using EmployeePermissions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EmployeePermissions.Infrastructure.Repositories;

public class PermissionTypeRepository : IPermissionTypeRepository
{
    private readonly ApplicationDbContext _context;

    public PermissionTypeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PermissionType?> GetByIdAsync(Guid id)
    {
        return await _context.PermissionTypes.FindAsync(id);
    }

    public async Task<IEnumerable<PermissionType>> GetAllAsync()
    {
        return await _context.PermissionTypes.ToListAsync();
    }

    public async Task AddAsync(PermissionType entity)
    {
        await _context.PermissionTypes.AddAsync(entity);
    }

    public void Update(PermissionType entity)
    {
        _context.PermissionTypes.Update(entity);
    }

    public void Delete(Guid id)
    {
        var entityToDelete = new Employee { Id = id };
        _context.Employees.Attach(entityToDelete);
        _context.Employees.Remove(entityToDelete);
    }
}
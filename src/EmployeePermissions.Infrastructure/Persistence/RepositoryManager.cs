using EmployeePermissions.Application.Interfaces;
using EmployeePermissions.Core.Interfaces;

namespace EmployeePermissions.Infrastructure.Persistence;

// Unit of Work implementation for managing repositories
public class RepositoryManager : IRepositoryManager
{
    private readonly ApplicationDbContext _context;

    public IEmployeeRepository Employees { get; }
    public IPermissionRepository Permissions { get; }
    public IPermissionTypeRepository PermissionTypes { get; }

    public RepositoryManager(
        ApplicationDbContext context,
        IEmployeeRepository employeeRepository,
        IPermissionRepository permissionRepository,
        IPermissionTypeRepository permissionTypeRepository)
    {
        _context = context;
        Employees = employeeRepository;
        Permissions = permissionRepository;
        PermissionTypes = permissionTypeRepository;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
using EmployeePermissions.Core.Entities;
using EmployeePermissions.Core.Interfaces;

namespace EmployeePermissions.Application.Interfaces;

// Unit of Work implementation for managing repositories
public interface IRepositoryManager
{
    IEmployeeRepository Employees { get; }
    IPermissionRepository Permissions { get; }
    IPermissionTypeRepository PermissionTypes { get; }
    Task<int> SaveChangesAsync();
}
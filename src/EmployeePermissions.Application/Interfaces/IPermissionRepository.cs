using EmployeePermissions.Application.DTOs;
using EmployeePermissions.Core.Entities;

namespace EmployeePermissions.Application.Interfaces;


public interface IPermissionRepository 
{
    Task<IEnumerable<Permission>> GetAllAsync();
    Task<IEnumerable<PermissionDto>> GetAllByEmployeeAsync();
    Task<Permission?> GetByIdAsync(Guid employeeId, Guid permissionTypeId);
    Task AddAsync(Permission entity);
    void Update(Permission entity);
    void Delete(Guid id);
}
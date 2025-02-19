using EmployeePermissions.Application.DTOs;
using EmployeePermissions.Core.Entities;

namespace EmployeePermissions.Application.Interfaces;

public interface IElasticsearchService
{
    Task IndexPermissionAsync(Permission permission);
    Task UpdatePermissionAsync(Permission permission);
    Task<IEnumerable<ElasticsearchPermission>> GetPermissionsAsync();
}
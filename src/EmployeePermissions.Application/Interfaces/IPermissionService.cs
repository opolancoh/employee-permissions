using EmployeePermissions.Application.Commands;
using EmployeePermissions.Application.DTOs;
using EmployeePermissions.Application.Queries;
using EmployeePermissions.Core.Entities;

namespace EmployeePermissions.Application.Interfaces;

public interface IPermissionService
{
    Task RequestPermissionAsync(RequestPermissionCommand command);
    Task ModifyPermissionAsync(RequestPermissionCommand command);
    Task<IEnumerable<PermissionDto>> GetPermissionsAsync(GetPermissionsQuery query);
}
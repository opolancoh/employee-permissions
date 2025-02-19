namespace EmployeePermissions.Application.DTOs;

public class PermissionDto
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; }
    public List<PermissionTypeDto> Permissions { get; set; }
}

public class PermissionTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}
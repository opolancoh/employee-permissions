namespace EmployeePermissions.Application.DTOs;

public class ElasticsearchPermission
{
    public Guid EmployeeId { get; set; }
    public Guid PermissionTypeId { get; set; }
    public DateTime GrantedDate { get; set; }
    public string Description { get; set; } = string.Empty;
}
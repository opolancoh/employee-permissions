namespace EmployeePermissions.Application.Commands;

public class RequestPermissionCommand
{
    public Guid EmployeeId { get; set; }
    public Guid PermissionTypeId { get; set; }
    public DateTime GrantedDate { get; set; }
    public string? Description { get; set; }
}
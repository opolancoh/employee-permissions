namespace EmployeePermissions.Core.Entities;

public class Permission
{
    // Composite key
    public Guid EmployeeId { get; set; }
    public Guid PermissionTypeId { get; set; }
    
    // Additional data on the relationship
    public DateTime GrantedDate { get; set; }
    public string? Description { get; set; }

    // Navigation properties
    public Employee Employee { get; set; }
    public PermissionType PermissionType { get; set; }
}
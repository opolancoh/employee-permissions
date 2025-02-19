namespace EmployeePermissions.Core.Entities;

public class PermissionType
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    
    // Navigation property for reverse navigation (optional but useful)
    public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
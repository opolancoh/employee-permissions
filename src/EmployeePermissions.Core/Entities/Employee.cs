namespace EmployeePermissions.Core.Entities;

public class Employee
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    
    // Navigation property to the join entity
    public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
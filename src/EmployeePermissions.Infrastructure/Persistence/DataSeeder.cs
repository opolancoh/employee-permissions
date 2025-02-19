using EmployeePermissions.Core.Entities;

namespace EmployeePermissions.Infrastructure.Persistence;

public static class DataSeeder
{
    public static void SeedInitialData(ApplicationDbContext context)
    {
        if (!context.Employees.Any())
        {
            context.Employees.AddRange(
                new Employee
                {
                    Id = Guid.Parse("f47ac10b-58cc-4372-a567-0e02b2c3d479"),
                    Name = "Alice"
                },
                new Employee
                {
                    Id = Guid.Parse("c9bf9e57-1685-4c89-bafb-ff5af830be8a"),
                    Name = "Bob"
                },
                new Employee
                {
                    Id = Guid.Parse("a3e8f7b6-4d5c-4f3e-8b2a-9e1f3c4d5e6f"),
                    Name = "John Doe"
                }
            );
        }

        if (!context.PermissionTypes.Any())
        {
            context.PermissionTypes.AddRange(
                new PermissionType
                {
                    Id = Guid.Parse("1b9d6bcd-bbfd-4b2d-9b5d-ab8dfbbd4bed"),
                    Name = "Vacation",
                    Description = "Paid time off"
                },
                new PermissionType
                {
                    Id = Guid.Parse("6ba7b810-9dad-11d1-80b4-00c04fd430c8"),
                    Name = "Medical",
                    Description = "Medical leave"
                }
            );
        }

        context.SaveChanges();

        if (!context.Permissions.Any())
        {
            var alice = context.Employees.FirstOrDefault(e => e.Name == "Alice");
            var bob = context.Employees.FirstOrDefault(e => e.Name == "Bob");
            
            var vacationType = context.PermissionTypes.FirstOrDefault(pt => pt.Name == "Vacation");
            var medicalType = context.PermissionTypes.FirstOrDefault(pt => pt.Name == "Medical");
            
            context.Permissions.Add(new Permission
            {
                EmployeeId = alice!.Id,
                PermissionTypeId = vacationType!.Id,
                GrantedDate = DateTime.UtcNow,
                Description = "Initial seeded permission for Alice and PTO.",
            });
            
            context.Permissions.Add(new Permission
            {
                EmployeeId = alice!.Id,
                PermissionTypeId = medicalType!.Id,
                GrantedDate = DateTime.UtcNow,
                Description = "Initial seeded permission for Alice and medical leave.",
            });
            
            context.Permissions.Add(new Permission
            {
                EmployeeId = bob!.Id,
                PermissionTypeId = vacationType!.Id,
                GrantedDate = DateTime.UtcNow,
                Description = "Initial seeded permission for Bob and PTO.",
            });
        }

        context.SaveChanges();
    }
}
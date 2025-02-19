using EmployeePermissions.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeePermissions.Infrastructure.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .HasMaxLength(100)
            .IsRequired();

        // Configures a one-to-many relationship between Employee and Permissions.
        // Each Employee can have many Permissions, and each Permission is associated with one Employee.
        // The foreign key in the Permissions table is EmployeeId.
        // When an Employee is deleted, all associated Permissions are also deleted due to the cascade delete behavior (DeleteBehavior.Cascade).
        builder.HasMany(x => x.Permissions)
            .WithOne(x => x.Employee)
            .HasForeignKey(x => x.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
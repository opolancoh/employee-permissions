using EmployeePermissions.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EmployeePermissions.Infrastructure.Persistence.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");

        // Composite primary key to ensure an employee can have a specific permission type only once
        builder.HasKey(p => new { p.EmployeeId, p.PermissionTypeId });

        builder.Property(p => p.GrantedDate)
            .IsRequired();

        // Configure the relationship with Employee:
        // - Each Permission must be associated with one Employee
        // - An Employee can have many Permissions
        // - The foreign key is EmployeeId.
        builder.HasOne(p => p.Employee)
            .WithMany(e => e.Permissions)
            .HasForeignKey(p => p.EmployeeId);

        // Configure the relationship with PermissionType:
        // - Each Permission must be associated with one PermissionType
        // - A PermissionType can be associated with many Permissions
        // - The foreign key is PermissionTypeId
        builder.HasOne(p => p.PermissionType)
            .WithMany(pt => pt.Permissions)
            .HasForeignKey(p => p.PermissionTypeId);
    }
}
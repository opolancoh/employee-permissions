using System.Net;
using System.Net.Http.Json;
using EmployeePermissions.IntegrationTests.Common;

namespace EmployeePermissions.IntegrationTests;

public class ModifyPermissionTests : BaseIntegrationTest
{
    private const string RequestUri = "/api/permissions";

    [Fact]
    public async Task ModifyPermission_AndModifyNewPermission_ShouldReturnNoContent()
    {
        // Arrange
        var employee = await CreateEmployeeAsync("John");
        var permissionType = await CreatePermissionTypeAsync("Vacation", "Paid time off");
        
        var permission = await CreatePermissionAsync(employee.Id, permissionType.Id, DateTime.UtcNow, "This is a description");
        await CreateAndIndexElasticsearchPermissionAsync(permission);

        var requestPayload = new
        {
            EmployeeId = employee.Id,
            PermissionTypeId = permissionType.Id,
            GrantedDate = DateTime.UtcNow,
            Description = "This is a description updated"
        };

        // Act
        var response = await Client.PutAsJsonAsync(RequestUri, requestPayload);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task ModifyPermission_WhenEmployeeIdNoValid_ShouldReturnInternalServerError()
    {
        // Arrange
        var permissionType = await CreatePermissionTypeAsync("Vacation", "Paid time off");

        var requestPayload = new
        {
            EmployeeId = Guid.NewGuid(),
            PermissionTypeId = permissionType.Id,
            GrantedDate = DateTime.UtcNow,
            Description = "Integration test permission updated"
        };

        // Act
        var response = await Client.PostAsJsonAsync(RequestUri, requestPayload);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task ModifyPermission_WhenPermissionTypeIdNoValid_ShouldReturnInternalServerError()
    {
        // Arrange
        var employee = await CreateEmployeeAsync("John");

        var requestPayload = new
        {
            EmployeeId = employee.Id,
            PermissionTypeId = Guid.NewGuid(),
            GrantedDate = DateTime.UtcNow,
            Description = "Integration test permission"
        };

        // Act
        var response = await Client.PutAsJsonAsync(RequestUri, requestPayload);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }
    
    [Fact]
    public async Task ModifyPermission_AndNoPermission_ShouldReturnInternalServerError()
    {
        // Arrange
        var employee = await CreateEmployeeAsync("John");
        var permissionType = await CreatePermissionTypeAsync("Vacation", "Paid time off");

        var requestPayload = new
        {
            EmployeeId = employee.Id,
            PermissionTypeId = permissionType.Id,
            GrantedDate = DateTime.UtcNow,
            Description = "This is a description updated"
        };

        // Act
        var response = await Client.PutAsJsonAsync(RequestUri, requestPayload);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }
}
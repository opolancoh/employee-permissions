using System.Net;
using System.Net.Http.Json;
using EmployeePermissions.IntegrationTests.Common;

namespace EmployeePermissions.IntegrationTests;

public class RequestPermissionTests : BaseIntegrationTest
{
    private const string RequestUri = "/api/permissions";

    [Fact]
    public async Task RequestPermission_AndCreateNewPermission_ShouldReturnOk()
    {
        // Arrange
        var employee = await CreateEmployeeAsync("John");
        var permissionType = await CreatePermissionTypeAsync("Vacation", "Paid time off");

        var requestPayload = new
        {
            EmployeeId = employee.Id,
            PermissionTypeId = permissionType.Id,
            GrantedDate = DateTime.UtcNow,
            Description = "Integration test permission"
        };

        // Act
        var response = await Client.PostAsJsonAsync(RequestUri, requestPayload);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task RequestPermission_WhenEmployeeIdNoValid_ShouldReturnInternalServerError()
    {
        // Arrange
        var permissionType = await CreatePermissionTypeAsync("Vacation", "Paid time off");

        var requestPayload = new
        {
            EmployeeId = Guid.NewGuid(),
            PermissionTypeId = permissionType.Id,
            GrantedDate = DateTime.UtcNow,
            Description = "Integration test permission"
        };

        // Act
        var response = await Client.PostAsJsonAsync(RequestUri, requestPayload);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task RequestPermission_WhenPermissionTypeIdNoValid_ShouldReturnInternalServerError()
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
        var response = await Client.PostAsJsonAsync(RequestUri, requestPayload);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }
}
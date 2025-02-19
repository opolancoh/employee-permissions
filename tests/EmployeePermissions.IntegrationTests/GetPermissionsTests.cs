using System.Net;
using System.Net.Http.Json;
using EmployeePermissions.Application.DTOs;
using EmployeePermissions.IntegrationTests.Common;

namespace EmployeePermissions.IntegrationTests;

public class GetPermissionsTests : BaseIntegrationTest
{
    private const string RequestUri = "/api/permissions";

    [Fact]
    public async Task GetPermissions_ShouldReturnOk_AndReturnList()
    {
        // Arrange
        var employee = await CreateEmployeeAsync("John");
        var permissionType = await CreatePermissionTypeAsync("Vacation", "Paid time off");
        await CreatePermissionAsync(employee.Id, permissionType.Id, DateTime.UtcNow, "This is a description");

        // Act
        var response = await Client.GetAsync(RequestUri);
        var responseContent = await response.Content.ReadFromJsonAsync<IEnumerable<PermissionDto>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseContent);

        var employeePermissions = responseContent.ToList();
        Assert.NotEmpty(employeePermissions);
        Assert.Single(employeePermissions);

        var employeePermissionItem = employeePermissions.SingleOrDefault(p => p.EmployeeId == employee.Id);
        Assert.NotNull(employeePermissionItem);
        Assert.Equal(employee.Id, employeePermissionItem.EmployeeId);
        Assert.Equal(employee.Name, employeePermissionItem.EmployeeName);

        var permissions = employeePermissionItem.Permissions;
        Assert.NotEmpty(permissions);
        Assert.Single(permissions);

        var permissionItem = permissions.SingleOrDefault(x => x.Id == permissionType.Id);
        Assert.NotNull(permissionItem);
        Assert.Equal(permissionType.Id, permissionItem.Id);
        Assert.Equal(permissionType.Name, permissionItem.Name);
    }

    [Fact]
    public async Task GetPermissions_WhenNoPermissionsExist_ShouldReturnOkAndEmptyList()
    {
        // Arrange
        // No data to add.

        // Act
        var response = await Client.GetAsync(RequestUri);
        var responseContent = await response.Content.ReadFromJsonAsync<IEnumerable<PermissionDto>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseContent);
        Assert.Empty(responseContent);
    }

    [Fact]
    public async Task GetPermissions_WhenMultiplePermissionsExist_ShouldReturnOkAndAllRecords()
    {
        // Arrange
        var employee1 = await CreateEmployeeAsync("Alice");
        var employee2 = await CreateEmployeeAsync("Bob");

        var permissionTypeVacation = await CreatePermissionTypeAsync("Vacation", "Paid time off");
        var permissionTypeMedical = await CreatePermissionTypeAsync("Medical", "Medical leave");

        await CreatePermissionAsync(employee1.Id, permissionTypeVacation.Id, DateTime.UtcNow, "Alice - Vacation");
        await CreatePermissionAsync(employee2.Id, permissionTypeVacation.Id, DateTime.UtcNow, "Bob - Vacation 1");
        await CreatePermissionAsync(employee2.Id, permissionTypeMedical.Id, DateTime.UtcNow, "Bob - Medical 1");

        // Act
        var response = await Client.GetAsync(RequestUri);
        var responseContent = await response.Content.ReadFromJsonAsync<IEnumerable<PermissionDto>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseContent);

        var permissionList = responseContent.ToList();
        Assert.Equal(2, permissionList.Count);

        var aliceEntry = permissionList.SingleOrDefault(x => x.EmployeeId == employee1.Id);
        Assert.NotNull(aliceEntry);
        Assert.Equal("Alice", aliceEntry.EmployeeName);
        Assert.Single(aliceEntry.Permissions);

        var bobEntry = permissionList.SingleOrDefault(x => x.EmployeeId == employee2.Id);
        Assert.NotNull(bobEntry);
        Assert.Equal("Bob", bobEntry.EmployeeName);
        Assert.Equal(2, bobEntry.Permissions.Count);
    }

    [Fact]
    public async Task GetPermissions_WhenEmployeeHasMultiplePermissionTypes_ShouldReturnAllTypesInSameEntry()
    {
        // Arrange
        var employee = await CreateEmployeeAsync("John");

        var vacationType = await CreatePermissionTypeAsync("Vacation", "Paid time off");
        var medicalType = await CreatePermissionTypeAsync("Medical", "Medical leave");

        await CreatePermissionAsync(employee.Id, vacationType.Id, DateTime.UtcNow, "John - Vacation 1");
        await CreatePermissionAsync(employee.Id, medicalType.Id, DateTime.UtcNow, "John - Medical 1");

        // Act
        var response = await Client.GetAsync(RequestUri);
        var responseContent = await response.Content.ReadFromJsonAsync<IEnumerable<PermissionDto>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(responseContent);

        var permissionList = responseContent.ToList();
        Assert.Single(permissionList);

        var johnEntry = permissionList.Single();
        Assert.Equal(employee.Id, johnEntry.EmployeeId);
        Assert.Equal("John", johnEntry.EmployeeName);
        Assert.Equal(2, johnEntry.Permissions.Count);
        Assert.Contains(johnEntry.Permissions, pt => pt.Id == vacationType.Id && pt.Name == vacationType.Name);
        Assert.Contains(johnEntry.Permissions, pt => pt.Id == medicalType.Id && pt.Name == medicalType.Name);
    }
}
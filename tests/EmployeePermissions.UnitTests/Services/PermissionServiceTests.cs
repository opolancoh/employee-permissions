using EmployeePermissions.Application.Commands;
using EmployeePermissions.Application.DTOs;
using EmployeePermissions.Application.Interfaces;
using EmployeePermissions.Application.Queries;
using EmployeePermissions.Application.Services;
using EmployeePermissions.Core.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace EmployeePermissions.UnitTests.Services;

public class PermissionServiceTests
{
    private readonly Mock<ILogger<PermissionService>> _loggerMock;
    private readonly Mock<IRepositoryManager> _repositoryManagerMock;
    private readonly Mock<IKafkaService> _kafkaServiceMock;
    private readonly Mock<IElasticsearchService> _elasticsearchServiceMock;

    private readonly PermissionService _permissionService;

    public PermissionServiceTests()
    {
        _loggerMock = new Mock<ILogger<PermissionService>>();
        _repositoryManagerMock = new Mock<IRepositoryManager>();
        _kafkaServiceMock = new Mock<IKafkaService>();
        _elasticsearchServiceMock = new Mock<IElasticsearchService>();

        // Create the actual service, injecting the mocks
        _permissionService = new PermissionService(
            _loggerMock.Object,
            _repositoryManagerMock.Object,
            _kafkaServiceMock.Object,
            _elasticsearchServiceMock.Object
        );
    }

    #region RequestPermissionAsync

    [Fact]
    public async Task RequestPermissionAsync_ValidCommand_ShouldSucceed()
    {
        // Arrange
        var command = new RequestPermissionCommand
        {
            EmployeeId = Guid.NewGuid(),
            PermissionTypeId = Guid.NewGuid(),
            GrantedDate = DateTime.UtcNow,
            Description = "Test Permission"
        };

        // Mock repository add/save
        _repositoryManagerMock
            .Setup(r => r.Permissions.AddAsync(It.IsAny<Permission>()))
            .Returns(Task.CompletedTask);

        _repositoryManagerMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Mock Kafka publish
        _kafkaServiceMock
            .Setup(k => k.PublishOperationAsync(It.IsAny<Guid>(), "request"))
            .Returns(Task.CompletedTask);

        // Mock Elasticsearch index
        _elasticsearchServiceMock
            .Setup(es => es.IndexPermissionAsync(It.IsAny<Permission>()))
            .Returns(Task.CompletedTask);

        // Act
        await _permissionService.RequestPermissionAsync(command);

        // Assert
        // Verify repository usage
        _repositoryManagerMock
            .Verify(r => r.Permissions.AddAsync(It.Is<Permission>(p =>
                p.EmployeeId == command.EmployeeId &&
                p.PermissionTypeId == command.PermissionTypeId &&
                p.Description == command.Description)), Times.Once);

        _repositoryManagerMock
            .Verify(r => r.SaveChangesAsync(), Times.Once);

        // Verify Kafka publish with "request"
        _kafkaServiceMock
            .Verify(k => k.PublishOperationAsync(It.IsAny<Guid>(), "request"), Times.Once);

        // Verify ES indexing
        _elasticsearchServiceMock
            .Verify(es => es.IndexPermissionAsync(It.IsAny<Permission>()), Times.Once);

        // Verify logging
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) => state.ToString()!.Contains("Permission successfully added")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            Times.Once
        );
    }

    #endregion

    #region ModifyPermissionAsync

    [Fact]
    public async Task ModifyPermissionAsync_ExistingPermission_ShouldSucceed()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var permissionTypeId = Guid.NewGuid();

        var command = new RequestPermissionCommand
        {
            EmployeeId = employeeId,
            PermissionTypeId = permissionTypeId,
            GrantedDate = DateTime.UtcNow.AddDays(1),
            Description = "Modified permission"
        };

        var existingPermission = new Permission
        {
            EmployeeId = employeeId,
            PermissionTypeId = permissionTypeId,
            GrantedDate = DateTime.UtcNow,
            Description = "Old description"
        };

        _repositoryManagerMock
            .Setup(r => r.Permissions.GetByIdAsync(employeeId, permissionTypeId))
            .ReturnsAsync(existingPermission);

        _repositoryManagerMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        _kafkaServiceMock
            .Setup(k => k.PublishOperationAsync(It.IsAny<Guid>(), "modify"))
            .Returns(Task.CompletedTask);

        _elasticsearchServiceMock
            .Setup(es => es.UpdatePermissionAsync(It.IsAny<Permission>()))
            .Returns(Task.CompletedTask);

        // Act
        await _permissionService.ModifyPermissionAsync(command);

        // Assert
        // Verify the repository updated the permission
        _repositoryManagerMock
            .Verify(r => r.Permissions.Update(It.Is<Permission>(p =>
                    p.EmployeeId == command.EmployeeId &&
                    p.PermissionTypeId == command.PermissionTypeId &&
                    p.Description == command.Description &&
                    p.GrantedDate == command.GrantedDate)),
                Times.Once);

        _repositoryManagerMock
            .Verify(r => r.SaveChangesAsync(), Times.Once);

        // Verify Kafka publish with "modify"
        _kafkaServiceMock
            .Verify(k => k.PublishOperationAsync(It.IsAny<Guid>(), "modify"), Times.Once);

        // Verify ES update
        _elasticsearchServiceMock
            .Verify(es => es.UpdatePermissionAsync(It.IsAny<Permission>()), Times.Once);

        // Verify logging
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, t) => state.ToString()!.Contains("Permission successfully updated")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            Times.Once
        );
    }

    [Fact]
    public async Task ModifyPermissionAsync_NonExistingPermission_ShouldThrowException()
    {
        // Arrange
        var command = new RequestPermissionCommand
        {
            EmployeeId = Guid.NewGuid(),
            PermissionTypeId = Guid.NewGuid()
        };

        _repositoryManagerMock
            .Setup(r => r.Permissions.GetByIdAsync(command.EmployeeId, command.PermissionTypeId))
            .ReturnsAsync((Permission)null!);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _permissionService.ModifyPermissionAsync(command));

        // Verify we never updated or saved
        _repositoryManagerMock.Verify(r => r.Permissions.Update(It.IsAny<Permission>()), Times.Never);
        _repositoryManagerMock.Verify(r => r.SaveChangesAsync(), Times.Never);

        // Verify we never published or updated ES
        _kafkaServiceMock.Verify(k => k.PublishOperationAsync(It.IsAny<Guid>(), "modify"), Times.Never);
        _elasticsearchServiceMock.Verify(es => es.UpdatePermissionAsync(It.IsAny<Permission>()), Times.Never);
    }

    #endregion

    #region GetPermissionsAsync

    [Fact]
    public async Task GetPermissionsAsync_ShouldReturnList_AndPublishGet()
    {
        // Arrange
        var query = new GetPermissionsQuery();

        // Suppose repository returns a list of PermissionDto
        var expected = new List<PermissionDto>
        {
            new PermissionDto
            {
                EmployeeId = Guid.NewGuid(),
                EmployeeName = "John",
                Permissions = new List<PermissionTypeDto>
                {
                    new PermissionTypeDto { Id = Guid.NewGuid(), Name = "Vacation" }
                }
            }
        };

        // If your repository method returns IEnumerable<PermissionDto>, mock that
        _repositoryManagerMock
            .Setup(r => r.Permissions.GetAllByEmployeeAsync())
            .ReturnsAsync(expected);

        // Mock Kafka publish with "get"
        _kafkaServiceMock
            .Setup(k => k.PublishOperationAsync(It.IsAny<Guid>(), "get"))
            .Returns(Task.CompletedTask);

        // Mock Elasticsearch
        _elasticsearchServiceMock
            .Setup(es => es.GetPermissionsAsync())
            .ReturnsAsync(new List<ElasticsearchPermission>()); // or do nothing if you only test the call

        // Act
        var result = await _permissionService.GetPermissionsAsync(query);

        // Assert
        Assert.NotNull(result);
        var list = result.ToList();
        Assert.Single(list); // if you only expect one

        // Validate the item
        Assert.Equal(expected[0].EmployeeId, list[0].EmployeeId);
        Assert.Equal("John", list[0].EmployeeName);

        // Verify publish
        _kafkaServiceMock
            .Verify(k => k.PublishOperationAsync(It.IsAny<Guid>(), "get"), Times.Once);

        // Verify we called ES get
        _elasticsearchServiceMock
            .Verify(es => es.GetPermissionsAsync(), Times.Once);
    }

    #endregion
}
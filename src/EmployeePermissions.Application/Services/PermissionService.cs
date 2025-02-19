using EmployeePermissions.Application.Commands;
using EmployeePermissions.Application.DTOs;
using EmployeePermissions.Application.Interfaces;
using EmployeePermissions.Application.Queries;
using EmployeePermissions.Core.Entities;
using Microsoft.Extensions.Logging;

namespace EmployeePermissions.Application.Services;

// Uses the Unit of Work implementation of IRepositoryManager
public class PermissionService : IPermissionService
{
    private readonly ILogger<PermissionService> _logger;
    private readonly IRepositoryManager _repositoryManager;
    private readonly IKafkaService _kafkaService;
    private readonly IElasticsearchService _elasticsearchService;

    public PermissionService(
        ILogger<PermissionService> logger,
        IRepositoryManager repositoryManager,
        IKafkaService kafkaService,
        IElasticsearchService elasticsearchService)
    {
        _logger = logger;
        _repositoryManager = repositoryManager;
        _kafkaService = kafkaService;
        _elasticsearchService = elasticsearchService;
    }

    public async Task RequestPermissionAsync(RequestPermissionCommand command)
    {
        var permission = new Permission
        {
            EmployeeId = command.EmployeeId,
            PermissionTypeId = command.PermissionTypeId,
            GrantedDate = command.GrantedDate,
            Description = command.Description
        };

        await _repositoryManager.Permissions.AddAsync(permission);
        await _repositoryManager.SaveChangesAsync();
        _logger.LogInformation("Permission successfully added in the database. Permission details: {Permission}", permission);

        // Publish Kafka message (operation = "request")
        await _kafkaService.PublishOperationAsync(Guid.NewGuid(), "request");

        // Index in Elasticsearch
        await _elasticsearchService.IndexPermissionAsync(permission);
    }

    public async Task ModifyPermissionAsync(RequestPermissionCommand command)
    {
        var employeeId = command.EmployeeId;
        var permissionTypeId = command.PermissionTypeId;

        var permission =
            await _repositoryManager.Permissions.GetByIdAsync(employeeId, permissionTypeId);
        if (permission == null)
            throw new Exception("Permission not found.");

        permission.EmployeeId = employeeId;
        permission.PermissionTypeId = permissionTypeId;
        permission.GrantedDate = command.GrantedDate;
        permission.Description = command.Description;

        _repositoryManager.Permissions.Update(permission);
        await _repositoryManager.SaveChangesAsync();
        _logger.LogInformation("Permission successfully updated in the database. Permission details: {Permission}", permission);
        
        // Publish Kafka message (operation = "modify")
        await _kafkaService.PublishOperationAsync(Guid.NewGuid(), "modify");

        // Update in Elasticsearch
        await _elasticsearchService.UpdatePermissionAsync(permission);
    }

    public async Task<IEnumerable<PermissionDto>> GetPermissionsAsync(GetPermissionsQuery query)
    {
        var permissions = await _repositoryManager.Permissions.GetAllByEmployeeAsync();
        _logger.LogInformation("Permissions successfully fetched from the database.");

        // Publish Kafka message (operation = "get")
        await _kafkaService.PublishOperationAsync(Guid.NewGuid(), "get");
        
        // Get record from Elasticsearch
        var permissionsInElasticsearch = await _elasticsearchService.GetPermissionsAsync();

        return permissions;
    }
}
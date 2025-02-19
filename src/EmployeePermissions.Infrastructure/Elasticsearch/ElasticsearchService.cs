using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using EmployeePermissions.Application.DTOs;
using EmployeePermissions.Application.Interfaces;
using EmployeePermissions.Core.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EmployeePermissions.Infrastructure.Elasticsearch;

public class ElasticsearchService : IElasticsearchService
{
    private readonly ILogger<ElasticsearchService> _logger;
    private readonly string _indexName;
    private readonly ElasticsearchClient _client;

    public ElasticsearchService(ILogger<ElasticsearchService> logger, IOptions<ElasticsearchOptions> options)
    {
        _logger = logger;
        _indexName = options.Value.IndexName;

        var clientSettings = new ElasticsearchClientSettings(new Uri(options.Value.Uri))
            .DefaultIndex(_indexName);

        _client = new ElasticsearchClient(clientSettings);

        EnsureIndex().Wait();
    }

    public async Task IndexPermissionAsync(Permission permission)
    {
        try
        {
            var elasticPermission = new ElasticsearchPermission
            {
                EmployeeId = permission.EmployeeId,
                PermissionTypeId = permission.PermissionTypeId,
                GrantedDate = permission.GrantedDate,
                Description = permission.Description ?? string.Empty,
            };

            // Use composite ID when indexing
            _logger.LogInformation("[Elasticsearch] Indexing Elasticsearch data: Index={Index}", _indexName);
            var response = await _client.IndexAsync(
                elasticPermission,
                idx => idx.Index(_indexName)
                    .Id(GetDocumentId(permission.EmployeeId, permission.PermissionTypeId))
            );

            if (!response.IsValidResponse)
            {
                _logger.LogError(
                    "Error indexing permission for Employee {EmployeeId} and PermissionType {PermissionTypeId}: {Error}",
                    permission.EmployeeId,
                    permission.PermissionTypeId,
                    response.DebugInformation);
                throw new Exception($"Failed to index permission: {response.DebugInformation}");
            }
            
            _logger.LogInformation("[Elasticsearch] Elasticsearch data indexed!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error indexing permission for Employee {EmployeeId} and PermissionType {PermissionTypeId}",
                permission.EmployeeId,
                permission.PermissionTypeId);
            throw;
        }
    }

    public async Task UpdatePermissionAsync(Permission permission)
    {
        try
        {
            var elasticPermission = new ElasticsearchPermission
            {
                EmployeeId = permission.EmployeeId,
                PermissionTypeId = permission.PermissionTypeId,
                GrantedDate = permission.GrantedDate,
                Description = permission.Description ?? string.Empty
            };

            // Use composite ID when updating
            var documentId = GetDocumentId(permission.EmployeeId, permission.PermissionTypeId);
            _logger.LogInformation("[Elasticsearch] Updating Elasticsearch data: DocumentId={DocumentId}", documentId);
            var response = await _client.UpdateAsync<ElasticsearchPermission, ElasticsearchPermission>(
                _indexName,
                documentId,
                u => u.Doc(elasticPermission)
            );

            if (!response.IsValidResponse)
            {
                _logger.LogError(
                    "Error updating permission for Employee {EmployeeId} and PermissionType {PermissionTypeId}: {Error}",
                    permission.EmployeeId,
                    permission.PermissionTypeId,
                    response.DebugInformation);
                throw new Exception($"Failed to update permission: {response.DebugInformation}");
            }

            _logger.LogInformation("[Elasticsearch] Elasticsearch data updated!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating permission for Employee {EmployeeId} and PermissionType {PermissionTypeId}",
                permission.EmployeeId,
                permission.PermissionTypeId);
            throw;
        }
    }

    public async Task<IEnumerable<ElasticsearchPermission>> GetPermissionsAsync()
    {
        try
        {
            _logger.LogInformation("[Elasticsearch] Getting Elasticsearch data from Index={Index}", _indexName);
            var response = await _client.SearchAsync<ElasticsearchPermission>(s => s
                .Index(_indexName)
                .Query(q => q.MatchAll(new MatchAllQuery()))
            );

            if (!response.IsValidResponse)
            {
                _logger.LogError(
                    "Error retrieving permissions: {Error}",
                    response.DebugInformation);
                throw new Exception($"Failed to retrieve permissions: {response.DebugInformation}");
            }

            _logger.LogInformation("[Elasticsearch] Elasticsearch data: {Data}", response.Documents.ToString());
            return response.Documents;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permissions");
            throw;
        }
    }

    private string GetDocumentId(Guid employeeId, Guid permissionTypeId)
    {
        return $"{employeeId}_{permissionTypeId}";
    }

    private async Task EnsureIndex()
    {
        try
        {
            var existsResponse = await _client.Indices.ExistsAsync(_indexName);

            if (!existsResponse.Exists)
            {
                var createIndexResponse = await _client.Indices.CreateAsync(_indexName, i => i
                    .Mappings(m => m
                        .Properties<ElasticsearchPermission>(p => p
                            .Keyword(n => n.EmployeeId)
                            .Keyword(n => n.PermissionTypeId)
                            .Date(d => d.GrantedDate)
                            .Keyword(k => k.Description)
                        )
                    )
                );

                if (!createIndexResponse.IsValidResponse)
                {
                    _logger.LogError(
                        "Failed to create index: {Error}",
                        createIndexResponse.DebugInformation);
                    throw new Exception(
                        $"Failed to create Elasticsearch index: {createIndexResponse.DebugInformation}");
                }

                _logger.LogInformation("Successfully created index: {IndexName}", _indexName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring index existence");
            throw;
        }
    }
}
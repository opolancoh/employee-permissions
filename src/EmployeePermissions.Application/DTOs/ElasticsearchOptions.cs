namespace EmployeePermissions.Application.DTOs;

public class ElasticsearchOptions
{
    public string Uri { get; init; } = null!;
    public string IndexName { get; init; } = null!;
}
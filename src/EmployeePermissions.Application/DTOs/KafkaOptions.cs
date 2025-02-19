namespace EmployeePermissions.Application.DTOs;

public class KafkaOptions
{
    public string BootstrapServers { get; init; } = null!;
    public string Topic { get; init; } = null!;
}
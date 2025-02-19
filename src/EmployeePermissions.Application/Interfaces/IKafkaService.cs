namespace EmployeePermissions.Application.Interfaces;

public interface IKafkaService
{
    Task PublishOperationAsync(Guid id, string name);
}
using Microsoft.AspNetCore.Mvc;
using EmployeePermissions.Application.Commands;
using EmployeePermissions.Application.Interfaces;
using EmployeePermissions.Application.Queries;

namespace EmployeePermissions.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PermissionsController : ControllerBase
{
    private readonly ILogger<PermissionsController> _logger;
    private readonly IPermissionService _permissionService;

    public PermissionsController(ILogger<PermissionsController> logger, IPermissionService permissionService)
    {
        _logger = logger;
        _permissionService = permissionService;
    }

    [HttpPost]
    public async Task<IActionResult> RequestPermission([FromBody] RequestPermissionCommand command)
    {
        _logger.LogInformation("[RequestPermission] Starting RequestPermission operation");
        await _permissionService.RequestPermissionAsync(command);
        _logger.LogInformation("[RequestPermission] Completed RequestPermission operation");

        return NoContent();
    }

    [HttpPut]
    public async Task<IActionResult> ModifyPermission([FromBody] RequestPermissionCommand command)
    {
        _logger.LogInformation("[ModifyPermission] Starting ModifyPermission operation");
        await _permissionService.ModifyPermissionAsync(command);
        _logger.LogInformation("[ModifyPermission] Completed ModifyPermission operation");

        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> GetPermissions()
    {
        _logger.LogInformation("[GetPermissions] Starting GetPermissions operation");
        var query = new GetPermissionsQuery();
        var result = await _permissionService.GetPermissionsAsync(query);
        _logger.LogInformation("[GetPermissions] Completed GetPermissions operation");

        return Ok(result);
    }
}
using EmployeePermissions.Core.Entities;
using EmployeePermissions.Core.Interfaces;
using EmployeePermissions.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EmployeePermissions.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly ApplicationDbContext _context;
    
    public EmployeeRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Employee>> GetAllAsync()
    {
        return await _context.Employees.ToListAsync();
    }

    public async Task<Employee?> GetByIdAsync(Guid id)
    {
        return await _context.Employees.FindAsync(id);
    }
    
    public async Task AddAsync(Employee entity)
    {
        await _context.Employees.AddAsync(entity);
    }

    public void Update(Employee entity)
    {
        _context.Employees.Update(entity);
    }

    public void Delete(Guid id)
    {
        var entityToDelete = new Employee { Id = id };
        _context.Employees.Attach(entityToDelete);
        _context.Employees.Remove(entityToDelete);
    }
}
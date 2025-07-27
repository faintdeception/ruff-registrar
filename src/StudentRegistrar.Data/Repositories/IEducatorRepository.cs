using StudentRegistrar.Models;

namespace StudentRegistrar.Data.Repositories;

public interface IEducatorRepository
{
    Task<IEnumerable<Educator>> GetAllAsync();
    Task<Educator?> GetByIdAsync(Guid id);
    Task<IEnumerable<Educator>> GetByCourseIdAsync(Guid courseId);
    Task<IEnumerable<Educator>> GetUnassignedAsync();
    Task<IEnumerable<Educator>> GetActiveAsync();
    Task<Educator> CreateAsync(Educator educator);
    Task<Educator> UpdateAsync(Educator educator);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> DeactivateAsync(Guid id);
    Task<bool> ActivateAsync(Guid id);
}

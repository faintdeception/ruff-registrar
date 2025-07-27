using StudentRegistrar.Models;

namespace StudentRegistrar.Data.Repositories;

public interface ISemesterRepository
{
    Task<Semester?> GetByIdAsync(Guid id);
    Task<Semester?> GetByCodeAsync(string code);
    Task<Semester?> GetActiveAsync();
    Task<IEnumerable<Semester>> GetAllAsync();
    Task<Semester> CreateAsync(Semester semester);
    Task<Semester> UpdateAsync(Semester semester);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<Semester>> GetSemestersWithCoursesAsync();
    Task<bool> ExistsAsync(Guid id);
    Task<bool> CodeExistsAsync(string code);
    Task<Semester> SetActiveAsync(Guid id);
}

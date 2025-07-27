using StudentRegistrar.Models;

namespace StudentRegistrar.Data.Repositories;

public interface ICourseRepository
{
    Task<Course?> GetByIdAsync(Guid id);
    Task<Course?> GetByCodeAsync(string code, Guid semesterId);
    Task<IEnumerable<Course>> GetBySemesterAsync(Guid semesterId);
    Task<IEnumerable<Course>> GetByPeriodAsync(Guid semesterId, string periodCode);
    Task<IEnumerable<Course>> GetAllAsync();
    Task<Course> CreateAsync(Course course);
    Task<Course> UpdateAsync(Course course);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<Course>> GetCoursesWithEnrollmentsAsync(Guid? semesterId = null);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> CodeExistsAsync(string code, Guid semesterId);
    Task<int> GetEnrollmentCountAsync(Guid courseId);
    Task<int> GetAvailableSpotsAsync(Guid courseId);
}
